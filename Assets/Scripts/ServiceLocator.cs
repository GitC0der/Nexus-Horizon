using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static Dictionary<Type, MonoBehaviour> services = new Dictionary<Type, MonoBehaviour>();

    public static void RegisterService<T>(T service) where T : MonoBehaviour
    {
        Type type = typeof(T);
        if (!services.ContainsKey(type))
        {
            services.Add(type, service);
        }
    }

    public static T GetService<T>() where T : MonoBehaviour
    {
        Type type = typeof(T);
        if (services.ContainsKey(type))
        {
            return services[type] as T;
        }
        return null;
    }
}