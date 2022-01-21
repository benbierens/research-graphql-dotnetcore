
public class BaseGqlTestClassGenerator : BaseGenerator
{
    public BaseGqlTestClassGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void CreateBaseGqlTestClass()
    {
        var fm = StartTestFile("BaseGqlTest");
        AddBaseGqlTestClass(fm.AddClass("BaseGqlTest"));
        AddDockerInitializer(fm.AddClass("DockerInitializer"));
        fm.Build();
    }

    private void AddBaseGqlTestClass(ClassMaker cm)
    {
        cm.AddUsing("NUnit.Framework");
        cm.AddUsing("System.Threading.Tasks");

        cm.AddAttribute("Category(\"" + Config.Tests.TestCategory + "\")");

        cm.AddLine("[SetUp]");
        cm.AddClosure("public void GqlSetUp()", liner =>
        {
            liner.Add("TestData = new TestData();");
        });

        cm.AddLine("[TearDown]");
        cm.AddClosure("public async Task GqlTearDown()", liner =>
        {
            liner.Add("await Gql.CloseActiveSubscriptionHandles();");
        });

        cm.AddProperty("TestData")
            .IsType("TestData")
            .Build();

        cm.AddProperty("Gql")
            .IsType("Gql")
            .Build();
    }

    private void AddDockerInitializer(ClassMaker cm)
    {
        cm.AddAttribute("SetUpFixture");
        cm.AddLine("private readonly DockerController docker = new DockerController();");
        cm.AddBlankLine();
        cm.AddLine("[OneTimeSetUp]");
        cm.AddClosure("public void OneTimeGqlSetUp()", liner =>
        {
            liner.Add("docker.Start();");
        });

        cm.AddLine("[OneTimeTearDown]");
        cm.AddClosure("public void OneTimeGqlTearDown()", liner =>
        {
            liner.Add("docker.Stop();");
        });
    }
}