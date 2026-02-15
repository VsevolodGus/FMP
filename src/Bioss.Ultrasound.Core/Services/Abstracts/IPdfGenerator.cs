using Bioss.Ultrasound.Core.Domain.Models;

namespace Bioss.Ultrasound.Core.Services.Abstracts;

public interface IPdfGenerator
{
    void GenerateToFile(string fileName, Record record);
}
