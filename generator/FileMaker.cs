using System.Collections.Generic;
using System.Linq;

public class FileMaker
{
    private readonly GeneratorConfig config;
    private readonly string filename;
    private readonly List<ClassMaker> classMakers = new List<ClassMaker>();

    public FileMaker(GeneratorConfig config, string filename)
    {
        this.config = config;
        this.filename = filename;
    }

    public ClassMaker AddClass(string className)
    {
        var cm = new ClassMaker(config, className);
        classMakers.Add(cm);
        return cm;
    }

    public void Build()
    {
        var liner = new Liner();
        var usings = classMakers.SelectMany(c => c.GetUsings()).Distinct().ToArray();
        foreach (var u in usings)
        {
            liner.Add("using " + EndStatement(u));
        }
        liner.Add("");
        liner.Add("// This file is generated.");
        liner.Add("");

        liner.StartClosure("namespace " + config.Config.GenerateNamespace);
        
        foreach (var c in classMakers)
        {
            c.Write(liner);
        }
        
        liner.EndClosure();

        liner.Write(filename);
    }

    private string EndStatement(string s)
    {
        if (s.EndsWith(";")) return s;
        return s + ";";
    }
}