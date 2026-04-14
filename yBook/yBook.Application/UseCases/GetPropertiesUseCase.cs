using yBook.Application.Ports;
using yBook.Domain.Entities;

namespace yBook.Application.UseCases;

public class GetPropertiesUseCase(IPropertyRepository propertyRepository)
{
    public Task<IReadOnlyList<Property>> ExecuteAsync()
        => propertyRepository.GetPropertiesAsync();
}
