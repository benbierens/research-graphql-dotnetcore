
public class Generator : BaseGenerator
{
    private readonly ProjectGenerator projectGenerator;
    private readonly DtoGenerator dtoGenerator;
    private readonly DatabaseGenerator databaseGenerator;
    private readonly GraphQlGenerator graphQlGenerator;

    public Generator(GeneratorConfig config)
        : base(config)
    {
        projectGenerator = new ProjectGenerator(config);
        dtoGenerator = new DtoGenerator(config);
        databaseGenerator = new DatabaseGenerator(config);
        graphQlGenerator = new GraphQlGenerator(config);
    }

    public void Generate()
    {
        MakeDirA();
        MakeDirA(Config.Output.SourceFolder);
        MakeDirA(Config.Output.TestFolder);

        projectGenerator.CreateDotNetProject();

        MakeSrcDir(Config.Output.GeneratedFolder);
        dtoGenerator.GenerateDtos();
        databaseGenerator.GenerateDbContext();
        graphQlGenerator.GenerateGraphQl();

        projectGenerator.ModifyDefaultFiles();
        databaseGenerator.CreateAndApplyInitialMigration();
        // generate docker file
        // generate tests?
    }
}
