using System;
using System.Diagnostics.CodeAnalysis;

namespace Tomat.HoloCore.Framework.DependencyInjection;

public class ReadonlyServiceProvider : IReadonlyServiceProvider {
    private readonly IReadonlyServiceProvider provider;

    public ReadonlyServiceProvider(IReadonlyServiceProvider provider) {
        this.provider = provider;
    }

    public bool TryGetService(Type type, [NotNullWhen(returnValue: true)] out object? service) {
        return provider.TryGetService(type, out service);
    }

    public bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service) where T : class {
        return provider.TryGetService(out service);
    }
}
