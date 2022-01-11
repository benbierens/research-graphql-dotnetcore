public class DatabaseGenerator : BaseGenerator
{
    public DatabaseGenerator(GeneratorConfig config)
        :base(config)
    {
    }

    public void GenerateDbContext()
    {
        MakeDir(Config.Output.GeneratedFolder, Config.Output.DatabaseSubFolder);

        var fm = StartFile(Config.Output.DatabaseSubFolder, Config.Database.DbContextFileName);
        AddDatabaseContextClass(fm);
        AddStaticAccessClass(fm);

        fm.Build();
    }

    public void CreateAndApplyInitialMigration()
    {
        RunCommand("dotnet", "ef", "database", "update");
        RunCommand("dotnet", "ef", "migrations", "add", "initial-setup");
        RunCommand("dotnet", "ef", "database", "update");
    }

    private void AddDatabaseContextClass(FileMaker fm)
    {
        var cm = StartClass(fm, Config.Database.DbContextClassName);

        cm.AddUsing("System.Collections.Generic");
        cm.AddUsing("Microsoft.EntityFrameworkCore");

        cm.AddInherrit("DbContext");

        foreach (var m in Models)
        {
            cm.AddProperty("DbSet<" + m.Name + ">", m.Name + "s");
        }

        cm.AddClosure("protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)", liner =>
        {
            liner.Add("optionsBuilder");
            liner.Indent();
            liner.Add(".UseLazyLoadingProxies()");
            liner.Add(".UseNpgsql(@\"" + Config.Database.ConnectionString + "\");");
            liner.Deindent();
        });
    }

    public void AddStaticAccessClass(FileMaker fm)
    {
        var contextName = "context";
        var cm = fm.AddClass(Config.Database.DbAccesserClassName);
        cm.Modifiers.Clear();
        cm.Modifiers.Add("static");

        cm.AddLine("private static " + Config.Database.DbContextClassName + " " + contextName + ";");

        cm.AddClosure("public static " + Config.Database.DbContextClassName + " Context", liner => 
        {
            liner.StartClosure("get");
            liner.Add("return new " + Config.Database.DbContextClassName + "();");
            liner.EndClosure();
        });
    }
}
