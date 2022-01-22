
public class TestGenerator : BaseGenerator
{
    private readonly BaseGqlTestClassGenerator baseGqlTestClassGenerator;
    private readonly SubscriptionHandleClassGenerator subscriptionHandleClassGenerator;
    private readonly DockerControllerClassGenerator dockerControllerClassGenerator;
    private readonly QueryClassGenerator queryClassGenerator;
    private readonly TestDataClassGenerator testDataClassGenerator;
    private readonly ConverterClassGenerator converterClassGenerator;
    private readonly GqlClassGenerator gqlClassGenerator;

    public TestGenerator(GeneratorConfig config)
        : base(config)
    {
        baseGqlTestClassGenerator = new BaseGqlTestClassGenerator(config);
        subscriptionHandleClassGenerator = new SubscriptionHandleClassGenerator(config);
        dockerControllerClassGenerator = new DockerControllerClassGenerator(config);
        queryClassGenerator = new QueryClassGenerator(config);
        testDataClassGenerator = new TestDataClassGenerator(config);
        converterClassGenerator = new ConverterClassGenerator(config);
        gqlClassGenerator = new GqlClassGenerator(config);
    }

    public void GenerateTests()
    {
        MakeTestDir(Config.Tests.SubFolder);
        MakeTestDir(Config.Tests.SubFolder, Config.Tests.UtilsFolder);
        
        baseGqlTestClassGenerator.CreateBaseGqlTestClass();
        subscriptionHandleClassGenerator.CreateSubscriptionHandleClass();
        dockerControllerClassGenerator.CreateDockerControllerClass();
        queryClassGenerator.CreateQueryClasses();
        testDataClassGenerator.CreateTestDataClass();
        converterClassGenerator.CreateConverterClass();
        gqlClassGenerator.CreateGqlClass();
    }
}
