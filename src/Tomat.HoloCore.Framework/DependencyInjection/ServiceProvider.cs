using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tomat.HoloCore.Framework.DependencyInjection;

public class ServiceProvider : IServiceProvider {
    private readonly Dictionary<Type, object> services = new();
    private readonly IServiceProvider? parent;

    public ServiceProvider(IServiceProvider? parent = null) {
        this.parent = parent;
    }

    public bool TryGetService(Type type, [NotNullWhen(returnValue: true)] out object? service) {
        if (parent?.TryGetService(type, out service) ?? false)
            return true;

        if (services.ContainsKey(type)) {
            service = services[type];
            return true;
        }

        service = null;
        return false;
    }

    public bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service) where T : class {
        if (parent?.TryGetService(out service) ?? false)
            return true;

        var result = TryGetService(typeof(T), out var serviceObj);
        service = serviceObj as T;
        return result;
    }

    public void Register(object instance, Type type) {
        services[type] = instance;
    }
}
