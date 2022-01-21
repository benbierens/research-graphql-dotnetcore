public class ConverterClassGenerator : BaseGenerator
{
    public ConverterClassGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void CreateConverterClass()
    {
        var fm = StartTestFile("Converters");
        var cm = fm.AddClass("Converters");
        cm.Modifiers.Clear();
        cm.Modifiers.Add("static");
        cm.AddUsing(Config.GenerateNamespace);

        foreach (var m in Models)
        {
            AddConvertMethods(cm, m);
        }

        fm.Build();
    }

    private void AddConvertMethods(ClassMaker cm, GeneratorConfig.ModelConfig model)
    {
        var inputTypes = GetInputTypeNames(model);
        var l = model.Name.FirstToLower();

        AddConvertMethod(cm, model, inputTypes.Create, Config.GraphQl.GqlMutationsCreateMethod);
        AddConvertMethod(cm, model, inputTypes.Update, Config.GraphQl.GqlMutationsUpdateMethod, model.Name + "Id = " + l + ".Id,");

        cm.AddClosure("public static " + inputTypes.Delete + " To" + Config.GraphQl.GqlMutationsDeleteMethod + "(this " + model.Name + " " + l + ")", liner =>
        {
            liner.StartClosure("return new " + inputTypes.Delete);
            liner.Add(model.Name + "Id = " + l + ".Id,");
            liner.EndClosure(";");
        });
    }

    private void AddConvertMethod(ClassMaker cm, GeneratorConfig.ModelConfig model, string inputType, string mutationMethod, string firstLine = "")
    {
        var l = model.Name.FirstToLower();
        cm.AddClosure("public static " + inputType + " To" + mutationMethod + "(this " + model.Name + " " + l + ")", liner =>
        {
            liner.StartClosure("return new " + inputType);
            if (!string.IsNullOrWhiteSpace(firstLine)) liner.Add(firstLine);
            foreach (var f in model.Fields)
            {
                liner.Add(f.Name + " = " + l + "." + f.Name + ",");
            }
            var foreignProperties = GetForeignProperties(model);
            foreach (var f in foreignProperties)
            {
                liner.Add(f.WithId + " = " + l + "." + f.WithId + ",");
            }
            liner.EndClosure(";");
        });
    }

}
