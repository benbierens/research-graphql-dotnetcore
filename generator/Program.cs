using System;
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
            private const string dtoFolder = "dto";
            private const string dbFolder = "db";
            private const string dbContextName = "DatabaseContext";
            private const string dbContextFilename = "DatabaseContext";

            public Generator(GeneratorConfig config)
            {
                this.config = config;
            }
        
            public void Generate()
            {
                MakeDir();

                GenerateDtos();

                GenerateDbContext();
            }

            private void GenerateDbContext()
            {
                MakeDir(dbFolder);

                var filename = Path.Join(config.Config.Output, dbFolder, dbContextFilename + ".cs");
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
                MakeDir(dtoFolder);

                foreach (var model in config.Models)
                {
                    var filename = Path.Join(config.Config.Output, dtoFolder, model.Name + ".cs");
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
        
            private string[] GetForeignProperties(GeneratorConfig.ModelConfig model, GeneratorConfig config)
            {
                return config.Models.Where(m => m.HasMany != null && m.HasMany.Contains(model.Name)).Select(m => m.Name).ToArray();
            }
        }
    }

}
