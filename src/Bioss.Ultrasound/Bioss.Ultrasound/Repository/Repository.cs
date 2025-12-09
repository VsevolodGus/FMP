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
        private static readonly Record _mockRecord = new Record()
        {
            Id = 0,
            StartTime = DateTime.Now,
            StopTime = DateTime.Now.AddMinutes(40),
            DeviceSerialNumber = "mockSerialNumber",
            Biometric = new Biometric()
            {
                Comment = "asddsdas",
                Sugar = 5,
                Diastolic = 90,
                Pulse = 60,
                Systolic = 120,
                Temperature = 36,
                Id = 0,
                RecordId = 0
            }
        };
        public async Task<Record> Get(long id)
        {
            if(id == 0)
                return _mockRecord;

            var record = await _database.RecordsTable.FirstOrDefaultAsync(a => a.Id == id);
            await _database.Connection.GetChildrenAsync(record);
            return record.ToRecord();
        }

        public async Task<IEnumerable<Record>> RecordsAsync()
        {
            var entities = await _database.Connection.GetAllWithChildrenAsync<RecordEntity>();
            var records = entities.Select(a => a.ToRecord()).ToList();
            records.Add(_mockRecord);

            return records.OrderByDescending(a => a.StartTime);
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