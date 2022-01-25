using System.Collections.Generic;
using System.Linq;

public static class TypeUtils
{
    private class TypeInfo
    {
        public TypeInfo(string type, string defaultInitializer, string valueAccessor, string requiredUsing = "")
        {
            Type = type;
            DefaultInitializer = defaultInitializer;
            ValueAccessor = valueAccessor;
            RequiredUsing = requiredUsing;
        }

        public string Type { get; }
        public string DefaultInitializer { get; }
        public string ValueAccessor { get; }
        public string RequiredUsing { get; }
    }

    private static List<TypeInfo> types = new List<TypeInfo>
    {
        new TypeInfo("int", "", ".Value"),
        new TypeInfo("bool", "", ".Value"),
        new TypeInfo("string", " = \"\";", ""),
        new TypeInfo("float", "", ".Value"),
        new TypeInfo("double", "", ".Value"),
        new TypeInfo("DateTime", "", ".Value", "System")
    };

    public static bool IsNullableRequiredForType(string type)
    {
        return types.Any(t => t.Type == type);
    }

    public static string GetInitializerForType(string type)
    {
        return Get(type).DefaultInitializer;
    }

    public static string GetValueAccessor(string type)
    {
        return Get(type).ValueAccessor;
    }

    public static void AddTypeRequiredUsing(FileMaker fm, GeneratorConfig.ModelConfig m)
    {
        foreach (var f in m.Fields) AddTypeRequiredUsing(fm, f);
    }

    public static void AddTypeRequiredUsing(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        foreach (var f in m.Fields) AddTypeRequiredUsing(cm, f);
    }

    public static void AddTypeRequiredUsing(FileMaker fm, GeneratorConfig.ModelField f)
    {
        AddTypeRequiredUsing(fm, f.Type);
    }

    public static void AddTypeRequiredUsing(ClassMaker cm, GeneratorConfig.ModelField f)
    {
        AddTypeRequiredUsing(cm, f.Type);
    }

    public static void AddTypeRequiredUsing(FileMaker fm, string type)
    {
        fm.AddUsing(Get(type).RequiredUsing);
    }

    public static void AddTypeRequiredUsing(ClassMaker cm, string type)
    {
        cm.AddUsing(Get(type).RequiredUsing);
    }

    private static TypeInfo Get(string type)
    {
        return types.Single(t => t.Type == type);
    }
}
