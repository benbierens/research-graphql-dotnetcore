using System.Diagnostics;
using System.IO;
using System.Linq;
using System;

public class BaseGenerator
{
    public BaseGenerator(GeneratorConfig config)
    {
        Config = config.Config;
        Models = config.Models;
    }

    public GeneratorConfig.ConfigSection Config { get; private set; }
    public GeneratorConfig.ModelConfig[] Models { get; private set; }

    public FileMaker StartSrcFile(string subfolder, string filename)
    {
        var f = Path.Join(Config.Output.ProjectRoot, Config.Output.SourceFolder, Config.Output.GeneratedFolder, subfolder, filename + ".cs");
        return new FileMaker(Config, f);
    }

    public FileMaker StartTestFile(string filename)
    {
        var f = Path.Join(Config.Output.ProjectRoot, Config.Output.TestFolder, Config.Tests.SubFolder, filename + ".cs");
        return new FileMaker(Config, f);
    }

    public CodeFileModifier ModifyFile(string filename)
    {
        return ModifyFile("", filename);
    }

    public CodeFileModifier ModifyFile(string subfolder, string filename)
    {
        var f = Path.Join(Config.Output.ProjectRoot, subfolder, filename);
        return new CodeFileModifier(f);
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

    public void MakeSrcDir(params string[] path)
    {
        var arr = new[] { Config.Output.SourceFolder }.Concat(path).ToArray();
        MakeDir(arr);
    }

    public void MakeTestDir(params string[] path)
    {
        var arr = new[] { Config.Output.TestFolder }.Concat(path).ToArray();
        MakeDir(arr);
    }

    public void WriteRawFile(Action<Liner> onLiner, params string[] filePath)
    {
        var arr = new[] { Config.Output.ProjectRoot }.Concat(filePath).ToArray();
        var liner = new Liner();
        onLiner(liner);
        File.WriteAllLines(Path.Combine(arr), liner.GetLines());
    }

    public void DeleteFile(params string[] path)
    {
        var arr = new[] { Config.Output.ProjectRoot }.Concat(path).ToArray();
        File.Delete(Path.Join(arr));
    }

    public ForeignProperty[] GetForeignProperties(GeneratorConfig.ModelConfig model)
    {
        var fp = Models.Where(m => m.HasMany != null && m.HasMany.Contains(model.Name)).Select(m => m.Name).ToArray();

        return fp.Select(f => new ForeignProperty
        {
            Type = f,
            Name = GetForeignPropertyPrefix(model, f) + f,
            WithId = GetForeignPropertyPrefix(model, f) + f + "Id",
            IsSelfReference = IsSelfReference(model, f)

        }).ToArray();
    }

    private string GetForeignPropertyPrefix(GeneratorConfig.ModelConfig m, string hasManyEntry)
    {
        if (IsSelfReference(m, hasManyEntry)) return Config.SelfRefNavigationPropertyPrefix;
        return "";
    }

    private bool IsSelfReference(GeneratorConfig.ModelConfig model, string hasManyEntry)
    {
        return hasManyEntry == model.Name;
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
            cm.AddProperty(f.Name)
                .IsType(f.Type)
                .Build();
        }
    }

    public InputTypeNames GetInputTypeNames(GeneratorConfig.ModelConfig model)
    {
        return new InputTypeNames
        {
            Create = Config.GraphQl.GqlMutationsCreateMethod + model.Name + Config.GraphQl.GqlMutationsInputTypePostfix,
            Update = Config.GraphQl.GqlMutationsUpdateMethod + model.Name + Config.GraphQl.GqlMutationsInputTypePostfix,
            Delete = Config.GraphQl.GqlMutationsDeleteMethod + model.Name + Config.GraphQl.GqlMutationsInputTypePostfix
        };
    }
}
