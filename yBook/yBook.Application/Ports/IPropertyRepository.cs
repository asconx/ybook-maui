using yBook.Domain.Entities;

namespace yBook.Application.Ports;

public interface IPropertyRepository
{
    Task<IReadOnlyList<Property>> GetPropertiesAsync();
}
