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

        WriteRawFile(liner =>
        {
            liner.Add("# DotNet-GraphQL Backend");
            liner.Add("## Build Development:");
            liner.Add("`dotnet build " + src + "`");
            liner.Add("## Run:");
            liner.Add("`dotnet run -p " + src + "`");
            liner.Add("## Test:");
            liner.Add("`dotnet test " + test + "`");
            liner.Add("`dotnet test " + test + " --filter TestCategory!=" + Config.Tests.TestCategory);
            liner.Add("## Build Release & Run Docker Image:");
            liner.Add("`dotnet publish " + src + " -c release`");
            liner.Add("`docker-compose up -d`");
        }, "Readme.md");
    }
}
