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
            liner.Add("optionsBuilder.UseNpgsql(@\"" + Config.Database.ConnectionString + "\");");
        });

        fm.Build();
    }

    public void CreateAndApplyInitialMigration()
    {
        RunCommand("dotnet", "ef", "database", "update");
        RunCommand("dotnet", "ef", "migrations", "add", "initial-setup");
        RunCommand("dotnet", "ef", "database", "update");
    }
}

// dotnet ef migrations add initialSetup                        
// dotnet ef database update
