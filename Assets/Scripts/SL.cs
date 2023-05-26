using System;
using System.Collections.Generic;
using UnityEngine;

public static class SL
{
    private static Dictionary<Type, MonoBehaviour> _services = new Dictionary<Type, MonoBehaviour>();

    public static void RegisterService<T>(T service) where T : MonoBehaviour
    {
        Type type = typeof(T);
        if (!_services.ContainsKey(type))
        {
            _services.Add(type, service);
        }
    }

    public static T Get<T>() where T : MonoBehaviour
    {
        Type type = typeof(T);
        if (_services.ContainsKey(type))
        {
            return _services[type] as T;
        }
        return null;
    }
}