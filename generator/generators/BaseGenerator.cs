using System.Diagnostics;
using System.IO;
using System.Linq;

public class BaseGenerator
{
    public BaseGenerator(GeneratorConfig config)
    {
        Config = config.Config;
        Models = config.Models;
    }

    public GeneratorConfig.ConfigSection Config { get; private set; }
    public GeneratorConfig.ModelConfig[] Models { get; private set; }

    public FileMaker StartFile(string subfolder, string filename)
    {
        var f = Path.Join(Config.Output.ProjectRoot, Config.Output.GeneratedFolder, subfolder, filename + ".cs");
        return new FileMaker(Config, f);
    }

    public FileModifier ModifyFile(string filename)
    {
        return ModifyFile("", filename);
    }

    public FileModifier ModifyFile(string subfolder, string filename)
    {
        var f = Path.Join(Config.Output.ProjectRoot, subfolder, filename + ".cs");
        return new FileModifier(f);
    }

    public ClassMaker StartClass(FileMaker fm, string className)
    {
        return fm.AddClass(className);
    }

    public void MakeDir(params string[] path)
    {
        var arr = new[] { Config.Output.ProjectRoot }.Concat(path).ToArray();
        var p = Path.Join(arr);
        if (!Directory.Exists(p))
        {
            Directory.CreateDirectory(p);
        }
    }
    
    public string[] GetForeignProperties(GeneratorConfig.ModelConfig model)
    {
        return Models.Where(m => m.HasMany != null && m.HasMany.Contains(model.Name)).Select(m => m.Name).ToArray();
    }
    
    public void RunCommand(string cmd, params string[] args)
    {
        var info = new ProcessStartInfo();
        info.Arguments = string.Join(" ", args);
        info.FileName = cmd;
        info.WorkingDirectory = Config.Output.ProjectRoot;
        var p = Process.Start(info);
        p.WaitForExit();
    }
        
    public void AddModelFields(ClassMaker cm, GeneratorConfig.ModelConfig model)
    {
        foreach (var f in model.Fields)
        {
            cm.AddProperty(f.Type, f.Name);
        }
    }
    
    public void AddForeignProperties(ClassMaker cm, GeneratorConfig.ModelConfig model, string typePrefix = "", bool idOnly = false)
    {
        var foreignProperties = GetForeignProperties(model);
        foreach (var f in foreignProperties)
        {
            cm.AddProperty(Config.IdType, f + "Id");
            if (!idOnly) cm.AddProperty(typePrefix + f, f);
        }
    }
}
