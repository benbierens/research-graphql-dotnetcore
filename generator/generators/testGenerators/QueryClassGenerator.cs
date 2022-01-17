public class QueryClassGenerator : BaseGenerator
{
    public QueryClassGenerator(GeneratorConfig config) : base(config)
    {
    }
    
    public void CreateQueryClasses()
    {
        var fm = StartTestFile("QueryClasses");
        CreateQueryDataClass(fm);

        foreach (var m in Models)
        {
            CreateQueryClassForModel(fm, m);
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

    private void CreateQueryDataClass(FileMaker fm)
    {
        var cm = fm.AddClass("GqlData<T>");
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
