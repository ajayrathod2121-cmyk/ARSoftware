using SQLite;
using ARSoftware.Models;

namespace ARSoftware.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public async Task Init()
        {
            if (_database != null) return;

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "ar_software.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<UserModel>();

            // Demo user - પહેલી વખત
            var count = await _database.Table<UserModel>().CountAsync();
            if (count == 0)
            {
                await _database.InsertAsync(new UserModel
                {
                    Username = "admin",
                    Password = "admin123",
                    Mobile = "9876543210",
                    BusinessName = "AR Software"
                });
            }
        }

        // ✅ 1. જૂનો Password સાચો છે કે નહીં Check કરવા - Change Password માં કામ આવશે
        public async Task<bool> VerifyPasswordAsync(string username, string oldPassword)
        {
            await Init();
            var user = await _database.Table<UserModel>().FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return false;
            return user.Password == oldPassword;
        }

        // ✅ 2. Username થી Password Update - Change Password Page માટે
        public async Task<bool> UpdatePasswordAsync(string username, string newPassword)
        {
            await Init();
            var user = await _database.Table<UserModel>().FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return false;

            user.Password = newPassword;
            await _database.UpdateAsync(user);
            return true;
        }

        // ✅ 3. Mobile થી Password Update - Forgot Password OTP માટે
        public async Task<bool> UpdatePasswordByMobileAsync(string mobile, string newPassword)
        {
            await Init();
            var user = await _database.Table<UserModel>().FirstOrDefaultAsync(u => u.Mobile == mobile);
            if (user == null) return false;

            user.Password = newPassword;
            await _database.UpdateAsync(user);
            return true;
        }

        public async Task<UserModel> GetUserByUsernameAsync(string username)
        {
            await Init();
            return await _database.Table<UserModel>().FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<UserModel> GetUserByMobileAsync(string mobile)
        {
            await Init();
            return await _database.Table<UserModel>().FirstOrDefaultAsync(u => u.Mobile == mobile);
        }
    }
}