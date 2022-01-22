public class QueryClassGenerator : BaseGenerator
{
    public QueryClassGenerator(GeneratorConfig config) : base(config)
    {
    }
    
    public void CreateQueryClasses()
    {
        var fm = StartTestUtilsFile("QueryClasses");
        CreateQueryDataClass(fm);

        foreach (var m in Models)
        {
            CreateQueryClassForModel(fm, m);
            CreateMutationResponseClassForModel(fm, m);
        }

        fm.Build();
    }

    private void CreateQueryClassForModel(FileMaker fm, GeneratorConfig.ModelConfig m)
    {
        var cm = fm.AddClass("All" + m.Name + "sQuery");
        cm.AddUsing("System.Collections.Generic");
        cm.AddProperty(m.Name)
            .IsListOfType(m.Name)
            .Build();
    }

    private void CreateMutationResponseClassForModel(FileMaker fm, GeneratorConfig.ModelConfig m)
    {
        var cm = fm.AddClass(Config.GraphQl.GqlMutationsCreateMethod + m.Name + "Response");
        cm.AddProperty(Config.GraphQl.GqlMutationsCreateMethod + m.Name)
            .IsType("MutationResponse")
            .Build();
    }

private void CreateQueryDataClass(FileMaker fm)
    {
        var cm = fm.AddClass("GqlData<T>");
        cm.AddUsing(Config.GenerateNamespace);

        cm.AddProperty("Data")
            .IsType("T")
            .IsNullable()
            .Build();

        var cm2 = fm.AddClass("MutationResponse");
        cm2.AddProperty("Id")
            .IsType(Config.IdType)
            .Build();
    }
}
