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

        var dockerDb = Config.Database.Docker;
        WriteRawFile(Path.Combine(Config.Output.ProjectRoot, "docker-compose.yml"), new [] {
            "version: '3'",
            "services:",
            "    " + Config.Database.DbContainerName + ":",
            "        container_name: " + Config.Database.DbContainerName,
            "        image: postgres",
            "        restart: always",
            "        environment:",
            "            - POSTGRES_PASSWORD=" + dockerDb.DbPassword,
            "            - POSTGRES_DB=" + dockerDb.DbName,
            "        volumes:",
            "            - db-data:/var/lib/postgresql",
            "    graphql:",
            "        image: dotnet-graphql:development",
            "        build:",
            "            context: .",
            "            dockerfile: ./docker/Dockerfile",
            "        environment:",
            "            - HOST=localhost",
            "            - DB_HOST=" + Config.Database.DbContainerName,
            "            - DB_DATABASENAME=" + dockerDb.DbName,
            "            - DB_USERNAME=" + dockerDb.DbUsername,
            "            - DB_PASSWORD=" + dockerDb.DbPassword,
            "        ports:",
            "            - \"80:80\"",
            "        depends_on:",
            "            - " + Config.Database.DbContainerName,
            "volumes:",
            "    db-data:",
            "        driver: local",
        });
    }
}
