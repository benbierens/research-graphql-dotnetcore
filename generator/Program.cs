using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace generator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigLoader().Get(args);
            var gen = new Generator(config);

            gen.Generate();
        }

        public class Generator
        {
            private readonly GeneratorConfig config;
            private readonly string[] packages = new [] {
                "Microsoft.EntityFrameworkCore --version 5.0.13",
                "Microsoft.EntityFrameworkCore.SqlServer --version 5.0.13",
                "HotChocolate.AspNetCore",
                "HotChocolate.AspNetCore.Subscriptions",
                "HotChocolate.Subscriptions",
                "HotChocolate.Subscriptions.InMemory",
            };
            private const string generatedFolder = "generated";
            private const string dtoFolder = "dto";

            private const string dbFolder = "db";
            private const string dbContextName = "DatabaseContext";
            private const string dbContextFilename = "DatabaseContext";

            private const string gqlFolder = "gql";
            private const string gqlQueriesName = "Queries";
            private const string gqlQueriesNameFilename = "Queries";
            private const string gqlMutationsName = "Mutations";
            private const string gqlMutationsNameFilename = "Mutations";
            private const string gqlSubscriptionsName = "Subscriptions";
            private const string gqlSubscriptionsNameFilename = "Subscriptions";

            public Generator(GeneratorConfig config)
            {
                this.config = config;
            }
        
            public void Generate()
            {
                MakeDir();

                CreateDotnetProject();

                MakeDir(generatedFolder);
                GenerateDtos();
                GenerateDbContext();
                GenerateGraphQl();
            }

            private void CreateDotnetProject()
            {
                RunCommand("dotnet", "new", "webapi");

                foreach (var p in packages)
                {
                    RunCommand("dotnet", "add", "package", p);
                }

                RunCommand("dotnet", "tool", "install", "--global", "dotnet-ef");
            }

            private void GenerateGraphQl()
            {
                MakeDir(generatedFolder, gqlFolder);
                GenerateQueries();
            }

            private void GenerateQueries()
            {
                //asasasa1
            }

            private void GenerateDbContext()
            {
                MakeDir(generatedFolder, dbFolder);

                var filename = Path.Join(config.Config.Output, generatedFolder, dbFolder, dbContextFilename + ".cs");
                var cm = new ClassMaker(config, dbContextName, filename);
                cm.AddUsing("System.Collections.Generic");
                cm.AddUsing("Microsoft.EntityFrameworkCore");

                cm.AddInherrit("DbContext");

                foreach (var m in config.Models)
                {
                    cm.AddProperty("DbSet<" + m.Name + ">", m.Name + "s");
                }

                cm.AddClosure("protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)", liner => 
                {
                    liner.Add("optionsBuilder.UseSqlServer(@\"" + config.Config.ConnectionString + "\");");
                });

                cm.Build();
            }

            private void GenerateDtos()
            {
                MakeDir(generatedFolder, dtoFolder);

                foreach (var model in config.Models)
                {
                    var filename = Path.Join(config.Config.Output, generatedFolder, dtoFolder, model.Name + ".cs");
                    var cm = new ClassMaker(config, model.Name, filename);
                    cm.AddUsing("System.Collections.Generic");

                    cm.AddProperty(config.Config.IdType, "Id");
                    foreach (var f in model.Fields)
                    {
                        cm.AddProperty(f.Type, f.Name);
                    }
                    if (model.HasMany != null) foreach (var m in model.HasMany)
                    {
                        cm.AddProperty("List<" + m + ">", m + "s");
                    }
                    var foreignProperties = GetForeignProperties(model, config);
                    foreach (var f in foreignProperties)
                    {
                        cm.AddProperty(config.Config.IdType, f + "Id");
                        cm.AddProperty(f, f);
                    }

                    cm.Build();
                }
            }

            private void MakeDir(params string[] path)
            {
                var arr = new []{config.Config.Output}.Concat(path).ToArray();
                var p = Path.Join(arr);
                if (!Directory.Exists(p)) 
                {
                    Directory.CreateDirectory(p);
                }
            }
        
            private void RunCommand(string cmd, params string[] args)
            {
                var info = new ProcessStartInfo();
                info.Arguments = string.Join(" ", args);
                info.FileName = cmd;
                info.WorkingDirectory = config.Config.Output;
                var p = Process.Start(info);
                p.WaitForExit();
            }

            private string[] GetForeignProperties(GeneratorConfig.ModelConfig model, GeneratorConfig config)
            {
                return config.Models.Where(m => m.HasMany != null && m.HasMany.Contains(model.Name)).Select(m => m.Name).ToArray();
            }
        }
    }

}
