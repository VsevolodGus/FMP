using Bioss.Ultrasound.Core.Domain.Models;

namespace Bioss.Ultrasound.Core.Repository.Abstracts;

public interface IRepository
{
    //  TODO; какая то фигня - не должно здесь быть
    event EventHandler<long> NewItem;
    event EventHandler<long> ItemDelated;

    Task<IEnumerable<Record>> RecordsAsync();
    Task<Record> Get(long id);
    Task InsertAsync(Record record);
    Task DeleteAsync(Record record);
    Task InsertOrUpdateAsync(Biometric biometric);
}
