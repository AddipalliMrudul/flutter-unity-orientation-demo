using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>> where TEnum:Enumeration<TEnum>
{
    private static readonly Dictionary<string, TEnum> Enumerations = CreateEnumerations();

    protected Enumeration(string name)
    {
        Name = name;
    }

    public readonly string Name = string.Empty;

    public static TEnum FromName(string name)
    {
        return Enumerations.TryGetValue(name, out TEnum enumeration)? enumeration : default ;
    }

    public bool Equals(Enumeration<TEnum> other)
    {
        if (other == null)
            return false;

        return GetType() == other.GetType() && Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        return obj is Enumeration<TEnum> && Equals(obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }


    private static Dictionary<string, TEnum> CreateEnumerations()
    {
        var enumerationType = typeof(TEnum);

        var fieldForType = enumerationType
                            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                            .Where(fieldInfo => enumerationType.IsAssignableFrom(fieldInfo.FieldType))
                            .Select(fieldInfo => (TEnum)fieldInfo.GetValue(default)!);

        return fieldForType.ToDictionary(x => x.Name);
    }
}
