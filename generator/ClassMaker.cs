using System;
using System.Collections.Generic;
using System.Linq;

public class ClassMaker
{
    private readonly GeneratorConfig config;
    private readonly string className;
    private readonly List<string> lines = new List<string>();
    private readonly List<string> inherrit = new List<string>();
    private readonly List<string> usings = new List<string>();

    public ClassMaker(GeneratorConfig config, string className)
    {
        this.config = config;
        this.className = className;
    }

    public void AddLine(string line)
    {
        lines.Add(line);
    }

    public void AddProperty(string type, string name)
    {
        lines.Add("public " + type + " " + name + " { get; set; }");
    }

    public void AddNullableProperty(string type, string name)
    {
        if (!usings.Contains("System")) usings.Add("System");

        if (Nullability.IsNullableRequiredForType(type))
        {
            lines.Add("public Nullable<" + type + "> " + name + " { get; set; }");
        }
        else
        {
            AddProperty(type, name);
        }
    }

    public void AddInherrit(string name)
    {
        inherrit.Add(name);
    }

    public void AddUsing(string name)
    {
        usings.Add(name);
    }

    public void AddClosure(string name, Action<Liner> inClosure)
    {
        var liner = new Liner();
        liner.StartClosure(name);
        inClosure(liner);
        liner.EndClosure();
        
        lines.AddRange(liner.GetLines());
    }

    public string[] GetUsings()
    {
        return usings.ToArray();
    }

    public void Write(Liner liner)
    {
        liner.StartClosure("public class " + className + GetInherritTag());
        foreach (var line in lines) liner.Add(line);
        liner.EndClosure();
    }

    private string GetInherritTag()
    {
        if (!inherrit.Any()) return "";
        return " : " + string.Join(", ", inherrit);
    }
}