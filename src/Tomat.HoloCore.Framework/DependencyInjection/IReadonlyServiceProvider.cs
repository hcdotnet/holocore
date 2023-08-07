using System;
using System.Diagnostics.CodeAnalysis;

namespace Tomat.HoloCore.Framework.DependencyInjection;

public interface IReadonlyServiceProvider {
    bool TryGetService(Type type, [NotNullWhen(returnValue: true)] out object? service);

    bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service) where T : class;
}

public static class ReadonlyServiceProviderExtensions {
    public static object? GetService(this IReadonlyServiceProvider provider, Type type) {
        provider.TryGetService(type, out var service);
        return service;
    }

    public static T? GetService<T>(this IReadonlyServiceProvider provider) where T : class {
        provider.TryGetService<T>(out var service);
        return service;
    }

    public static object ExpectService(this IReadonlyServiceProvider provider, Type type) {
        if (provider.TryGetService(type, out var service))
            return service;

        throw new InvalidOperationException($"Service of type {type} is not registered.");
    }

    public static T ExpectService<T>(this IReadonlyServiceProvider provider) where T : class {
        if (provider.TryGetService<T>(out var service))
            return service;

        throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
    }
    
    public static IReadonlyServiceProvider AsReadonly(this IReadonlyServiceProvider provider) {
        return new ReadonlyServiceProvider(provider);
    }
}
