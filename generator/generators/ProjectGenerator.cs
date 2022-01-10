public class ProjectGenerator : BaseGenerator
{
    public ProjectGenerator(GeneratorConfig config)
        :base(config)
    {
    }

    public void CreateDotNetProject()
    {
        RunCommand("dotnet", "new", "webapi");

        foreach (var p in Config.Packages)
        {
            RunCommand("dotnet", "add", "package", p);
        }

        RunCommand("dotnet", "tool", "install", "--global", "dotnet-ef");
    }
}