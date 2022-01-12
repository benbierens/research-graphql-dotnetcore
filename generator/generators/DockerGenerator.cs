using System.IO;

public class DockerGenerator : BaseGenerator
{
    private const string dockerFolder = "docker";

    public DockerGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void GenerateDockerFiles()
    {
        MakeDir(dockerFolder);

        WriteRawFile(Path.Combine(Config.Output.ProjectRoot, dockerFolder, "Dockerfile"), new [] {
            "FROM mcr.microsoft.com/dotnet/aspnet:5.0",
            "WORKDIR /app",
            "COPY src/bin/release/net5.0/publish/ ./",
            "ENTRYPOINT [\"dotnet\", \"src.dll\"]"
        });

        WriteRawFile(Path.Combine(Config.Output.ProjectRoot, "docker-compose.yml"), new [] {
            "version: '3'",
            "services:",
            "    graphql:",
            "        image: dotnet-graphql:development",
            "        build:",
            "            context: .",
            "            dockerfile: ./docker/Dockerfile",
            "        ports:",
            "            - \"80:80\""
        });
    }
}