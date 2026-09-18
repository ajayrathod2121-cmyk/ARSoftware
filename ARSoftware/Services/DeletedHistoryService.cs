using SQLite;
using System.Text.Json;

namespace ARSoftware.Services
{
    // ✅ Table - Delete થયેલો Data અહીં Save થશે
    public class DeletedHistoryTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string TableName { get; set; } = ""; // Party, Javak, Aavak, Bill
        public int OriginalId { get; set; }
        public string DisplayName { get; set; } = ""; // દેખાડવા માટે - Party Name / Bill No
        public string DataJson { get; set; } = ""; // આખો Data JSON માં
        public string DeletedBy { get; set; } = "";
        public DateTime DeletedAt { get; set; } = DateTime.Now;
        public string Reason { get; set; } = "";
    }

    public class DeletedHistoryService
    {
        private SQLiteAsyncConnection _db;

        private async Task InitAsync()
        {
            if (_db != null) return;
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "arsoftware.db3");
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<DeletedHistoryTable>();
        }

        // ✅ જ્યારે તમે Delete કરો ત્યારે આ Call કરો - History માં Save થશે
        public async Task AddToHistoryAsync(string tableName, int originalId, object data, string displayName = "", string reason = "User Deleted")
        {
            await InitAsync();
            var history = new DeletedHistoryTable
            {
                TableName = tableName,
                OriginalId = originalId,
                DisplayName = displayName,
                DataJson = JsonSerializer.Serialize(data),
                DeletedAt = DateTime.Now,
                DeletedBy = Preferences.Get("BusinessName", "Admin"),
                Reason = reason
            };
            await _db.InsertAsync(history);
        }

        // ✅ બધો History મેળવો
        public async Task<List<DeletedHistoryTable>> GetHistoryAsync()
        {
            await InitAsync();
            return await _db.Table<DeletedHistoryTable>().OrderByDescending(x => x.DeletedAt).ToListAsync();
        }

        // ✅ પાછું લાવો - Restore
        public async Task<bool> RestoreAsync(int historyId)
        {
            await InitAsync();
            var item = await _db.Table<DeletedHistoryTable>().Where(x => x.Id == historyId).FirstOrDefaultAsync();
            if (item == null) return false;

            try
            {
                // Table પ્રમાણે Restore Logic - હમણાં ફક્ત History માંથી Delete કરીએ
                // તમે પછી Party / Bill Table માં પાછું Insert કરવાનો Code અહીં Add કરી શકો
                // ઉદા: var party = JsonSerializer.Deserialize<PartyModel>(item.DataJson);
                // await _db.InsertAsync(party);

                await _db.DeleteAsync(item); // History માંથી કાઢી નાખો - Restore થયું માનો
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ✅ કાયમ માટે Delete
        public async Task DeletePermanentAsync(int historyId)
        {
            await InitAsync();
            await _db.DeleteAsync<DeletedHistoryTable>(historyId);
        }

        // ✅ બધો History Clear
        public async Task ClearAllAsync()
        {
            await InitAsync();
            await _db.DeleteAllAsync<DeletedHistoryTable>();
        }
    }
}