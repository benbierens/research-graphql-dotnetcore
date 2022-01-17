
public class TestGenerator : BaseGenerator
{
    private readonly BaseGqlTestClassGenerator baseGqlTestClassGenerator;
    private readonly SubscriptionHandleClassGenerator subscriptionHandleClassGenerator;
    private readonly DockerControllerClassGenerator dockerControllerClassGenerator;
    private readonly QueryClassGenerator queryClassGenerator;

    public TestGenerator(GeneratorConfig config)
        : base(config)
    {
        baseGqlTestClassGenerator = new BaseGqlTestClassGenerator(config);
        subscriptionHandleClassGenerator = new SubscriptionHandleClassGenerator(config);
        dockerControllerClassGenerator = new DockerControllerClassGenerator(config);
        queryClassGenerator = new QueryClassGenerator(config);
    }

    public void GenerateTests()
    {
        MakeTestDir(Config.Tests.SubFolder);
        baseGqlTestClassGenerator.CreateBaseGqlTestClass();
        subscriptionHandleClassGenerator.CreateSubscriptionHandleClass();
        dockerControllerClassGenerator.CreateDockerControllerClass();
        queryClassGenerator.CreateQueryClasses();
    }
}
