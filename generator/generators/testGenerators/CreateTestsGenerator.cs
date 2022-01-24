public class CreateTestsGenerator : BaseGenerator
{
    public CreateTestsGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void CreateCreateTests()
    {
        var fm = StartTestFile("CreateTests");
        var cm = fm.AddClass("CreateTests");
        cm.AddUsing("NUnit.Framework");
        cm.AddUsing("System.Threading.Tasks");
        cm.AddInherrit("BaseGqlTest");
        cm.Modifiers.Clear();

        foreach (var m in Models)
        {
            AddCreateTest(cm, m);
        }

        fm.Build();
    }

    private void AddCreateTest(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        cm.AddLine("[Test]");
        cm.AddClosure("public async Task ShouldCreate" + m.Name + "()", liner =>
        {
            liner.Add("var entity = await CreateTest" + m.Name + "();");
            liner.AddBlankLine();
            foreach (var f in m.Fields)
            {
                liner.Add("Assert.That(entity." + f.Name + ", Is.EqualTo(TestData.Test" + m.Name + "." + f.Name + "), \"Created incorrect " + m.Name + "." + f.Name + "\");");
            }
            var foreignProperties = GetForeignProperties(m);
            foreach (var f in foreignProperties)
            {
                if (!f.IsSelfReference)
                {
                    liner.Add("Assert.That(entity." + f.WithId + ", Is.EqualTo(TestData.Test" + f.Type + ".Id), \"Created incorrect " + m.Name + "." + f.WithId + "\");");
                }
            }
        });
    }
}
