public class ProjectGenerator : BaseGenerator
{
    public ProjectGenerator(GeneratorConfig config)
        :base(config)
    {
    }

    public void CreateDotNetProject()
    {
        RunCommand("dotnet", "new", "-i", "HotChocolate.Templates.Server");
        RunCommand("dotnet", "new", "graphql");

        foreach (var p in Config.Packages)
        {
            RunCommand("dotnet", "add", "package", p);
        }

        RunCommand("dotnet", "tool", "install", "--global", "dotnet-ef");
    }

    public void ModifyDefaultFiles()
    {
        var mf = ModifyFile("", "Startup");
        mf.AddUsing(Config.GenerateNamespace);
        mf.AddUsing("HotChocolate.AspNetCore");

        mf.ReplaceLine(".AddQueryType<Query>();",
                ".AddQueryType<" + Config.GraphQl.GqlQueriesClassName + ">()",
                ".AddMutationType<" + Config.GraphQl.GqlMutationsClassName + ">()",
                ".AddSubscriptionType<" + Config.GraphQl.GqlSubscriptionsClassName + ">();",
                "",
                "services.AddInMemorySubscriptions();");

        mf.ReplaceLine("app.UseDeveloperExceptionPage();",
            "app.UsePlayground();",
            "app.UseDeveloperExceptionPage();");

        mf.Modify();
    }
}
