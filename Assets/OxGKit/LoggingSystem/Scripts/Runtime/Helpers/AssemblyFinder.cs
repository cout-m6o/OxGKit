using System;
using System.Collections.Generic;
using System.Reflection;

namespace OxGKit.LoggingSystem
{
    public static class AssemblyFinder
    {
        public static List<Type> GetAssignableTypes(Type parentType)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                // Filter each assembly directly (same order and result as before,
                // without copying every type of every assembly into a temporary list first)
                var types = assembly.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (parentType.IsAssignableFrom(type))
                    {
                        if (type.Name == parentType.Name) continue;
                        result.Add(type);
                    }
                }
            }
            return result;
        }

        public static T GetAttribute<T>(Type type) where T : Attribute
        {
            return (T)type.GetCustomAttribute(typeof(T), false);
        }
    }
}
