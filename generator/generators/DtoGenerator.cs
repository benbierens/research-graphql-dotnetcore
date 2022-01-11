public class DtoGenerator : BaseGenerator
{
    public DtoGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void GenerateDtos()
    {
        MakeDir(Config.Output.GeneratedFolder, Config.Output.DtoSubFolder);

        foreach (var model in Models)
        {
            var fm = StartFile(Config.Output.DtoSubFolder, model.Name);
            var cm = StartClass(fm, model.Name);

            cm.AddUsing("System.Collections.Generic");

            cm.AddProperty(Config.IdType, "Id");
            AddModelFields(cm, model);

            if (model.HasMany != null) foreach (var m in model.HasMany)
                {
                    cm.AddProperty("virtual List<" + m + ">", m + "s");
                }
            AddForeignProperties(cm, model, "virtual ");

            fm.Build();
        }
    }
}