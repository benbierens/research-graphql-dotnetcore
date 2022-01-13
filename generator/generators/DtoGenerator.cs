public class DtoGenerator : BaseGenerator
{
    public DtoGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void GenerateDtos()
    {
        MakeSrcDir(Config.Output.GeneratedFolder, Config.Output.DtoSubFolder);

        foreach (var model in Models)
        {
            var fm = StartSrcFile(Config.Output.DtoSubFolder, model.Name);
            var cm = StartClass(fm, model.Name);

            cm.AddUsing("System.Collections.Generic");

            cm.AddProperty("Id")
                .IsType(Config.IdType)
                .Build();

            AddModelFields(cm, model);

            foreach (var m in model.HasMany)
            {
                cm.AddProperty(m)
                    .WithModifier("virtual")
                    .IsListOfType(m)
                    .Build();
            }
            AddForeignProperties(cm, model, "virtual ");

            fm.Build();
        }
    }
}