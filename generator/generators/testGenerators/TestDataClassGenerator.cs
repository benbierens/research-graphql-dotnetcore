using System;
using System.Collections.Generic;
using System.Linq;

public class TestDataClassGenerator : BaseGenerator
{
    private int dummyInt;
    private float dummyFloat;
    private double dummyDouble;

    public TestDataClassGenerator(GeneratorConfig config)
        : base(config)
    {
        dummyInt = 10000;
        dummyFloat = 10000.0f;
        dummyDouble = 10000.0;
    }

    public void CreateTestDataClass()
    {
        var fm = StartTestUtilsFile("TestData");
        var cm = fm.AddClass("TestData");
        cm.AddUsing(Config.GenerateNamespace);

        foreach (var m in Models)
        {
            cm.AddProperty("Test" + m.Name)
                .IsType(m.Name)
                .NoInitializer()
                .Build();
        }

        cm.AddBlankLine();

        AddConstructor(cm);

        fm.Build();
    }

    private void AddConstructor(ClassMaker cm)
    {
        cm.AddClosure("public TestData()", liner =>
        {
            IterateModelsInDependencyOrder(m =>
            {
                InitializeModel(liner, m);
            });
        });
    }

    private void InitializeModel(Liner liner, GeneratorConfig.ModelConfig m)
    {
        var foreign = GetForeignProperties(m);
        liner.StartClosure("Test" + m.Name + " = new " + m.Name);
        liner.Add("Id = " + DummyId() + ",");
        foreach (var f in m.Fields)
        {
            liner.Add(f.Name + " = " + DummyForType(m, f.Type) + ",");
        }
        foreach (var f in foreign)
        {
            if (!f.IsSelfReference)
            {
                liner.Add(f.Name + " = Test" + f.Name + ",");
                liner.Add(f.WithId + " = Test" + f.Name + ".Id,");
            }
        }
        liner.EndClosure(";");
    }

    private string DummyId()
    {
        if (Config.IdType == "int") return DummyInt();
        if (Config.IdType == "string") return "\"" + Guid.NewGuid().ToString() + "\"";
        throw new Exception("Unknown ID type: " + Config.IdType);
    }

    private string DummyForType(GeneratorConfig.ModelConfig m, string type)
    {
        if (type == "int") return DummyInt();
        if (type == "float") return DummyFloat();
        if (type == "string") return DummyString(m, type);
        if (type == "double") return DummyDouble();
        if (type == "bool") return "true";
        throw new Exception("Unknown type: " + type);
    }

    private string DummyString(GeneratorConfig.ModelConfig m, string fieldName)
    {
        return "\"Test" + m.Name + "\"";
    }

    private string DummyInt()
    {
        return (++dummyInt).ToString();
    }

    private string DummyFloat()
    {
        dummyFloat += 0.1f;
        return dummyFloat.ToString();
    }

    private string DummyDouble()
    {
        dummyDouble += 0.1;
        return dummyDouble.ToString();
    }
}
