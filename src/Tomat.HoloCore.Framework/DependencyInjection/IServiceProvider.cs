using System;
using System.Diagnostics.CodeAnalysis;

namespace Tomat.HoloCore.Framework.DependencyInjection;

public interface IServiceProvider {
    bool TryGetService(Type type, [NotNullWhen(returnValue: true)] out object? service);

    bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service) where T : class;

    void RegisterAs(object instance, Type type);

    void RegisterAs<T>(T instance, Type type) where T : class;
}

public static class ServiceProviderExtensions {
    public static object? GetService(this IServiceProvider provider, Type type) {
        provider.TryGetService(type, out var service);
        return service;
    }

    public static T? GetService<T>(this IServiceProvider provider) where T : class {
        provider.TryGetService<T>(out var service);
        return service;
    }

    public static object ExpectService(this IServiceProvider provider, Type type) {
        if (provider.TryGetService(type, out var service))
            return service;

        throw new InvalidOperationException($"Service of type {type} is not registered.");
    }

    public static T ExpectService<T>(this IServiceProvider provider) where T : class {
        if (provider.TryGetService<T>(out var service))
            return service;

        throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
    }

    public static void Register(this IServiceProvider provider, object instance) {
        provider.RegisterAs(instance, instance.GetType());
    }

    public static void Register<T>(this IServiceProvider provider, T instance) where T : class {
        provider.RegisterAs(instance, typeof(T));
    }
}
