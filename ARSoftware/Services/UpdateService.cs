using System.Text.Json;

namespace ARSoftware.Services
{
    public class UpdateService
    {
        // ✅ COMMIT ID વગર ની FINAL LINK - હંમેશા Latest
        private const string UpdateUrl = "https://gist.githubusercontent.com/ajayrathod2121-cmyk/10fc31ea91e36a21225137e3718a453b/raw/update.json";

        public async Task CheckUpdateAsync(bool showLatestMsg = false)
        {
            try
            {
                string currentVer = AppInfo.Current.VersionString;
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(15);

                // Cache Bypass
                string urlWithCacheBuster = $"{UpdateUrl}?t={DateTime.Now.Ticks}";
                string json = await client.GetStringAsync(urlWithCacheBuster);

                if (string.IsNullOrWhiteSpace(json) || json.Contains("Sorry but the page"))
                {
                    if (showLatestMsg)
                        await Shell.Current.DisplayAlert("⚠", "Update Server પર Error છે, થોડી વાર પછી Try કરો!", "OK");
                    return;
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string latestVer = root.GetProperty("LatestVersion").GetString() ?? currentVer;
                string downloadUrl = root.TryGetProperty("DownloadUrl", out var dl) ? dl.GetString() ?? "" : "";
                string releaseNotes = root.TryGetProperty("ReleaseNotes", out var notes) ? notes.GetString() ?? "" : "નવું Update આવ્યું છે!";
                bool mandatory = root.TryGetProperty("Mandatory", out var man) && man.GetBoolean();

                if (IsNewVersionAvailable(currentVer, latestVer))
                {
                    string msg = $"{releaseNotes}\n\nતમારું Version: {currentVer}\nનવું Version: {latestVer}\n\nUpdate કરવું છે?";
                    string title = mandatory ? $"🔥 Mandatory Update {latestVer}!" : $"🔄 નવું Update {latestVer}!";

                    bool go = await Shell.Current.DisplayAlert(title, msg, "હા, Update કરો", "ના");
                    if (go && !string.IsNullOrEmpty(downloadUrl))
                    {
                        // Drive ને બદલે સીધી GitHub Release Link ખુલશે
                        await Launcher.OpenAsync(downloadUrl);
                    }
                }
                else if (showLatestMsg)
                {
                    await Shell.Current.DisplayAlert("✅ Latest Version", $"તમારું Software Latest Version ({currentVer}) પર છે!", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Check Error: {ex.Message}");
                if (showLatestMsg)
                    await Shell.Current.DisplayAlert("⚠", $"Update Check માં Error: {ex.Message}", "OK");
            }
        }

        private bool IsNewVersionAvailable(string current, string latest)
        {
            try
            {
                var cur = new Version(current);
                var lat = new Version(latest);
                return lat > cur;
            }
            catch
            {
                return current != latest;
            }
        }
    }
}