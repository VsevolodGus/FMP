using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Mapping;
using Bioss.Ultrasound.Repository.Abstracts;
using SQLiteNetExtensionsAsync.Extensions;

namespace Bioss.Ultrasound.Repository
{
    public class Repository : IRepository
    {
        private readonly AppDatabase _database;

        public Repository(AppDatabase database)
        {
            _database = database;
        }

        public event EventHandler<long> NewItem;
        public event EventHandler<long> ItemDelated;

        public async Task<Record> Get(long id)
        {
            var record = await _database.RecordsTable.FirstOrDefaultAsync(a => a.Id == id);
            await _database.Connection.GetChildrenAsync(record);
            return record.ToRecord();
        }

        public async Task<IEnumerable<Record>> RecordsAsync()
        {
            var entities = await _database.Connection.GetAllWithChildrenAsync<RecordEntity>();
            return entities.Select(a => a.ToRecord())
                .OrderByDescending(a => a.StartTime);
        }

        public async Task InsertAsync(Record record)
        {
            var entity = record.ToEntity();
            await _database.Connection.InsertWithChildrenAsync(entity, true);
            NewItem?.Invoke(this, entity.Id);
        }

        public async Task DeleteAsync(Record record)
        {
            var entity = record.ToEntity();
            await _database.Connection.DeleteAsync(entity, true);
            ItemDelated?.Invoke(this, entity.Id);
        }

        public async Task InsertOrUpdateAsync(Biometric biometric)
        {
            var entry = biometric.ToEntity();

            if (biometric.Id == 0)
            {
                await _database.Connection.InsertAsync(entry);
                biometric.Id = entry.Id;
            }
            else
                await _database.Connection.UpdateAsync(entry);
        }
    }
}