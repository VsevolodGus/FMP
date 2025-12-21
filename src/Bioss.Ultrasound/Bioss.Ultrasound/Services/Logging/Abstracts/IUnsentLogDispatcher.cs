using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Logging.Abstracts
{
    public interface IUnsentLogDispatcher
    {
        Task SendAllUnsentAsync();
    }
}
