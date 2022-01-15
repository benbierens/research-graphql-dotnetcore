using System;
using System.Collections.Generic;
using System.Linq;

public class ClassMaker
{
    private readonly string className;
    private readonly List<string> lines = new List<string>();
    private readonly List<string> inherrit = new List<string>();
    private readonly List<string> usings = new List<string>();
    private readonly List<string> attributes = new List<string>();

    public ClassMaker(string className)
    {
        this.className = className;

        Modifiers = new List<string>();
        Modifiers.Add("partial");
    }

    public List<string> Modifiers
    {
        get; private set;
    }

    public void AddLine(string line)
    {
        lines.Add(NormalizeWhitespaces(line));
    }

    public void AddBlankLine()
    {
        AddLine("");
    }

    public PropertyMaker AddProperty(string name)
    {
        return new PropertyMaker(this, name);
    }

    public void AddInherrit(string name)
    {
        inherrit.Add(name);
    }

    public void AddUsing(string name)
    {
        usings.Add(name);
    }

    public void AddAttribute(string attribute)
    {
        attributes.Add(attribute);
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
        Modifiers.Insert(0, "public");
        Modifiers.Add("class");

        var distinct = Modifiers.Distinct().ToArray();
        var modifiers = string.Join(" ", distinct);

        foreach (var att in attributes)
        {
            liner.Add("[" + att + "]");
        }
        
        liner.StartClosure(modifiers + " " + className + GetInherritTag());
        foreach (var line in lines) liner.Add(line);
        liner.EndClosure();
    }

    private string GetInherritTag()
    {
        if (!inherrit.Any()) return "";
        return " : " + string.Join(", ", inherrit);
    }

    private string NormalizeWhitespaces(string s)
    {
        while (s.Contains ("  "))
        {
            s = s.Replace("  ", " ");
        }
        return s;
    }
}