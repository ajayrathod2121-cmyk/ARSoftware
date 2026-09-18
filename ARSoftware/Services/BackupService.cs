using System.Text.Json;

namespace ARSoftware.Services
{
    public class BackupService
    {
        public async Task<string> BackupNowAsync()
        {
            try
            {
                // 1. DB Path શોધો - બધા Possible નામ Check કરો
                string appData = FileSystem.AppDataDirectory;
                string[] possibleDbNames = { "arsoftware.db3", "ar_software.db3", "vyapar.db3", "app.db3" };
                string dbPath = "";

                foreach (var name in possibleDbNames)
                {
                    var p = Path.Combine(appData, name);
                    if (File.Exists(p)) { dbPath = p; break; }
                }
                // જો એક પણ ન મળે તો Default
                if (string.IsNullOrEmpty(dbPath))
                    dbPath = Path.Combine(appData, "arsoftware.db3");

                // 2. Backup Root - Windows માં D:\ અને Mobile માં AppData/Backups
                string backupRoot;
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    backupRoot = @"D:\ARSoftwareBackup";
                    try { Directory.CreateDirectory(backupRoot); }
                    catch
                    {
                        // D:\ ન મળે તો Documents માં
                        backupRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ARSoftwareBackup");
                    }
                }
                else
                {
                    backupRoot = Path.Combine(appData, "Backups");
                }
                Directory.CreateDirectory(backupRoot);

                // 3. File Name
                string fileName = $"Backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db3";
                string destPath = Path.Combine(backupRoot, fileName);

                // 4. DB Copy - Async Safe Copy
                if (File.Exists(dbPath))
                {
                    using var source = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var dest = new FileStream(destPath, FileMode.Create, FileAccess.Write);
                    await source.CopyToAsync(dest);
                }
                else
                {
                    // DB ના હોય તો પણ Info File બનાવો
                    var info = new
                    {
                        Message = "DB File Not Found - Demo Backup",
                        CreatedAt = DateTime.Now,
                        Business = BusinessService.GetBusiness(),
                        AppDataPath = appData
                    };
                    string json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(destPath + ".json", json);
                    await File.WriteAllTextAsync(destPath, $"Backup Created at {DateTime.Now} - DB not found at {dbPath}");
                }

                // 5. Business Details નો JSON Backup - જેથી બધી PDF નું નામ Restore થાય!
                try
                {
                    var biz = BusinessService.GetBusiness();
                    string bizJson = JsonSerializer.Serialize(biz, new JsonSerializerOptions { WriteIndented = true });
                    string bizBackupPath = Path.Combine(backupRoot, $"Business_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");
                    await File.WriteAllTextAsync(bizBackupPath, bizJson);
                }
                catch { /* Ignore Business Backup Error */ }

                // 6. 15 દિવસ જૂના Backup Auto Delete
                try
                {
                    foreach (var f in Directory.GetFiles(backupRoot).Where(f => File.GetCreationTime(f) < DateTime.Now.AddDays(-15)))
                    {
                        File.Delete(f);
                    }
                }
                catch { /* Delete Error Ignore */ }

                return destPath;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // ✅ Extra - Business Data ને અલગ Backup
        public async Task<string> BackupBusinessDataAsync()
        {
            try
            {
                var biz = BusinessService.GetBusiness();
                string json = JsonSerializer.Serialize(biz, new JsonSerializerOptions { WriteIndented = true });
                string backupRoot = DeviceInfo.Platform == DevicePlatform.WinUI
                    ? @"D:\ARSoftwareBackup"
                    : Path.Combine(FileSystem.AppDataDirectory, "Backups");

                Directory.CreateDirectory(backupRoot);
                string path = Path.Combine(backupRoot, $"Business_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                await File.WriteAllTextAsync(path, json);
                return path;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}