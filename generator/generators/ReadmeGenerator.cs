using System.IO;

public class ReadmeGenerator : BaseGenerator
{
    public ReadmeGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void GenerateReadme()
    {
        var src = Config.Output.SourceFolder;
        var test = Config.Output.TestFolder;

        WriteRawFile(Path.Combine(Config.Output.ProjectRoot, "Readme.md"), new [] {
            "# DotNet-GraphQL Backend",
            "## Build Development:",
            "`dotnet build " + src + "`",
            "## Run:",
            "`dotnet run -p " + src + "`",
            "## Test:",
            "`dotnet test " + test + "`",
            "## Build Release & Run Docker Image:",
            "`dotnet publish " + src + " -c release`",
            "`docker-compose up -d`"
        });
    }
}
