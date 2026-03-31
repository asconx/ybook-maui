using Microsoft.Extensions.DependencyInjection;
using yBook.Services;

namespace yBook.Views.Ustawienia;

public partial class PokojePage
{
    // Parameterless constructor so Shell DataTemplate (XAML) can instantiate this page
    public PokojePage()
        : this(IPlatformApplication.Current?.Services.GetService<IAuthService>() ?? throw new InvalidOperationException("IAuthService not registered"))
    {
    }
}
