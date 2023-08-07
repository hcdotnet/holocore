using System;

namespace Tomat.HoloCore.Framework.DependencyInjection;

public interface IServiceProvider : IReadonlyServiceProvider {
    void RegisterAs(object instance, Type type);

    void RegisterAs<T>(T instance, Type type) where T : class;
}

public static class ServiceProviderExtensions {
    public static void Register(this IServiceProvider provider, object instance) {
        provider.RegisterAs(instance, instance.GetType());
    }

    public static void Register<T>(this IServiceProvider provider, T instance) where T : class {
        provider.RegisterAs(instance, typeof(T));
    }
}
