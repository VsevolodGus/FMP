using Bioss.Ultrasound.Domain.Models;

namespace Bioss.Ultrasound.Services.Abstracts
{
    public interface IPdfGenerator
    {
        void GenerateToFile(string fileName, Record record);
    }
}
