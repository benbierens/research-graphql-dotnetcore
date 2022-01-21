using System.Collections.Generic;
using System.Linq;

public class FileMaker
{
    private readonly string filename;
    private readonly string @namespace;
    private readonly List<ClassMaker> classMakers = new List<ClassMaker>();

    public FileMaker(string filename, string @namespace)
    {
        this.filename = filename;
        this.@namespace = @namespace;
    }

    public ClassMaker AddClass(string className)
    {
        var cm = new ClassMaker(className);
        classMakers.Add(cm);
        return cm;
    }

    public ClassMaker AddInterface(string className)
    {
        var cm = new ClassMaker(className, true);
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

        liner.StartClosure("namespace " + @namespace);
        
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