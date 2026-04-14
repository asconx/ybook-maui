using System;
using Microsoft.Extensions.DependencyInjection;

namespace yBook.Services
{
    // Simple service locator to resolve services from places where constructor injection is not possible
    public static class ServiceLocator
    {
        public static IServiceProvider? ServiceProvider { get; set; }

        public static T? Get<T>() where T : class
        {
            return ServiceProvider?.GetService<T>();
        }
    }
}
