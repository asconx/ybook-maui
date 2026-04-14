using yBook.Models;

namespace yBook.Services;

public interface IPriceService
{
    Task<List<CennikItem>> FetchPriceModifiersAsync();
}

