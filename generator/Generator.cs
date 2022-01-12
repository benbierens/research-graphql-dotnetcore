
public class Generator : BaseGenerator
{
    private readonly ProjectGenerator projectGenerator;
    private readonly DtoGenerator dtoGenerator;
    private readonly DatabaseGenerator databaseGenerator;
    private readonly GraphQlGenerator graphQlGenerator;
    private readonly DockerGenerator dockerGenerator;
    private readonly ReadmeGenerator readmeGenerator;

    public Generator(GeneratorConfig config)
        : base(config)
    {
        projectGenerator = new ProjectGenerator(config);
        dtoGenerator = new DtoGenerator(config);
        databaseGenerator = new DatabaseGenerator(config);
        graphQlGenerator = new GraphQlGenerator(config);
        dockerGenerator = new DockerGenerator(config);
        readmeGenerator = new ReadmeGenerator(config);
    }

    public void Generate()
    {
        MakeDir();
        MakeDir(Config.Output.SourceFolder);
        MakeDir(Config.Output.TestFolder);

        projectGenerator.CreateDotNetProject();

        MakeSrcDir(Config.Output.GeneratedFolder);
        dtoGenerator.GenerateDtos();
        databaseGenerator.GenerateDbContext();
        graphQlGenerator.GenerateGraphQl();

        projectGenerator.ModifyDefaultFiles();
        databaseGenerator.CreateInitialMigration();

        dockerGenerator.GenerateDockerFiles();
        
        // generate tests?
        
        readmeGenerator.GenerateReadme();
    }
}
