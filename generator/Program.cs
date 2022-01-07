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
            private const string gqlTypesName = "GraphQlTypes";

            private const string gqlTypesFileName = "GraphQlTypes";
            private const string gqlMutationsName = "Mutations";
            private const string gqlMutationsNameFilename = "Mutations";
            private const string gqlMutationsInputTypePostfix = "Input";
            private const string gqlMutationsCreateMethod = "Create";
            private const string gqlMutationsUpdateMethod = "Update";
            private const string gqlMutationsDeleteMethod = "Delete";

            private const string gqlSubscriptionsName = "Subscriptions";
            private const string gqlSubscriptionsNameFilename = "Subscriptions";
            private const string gqlSubscriptionCreatedMethod = "Created";
            private const string gqlSubscriptionUpdatedMethod = "Updated";
            private const string gqlSubscriptionDeletedMethod = "Deleted";

            public Generator(GeneratorConfig config)
            {
                this.config = config;
            }
        
            public void Generate()
            {
                MakeDir();

                //CreateDotnetProject();

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
                GenerateSubscriptions();
                GenerateTypes();
                GenerateMutations();
            }

            private void GenerateQueries()
            {
                var fm = StartFile(gqlFolder, gqlQueriesNameFilename);
                var cm = StartClass(fm, gqlQueriesName);
                cm.AddUsing("System.Threading.Tasks");
                cm.AddUsing("Microsoft.EntityFrameworkCore");

                foreach (var model in config.Models)
                {
                    cm.AddClosure("public async Task<" + model.Name +"[]> " + model.Name + "s()", liner => 
                    {
                        liner.StartClosure("using (var db = new DatabaseContext())");
                        liner.Add("return await db." + model.Name + "s.ToArrayAsync();");
                        liner.EndClosure();
                    });
                }

                fm.Build();
            }

            private void GenerateSubscriptions()
            {
                var fm = StartFile(gqlFolder, gqlSubscriptionsNameFilename);
                var cm = StartClass(fm, gqlSubscriptionsName);
                cm.AddUsing("HotChocolate");
                cm.AddUsing("HotChocolate.Subscriptions");
                cm.AddUsing("HotChocolate.Types");

                foreach (var model in config.Models)
                {
                    AddSubscriptionMethod(cm, model.Name, gqlSubscriptionCreatedMethod);
                    AddSubscriptionMethod(cm, model.Name, gqlSubscriptionUpdatedMethod);
                    AddSubscriptionMethod(cm, model.Name, gqlSubscriptionDeletedMethod);
                }

                fm.Build();
            }

            private void GenerateTypes()
            {
                var fm = StartFile(gqlFolder, gqlTypesFileName);

                foreach (var model in config.Models)
                {
                    var addClass = StartClass(fm, gqlMutationsCreateMethod + model.Name + gqlMutationsInputTypePostfix);
                    AddModelFields(addClass, model);
                    AddForeignProperties(addClass, model, true);
                    
                    var updateClass = StartClass(fm, gqlMutationsUpdateMethod + model.Name + gqlMutationsInputTypePostfix);
                    AddModelFieldsAsNullable(updateClass, model);
                    AddForeignPropertiesAsNullable(updateClass, model, true);

                    var deleteClass = StartClass(fm, gqlMutationsDeleteMethod + model.Name + gqlMutationsInputTypePostfix);
                    deleteClass.AddProperty(config.Config.IdType, model.Name + "Id");
                }

                fm.Build();
            }

            private void GenerateMutations()
            {
        // public async Task<Cat> MoveCat(MoveCatInput input, [Service] ITopicEventSender sender)
        // {
        //     var result = Data.Instance.MoveCat(input.CatIndex, input.CouchIndex);

        //     await sender.SendAsync("OnCatChanged", result);

        //     return result;
        // }

                var fm = StartFile(gqlFolder, gqlMutationsNameFilename);
                var cm = StartClass(fm, gqlMutationsName);
                cm.AddUsing("System.Threading.Tasks");
                cm.AddUsing("HotChocolate");
                cm.AddUsing("HotChocolate.Subscriptions");

                foreach (var model in config.Models)
                {
                    create method, update method, delete method.
                }

                fm.Build();

            }

            private void AddSubscriptionMethod(ClassMaker cm, string modelName, string method)
            {
                var n =  modelName;
                var l = n.ToLowerInvariant();
                cm.AddLine("[Subscribe]");
                cm.AddLine("public " + n + " " + n + method + "([EventMessage] " + n + " _" + l + ") => _" + l + ";");
                cm.AddLine("");
            }

            private void GenerateDbContext()
            {
                MakeDir(generatedFolder, dbFolder);

                var fm = StartFile(dbFolder, dbContextFilename);
                var cm = StartClass(fm, dbContextName);

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

                fm.Build();
            }

            private void GenerateDtos()
            {
                MakeDir(generatedFolder, dtoFolder);

                foreach (var model in config.Models)
                {
                    var fm = StartFile(dtoFolder, model.Name);
                    var cm = StartClass(fm, model.Name);

                    cm.AddUsing("System.Collections.Generic");

                    cm.AddProperty(config.Config.IdType, "Id");
                    AddModelFields(cm, model);

                    if (model.HasMany != null) foreach (var m in model.HasMany)
                    {
                        cm.AddProperty("List<" + m + ">", m + "s");
                    }
                    AddForeignProperties(cm, model);

                    fm.Build();
                }
            }

            private void AddModelFields(ClassMaker cm, GeneratorConfig.ModelConfig model)
            {
                foreach (var f in model.Fields)
                {
                    cm.AddProperty(f.Type, f.Name);
                }
            }

            private void AddForeignProperties(ClassMaker cm, GeneratorConfig.ModelConfig model, bool idOnly = false)
            {
                var foreignProperties = GetForeignProperties(model, config);
                foreach (var f in foreignProperties)
                {
                    cm.AddProperty(config.Config.IdType, f + "Id");
                    if (!idOnly) cm.AddProperty(f, f);
                }
            }

            private void AddModelFieldsAsNullable(ClassMaker cm, GeneratorConfig.ModelConfig model)
            {
                foreach (var f in model.Fields)
                {
                    cm.AddNullableProperty(f.Type, f.Name);
                }
            }

            private void AddForeignPropertiesAsNullable(ClassMaker cm, GeneratorConfig.ModelConfig model, bool idOnly = false)
            {
                var foreignProperties = GetForeignProperties(model, config);
                foreach (var f in foreignProperties)
                {
                    cm.AddNullableProperty(config.Config.IdType, f + "Id");
                    if (!idOnly) cm.AddNullableProperty(f, f);
                }
            }

            private FileMaker StartFile(string subfolder, string filename)
            {
                var f = Path.Join(config.Config.Output, generatedFolder, subfolder, filename + ".cs");
                return new FileMaker(config, f);
            }

            private ClassMaker StartClass(FileMaker fm, string className)
            {
                return fm.AddClass(className);
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
