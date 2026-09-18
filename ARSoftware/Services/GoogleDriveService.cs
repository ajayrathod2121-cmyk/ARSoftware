using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ARSoftware.Services
{
    public class GoogleDriveService
    {
        private const string FolderName = "ARSoftwareBackup";
        private const string CLIENT_ID = "YOUR_CLIENT_ID.apps.googleusercontent.com";
        private const string CLIENT_SECRET = "YOUR_CLIENT_SECRET";
        private readonly string[] Scopes = { "https://www.googleapis.com/auth/drive" };

        public async Task<bool> ConnectGoogleDriveAsync()
        {
            try
            {
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets { ClientId = CLIENT_ID, ClientSecret = CLIENT_SECRET },
                    Scopes, "user", CancellationToken.None,
                    new FileDataStore(Path.Combine(FileSystem.AppDataDirectory, "GoogleAuth"), true));

                Preferences.Set("GoogleDriveConnected", true);
                Preferences.Set("GoogleDriveEmail", credential.UserId ?? "Connected");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = CLIENT_ID, ClientSecret = CLIENT_SECRET },
                Scopes, "user", CancellationToken.None,
                new FileDataStore(Path.Combine(FileSystem.AppDataDirectory, "GoogleAuth"), true));

            if (credential.Token.IsExpired(credential.Flow.Clock))
                await credential.RefreshTokenAsync(CancellationToken.None);

            return credential.Token.AccessToken;
        }

        // ✅ દર 1 કલાકે સીધું Drive માં - REST API થી - કોઈ Drive.v3 Package નહીં!
        public async Task<string> AutoBackupToDriveAsync()
        {
            string localDb = Path.Combine(FileSystem.AppDataDirectory, "arsoftware.db3");
            if (!File.Exists(localDb))
            {
                // જો db ન મળે તો AppData નો બેકઅપ
                localDb = Path.Combine(FileSystem.AppDataDirectory, "arsoftware.db3");
                if (!File.Exists(localDb)) return "No DB";
            }

            string token = await GetAccessTokenAsync();
            string folderId = await GetOrCreateFolderAsync(token);

            // File Upload
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var fileName = $"AR_Backup_{DateTime.Now:dd-MM-yyyy_HH-mm}.db3";

            using var form = new MultipartFormDataContent();
            var metadata = new { name = fileName, parents = new[] { folderId } };
            var json = JsonSerializer.Serialize(metadata);
            form.Add(new StringContent(json, System.Text.Encoding.UTF8, "application/json"), "metadata");
            form.Add(new StreamContent(File.OpenRead(localDb)), "file", fileName);

            var response = await client.PostAsync("https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart", form);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Preferences.Set("LastDriveBackup", DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
                await DeleteOldBackupsAsync(token, folderId); // 30 દિવસ વાળું
                return "Success";
            }
            return $"Failed: {result}";
        }

        private async Task<string> GetOrCreateFolderAsync(string token)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Search Folder
            var searchUrl = $"https://www.googleapis.com/drive/v3/files?q=mimeType='application/vnd.google-apps.folder' and name='{FolderName}' and trashed=false";
            var searchRes = await client.GetStringAsync(searchUrl);
            using var doc = JsonDocument.Parse(searchRes);
            var files = doc.RootElement.GetProperty("files");
            if (files.GetArrayLength() > 0)
            {
                return files[0].GetProperty("id").GetString();
            }

            // Create Folder
            var metadata = new { name = FolderName, mimeType = "application/vnd.google-apps.folder" };
            var content = new StringContent(JsonSerializer.Serialize(metadata), System.Text.Encoding.UTF8, "application/json");
            var createRes = await client.PostAsync("https://www.googleapis.com/drive/v3/files", content);
            var createJson = await createRes.Content.ReadAsStringAsync();
            using var createDoc = JsonDocument.Parse(createJson);
            return createDoc.RootElement.GetProperty("id").GetString();
        }

        private async Task DeleteOldBackupsAsync(string token, string folderId)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"https://www.googleapis.com/drive/v3/files?q='{folderId}' in parents and trashed=false&fields=files(id,createdTime)&orderBy=createdTime asc";
                var res = await client.GetStringAsync(url);
                using var doc = JsonDocument.Parse(res);
                var files = doc.RootElement.GetProperty("files");

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                foreach (var file in files.EnumerateArray())
                {
                    var created = file.GetProperty("createdTime").GetDateTime();
                    if (created < thirtyDaysAgo)
                    {
                        var fileId = file.GetProperty("id").GetString();
                        await client.DeleteAsync($"https://www.googleapis.com/drive/v3/files/{fileId}");
                    }
                }
            }
            catch { }
        }

        public async Task<bool> SyncFromDriveAsync()
        {
            try
            {
                string token = await GetAccessTokenAsync();
                string folderId = await GetOrCreateFolderAsync(token);
                return true;
            }
            catch { return false; }
        }
    }
}