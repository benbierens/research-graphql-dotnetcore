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
                    var inputTypeNames = GetInputTypeNames(model);

                    var addClass = StartClass(fm, inputTypeNames.Create);
                    AddModelFields(addClass, model);
                    AddForeignProperties(addClass, model, true);
                    
                    var updateClass = StartClass(fm, inputTypeNames.Update);
                    updateClass.AddProperty(config.Config.IdType, model.Name + "Id");
                    AddModelFieldsAsNullable(updateClass, model);
                    AddForeignPropertiesAsNullable(updateClass, model, true);

                    var deleteClass = StartClass(fm, inputTypeNames.Delete);
                    deleteClass.AddProperty(config.Config.IdType, model.Name + "Id");
                }

                fm.Build();
            }

            private void GenerateMutations()
            {
                var fm = StartFile(gqlFolder, gqlMutationsNameFilename);
                var cm = StartClass(fm, gqlMutationsName);
                cm.AddUsing("System.Threading.Tasks");
                cm.AddUsing("HotChocolate");
                cm.AddUsing("HotChocolate.Subscriptions");

                foreach (var model in config.Models)
                {
                    var inputTypeNames = GetInputTypeNames(model);

                    AddCreateMutation(cm, model, inputTypeNames);
                    AddUpdateMutation(cm, model, inputTypeNames);
                }

                fm.Build();

            }

            private void AddCreateMutation(ClassMaker cm, GeneratorConfig.ModelConfig model, InputTypeNames inputTypeNames)
            {
                cm.AddClosure("public async Task<" + model.Name + "> " + gqlMutationsCreateMethod + model.Name +
                "(" + inputTypeNames.Create + " input, [Service] ITopicEventSender sender)", liner => {
                    liner.StartClosure("var createdEntity = new " + model.Name);
                    AddModelInitializer(liner, model, "input");
                    liner.EndClosure(";");

                    AddDatabaseAddAndSave(liner);

                    liner.Add("await sender.SendAsync(\"" + model.Name + gqlSubscriptionCreatedMethod + "\", createdEntity);");
                    liner.Add("return createdEntity;");
                });
            }

            private void AddUpdateMutation(ClassMaker cm, GeneratorConfig.ModelConfig model, InputTypeNames inputTypeNames)
            {
                cm.AddClosure("public async Task<" + model.Name + "> " + gqlMutationsUpdateMethod + model.Name +
                "(" + inputTypeNames.Update + " input, [Service] ITopicEventSender sender)", liner => {
                    liner.Add(model.Name + " entity = null;");

                    liner.StartClosure("using (var db = new DatabaseContext())");
                    liner.Add("entity = db.Set<" + model.Name + ">().Find(input." + model.Name + "Id);");
                    AddModelUpdater(liner, model, "input");
                    liner.Add("db.SaveChanges();");
                    liner.EndClosure();

                    liner.Add("await sender.SendAsync(\"" + model.Name + gqlSubscriptionUpdatedMethod + "\", entity);");
                    liner.Add("return entity;");
                });
            }

            private void AddModelInitializer(Liner liner, GeneratorConfig.ModelConfig model, string inputName)
            {
                foreach (var field in model.Fields)
                {
                    liner.Add(field.Name + " = " + inputName + "." + field.Name + ",");
                }
                var foreignProperties = GetForeignProperties(model, config);
                foreach (var f in foreignProperties)
                {
                    liner.Add(f + "Id = " + inputName + "." + f + "Id,");
                }
            }

            private void AddDatabaseAddAndSave(Liner liner)
            {
                liner.StartClosure("using (var db = new DatabaseContext())");
                liner.Add("db.Add(createdEntity);");
                liner.Add("db.SaveChanges();");
                liner.EndClosure();
            }

            private void AddModelUpdater(Liner liner, GeneratorConfig.ModelConfig model, string inputName)
            {
                foreach (var field in model.Fields)
                {
                    AddAssignmentLine(liner, field.Type, field.Name, inputName);
                }
                var foreignProperties = GetForeignProperties(model, config);
                foreach (var f in foreignProperties)
                {
                    AddAssignmentLine(liner, config.Config.IdType, f + "Id", inputName);
                }
            }

            private void AddAssignmentLine(Liner liner, string type, string fieldName, string inputName)
            {
                if (Nullability.IsNullableRequiredForType(type))
                {
                    liner.Add("if (" + inputName + "." + fieldName + ".HasValue) entity." + fieldName + " = " + inputName + "." + fieldName + ".Value;");
                }
                else 
                {
                    liner.Add("if (" + inputName + "." + fieldName + " != null) entity." + fieldName + " = " + inputName + "." + fieldName + ";");
                }
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
        
            private InputTypeNames GetInputTypeNames(GeneratorConfig.ModelConfig model)
            {
                return new InputTypeNames
                {
                    Create = gqlMutationsCreateMethod + model.Name + gqlMutationsInputTypePostfix,
                    Update = gqlMutationsUpdateMethod + model.Name + gqlMutationsInputTypePostfix,
                    Delete = gqlMutationsDeleteMethod + model.Name + gqlMutationsInputTypePostfix
                };
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

            private class InputTypeNames
            {
                public string Create { get; set; }
                public string Update { get; set; }
                public string Delete { get; set; }
            }
        }
    }

}
