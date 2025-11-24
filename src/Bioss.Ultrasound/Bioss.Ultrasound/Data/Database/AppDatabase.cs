using System.Collections.Generic;
using System.Threading.Tasks;
using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Data.Database.Migrations;
using SQLite;
using SQLiteMigrations;

namespace Bioss.Ultrasound.Data.Database
{
    public class AppDatabase : SQLiteAsyncDatabase
    {
        private const int DB_VERSION = 3;

        public AppDatabase(string path) : base(DB_VERSION, path)
        {
            DbPath = path;
        }

        public string DbPath { get; }

        public AsyncTableQuery<RecordEntity> RecordsTable => Connection.Table<RecordEntity>();
        public AsyncTableQuery<DataEntity> DataTable => Connection.Table<DataEntity>();
        public AsyncTableQuery<EventEntity> EventTable => Connection.Table<EventEntity>();
        public AsyncTableQuery<AudioEntity> AudioTable => Connection.Table<AudioEntity>();
        public AsyncTableQuery<BiometricEntity> BiometricTable => Connection.Table<BiometricEntity>();

        public override async Task CreateAsync()
        {
            await Connection.CreateTableAsync<RecordEntity>();
            await Connection.CreateTableAsync<DataEntity>();
            await Connection.CreateTableAsync<EventEntity>();
            await Connection.CreateTableAsync<AudioEntity>();
            await Connection.CreateTableAsync<BiometricEntity>();
        }

        public override List<IMigration> Migrations()
        {
            return new List<IMigration>
            {
                new Migration1To2(),
                new Migration2To3()
            };
        }
    }
}
