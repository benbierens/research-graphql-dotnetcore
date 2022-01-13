using System.Collections.Generic;
using System.Linq;

public static class Nullability
{
    private class NullabilityType
    {
        public NullabilityType(string type, string defaultInitializer, string valueAccessor)
        {
            Type = type;
            DefaultInitializer = defaultInitializer;
            ValueAccessor = valueAccessor;
        }

        public string Type { get; }
        public string DefaultInitializer { get; }
        public string ValueAccessor { get; }
    }

    private static List<NullabilityType> types = new List<NullabilityType>
    {
        new NullabilityType("int", "", ".Value"),
        new NullabilityType("bool", "", ".Value"),
        new NullabilityType("string", " = \"\";", ""),
        new NullabilityType("float", "", ".Value"),
        new NullabilityType("double", "", ".Value")
    };

    public static bool IsNullableRequiredForType(string type)
    {
        return types.Any(t => t.Type == type);
    }

    public static string GetInitializerForType(string type)
    {
        return types.Single(t => t.Type == type).DefaultInitializer;
    }

    public static string GetValueAccessor(string type)
    {
        return types.Single(t => t.Type == type).ValueAccessor;
    }
}
