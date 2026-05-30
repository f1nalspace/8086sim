using System;
using System.Collections.Generic;

// Kompatibilitaets-Shim: bietet exakt die genutzte DevExpress.Mvvm-Service-API an,
// intern eine schlanke Eigenimplementierung. Keine DevExpress-Binaerabhaengigkeit.
namespace Final.CPU8086.Mvvm;

public interface IServiceContainer
{
    void RegisterService(object service);
    void RegisterService(string key, object service);
    void UnregisterService(object service);
    T GetService<T>() where T : class;
    T GetService<T>(string key) where T : class;
}

public interface ISupportServices
{
    IServiceContainer ServiceContainer { get; }
}

public class ServiceContainer : IServiceContainer
{
    private readonly List<object> _services = new List<object>();
    private readonly Dictionary<string, object> _keyedServices = new Dictionary<string, object>(StringComparer.Ordinal);

    public void RegisterService(object service)
    {
        if (service == null)
            return;
        if (!_services.Contains(service))
            _services.Add(service);
    }

    public void RegisterService(string key, object service)
    {
        if (string.IsNullOrEmpty(key))
        {
            RegisterService(service);
            return;
        }
        if (service == null)
            return;
        _keyedServices[key] = service;
    }

    public void UnregisterService(object service)
    {
        if (service == null)
            return;
        _services.Remove(service);

        List<string> staleKeys = null;
        foreach (KeyValuePair<string, object> kv in _keyedServices)
        {
            if (ReferenceEquals(kv.Value, service))
                (staleKeys ??= new List<string>()).Add(kv.Key);
        }
        if (staleKeys != null)
        {
            foreach (string key in staleKeys)
                _keyedServices.Remove(key);
        }
    }

    public T GetService<T>() where T : class
    {
        foreach (object service in _services)
        {
            if (service is T match)
                return match;
        }
        foreach (KeyValuePair<string, object> kv in _keyedServices)
        {
            if (kv.Value is T match)
                return match;
        }
        return null;
    }

    public T GetService<T>(string key) where T : class
    {
        if (string.IsNullOrEmpty(key))
            return GetService<T>();
        if (_keyedServices.TryGetValue(key, out object service) && service is T match)
            return match;
        return null;
    }
}