using Bioss.Ultrasound.Core.Data.Database;
using Bioss.Ultrasound.Core.Data.Database.Entities;
using Bioss.Ultrasound.Core.Domain.Models;
using Bioss.Ultrasound.Core.Mapping;
using Bioss.Ultrasound.Core.Repository.Abstracts;
using Bioss.Ultrasound.Core.Services.Logging.Abstracts;
using SQLiteNetExtensionsAsync.Extensions;

namespace Bioss.Ultrasound.Core.Repository;

public class Repository : IRepository
{
    private readonly AppDatabase _database;
    private readonly ILogger _logger;

    public Repository(
        ILogger logger,
        AppDatabase database)
    {
        _logger = logger;
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
        var records = entities.Select(a => a.ToRecord()).OrderByDescending(a => a.StartTime);
        return records;
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

        await _database.Connection.DeleteAsync(entity);
        ItemDelated?.Invoke(this, entity.Id);

        try
        {
            // запускаем удаление тяжеловесов
            var deleteData = entity.Datas.Select(c => _database.Connection.DeleteAsync(c)).ToList();

            var listTask = new List<Task>(entity.Events.Length + 2);
            if (entity.Audio is not null)
                listTask.Add(_database.Connection.DeleteAsync(entity.Audio));

            if (entity.Biometric is not null)
                listTask.Add(_database.Connection.DeleteAsync(entity.Biometric));

            if (entity.Events is not null && entity.Events.Length > 0)
            {
                listTask.AddRange(
                    entity.Events.Select(c => _database.Connection.DeleteAsync(c))
                );
            }

            // удаляем легко весные объекты
            await Task.WhenAll(listTask);

            // TODO если пользователь закроет приложение до завершения deleteData, то будет нарушена консистентность данных и в телефоне будут храниться мертвые данные.
            // их надо как-то подчищать, лучще при запуске приложения запускать джобу

            // ждем удаление тяжеловесов, если что удалится в фоне потом
            deleteData.Add(
                Task.Delay(TimeSpan.FromSeconds(30).Milliseconds)
                .ContinueWith(t => 0)
               );
            await Task.WhenAny(deleteData);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error additional data record: {ex}");
        }
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