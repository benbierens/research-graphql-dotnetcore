public class DtoGenerator : BaseGenerator
{
    public DtoGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void GenerateDtos()
    {
        MakeSrcDir(Config.Output.GeneratedFolder, Config.Output.DtoSubFolder);

        foreach (var model in Models)
        {
            var fm = StartSrcFile(Config.Output.DtoSubFolder, model.Name);
            var cm = StartClass(fm, model.Name);

            cm.AddUsing("System.Collections.Generic");

            cm.AddProperty("Id")
                .IsType(Config.IdType)
                .Build();

            AddModelFields(cm, model);

            foreach (var m in model.HasMany)
            {
                cm.AddProperty(m)
                    .WithModifier("virtual")
                    .IsListOfType(m)
                    .Build();
            }
            AddForeignProperties(cm, model);

            cm.AddBlankLine();
            AddConvertMethods(cm, model);

            fm.Build();
        }
    }

    private void AddConvertMethods(ClassMaker cm, GeneratorConfig.ModelConfig model)
    {
        var inputTypes = GetInputTypeNames(model);

        AddConvertMethod(cm, model, inputTypes.Create, Config.GraphQl.GqlMutationsCreateMethod);
        AddConvertMethod(cm, model, inputTypes.Update, Config.GraphQl.GqlMutationsUpdateMethod, model.Name + "Id = Id,");

        cm.AddClosure("public " + inputTypes.Delete + " To" + Config.GraphQl.GqlMutationsDeleteMethod + "()", liner =>
        {
            liner.StartClosure("return new " + inputTypes.Delete);
            liner.Add(model.Name + "Id = Id,");
            liner.EndClosure(";");
        });
    }

    private void AddConvertMethod(ClassMaker cm, GeneratorConfig.ModelConfig model, string inputType, string mutationMethod, string firstLine = "")
    {
        cm.AddClosure("public " + inputType + " To" + mutationMethod + "()", liner =>
        {
            liner.StartClosure("return new " + inputType);
            if (!string.IsNullOrWhiteSpace(firstLine)) liner.Add(firstLine);
            foreach (var f in model.Fields)
            {
                liner.Add(f.Name + " = " + f.Name + ",");
            }
            var foreignProperties = GetForeignProperties(model);
            foreach (var f in foreignProperties)
            {
                liner.Add(f.WithId + " = " + f.WithId + ",");
            }
            liner.EndClosure(";");
        });
    }

    private void AddForeignProperties(ClassMaker cm, GeneratorConfig.ModelConfig model)
    {
        var foreignProperties = GetForeignProperties(model);
        foreach (var f in foreignProperties)
        {
            if (f.IsSelfReference)
            {
                AddNullableForeignProperties(cm, f);
            }
            else
            {
                AddExplicitForeignProperties(cm, f);
            }
        }
    }

    private void AddExplicitForeignProperties(ClassMaker cm, ForeignProperty f)
    {
        cm.AddProperty(f.WithId)
            .IsType(Config.IdType)
            .Build();

        cm.AddProperty(f.Name)
            .WithModifier("virtual")
            .IsType(f.Type)
            .InitializeAsExplicitNull()
            .Build();
    }

    private void AddNullableForeignProperties(ClassMaker cm, ForeignProperty f)
    {
        cm.AddProperty(f.WithId)
            .IsType(Config.IdType)
            .IsNullable()
            .Build();

        cm.AddProperty(f.Name)
            .WithModifier("virtual")
            .IsType(f.Type)
            .IsNullable()
            .Build();
    }
}