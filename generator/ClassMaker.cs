using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ClassMaker
{
    private readonly GeneratorConfig config;
    private readonly string classname;
    private readonly string filename;
    private readonly List<string> lines = new List<string>();
    private readonly List<string> inherrit = new List<string>();
    private readonly List<string> usings = new List<string>();

    public ClassMaker(GeneratorConfig config, string classname, string filename)
    {
        this.config = config;
        this.classname = classname;
        this.filename = filename;
    }

    public void AddProperty(string type, string name)
    {
        lines.Add("public " + type + " " + name + " { get; set; }");
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

    public void Build()
    {
        var liner = new Liner();
        foreach (var u in usings)
        {
            liner.Add("using " + EndStatement(u));
        }
        liner.Add("");

        liner.StartClosure("namespace " + config.Config.GenerateNamespace);
        liner.StartClosure("public class " + classname + GetInherritTag());
        foreach (var line in lines) liner.Add(line);
        liner.EndClosure();
        liner.EndClosure();

        liner.Write(filename);
    }

    private string EndStatement(string s)
    {
        if (s.EndsWith(";")) return s;
        return s + ";";
    }

    private string GetInherritTag()
    {
        if (!inherrit.Any()) return "";
        return " : " + string.Join(", ", inherrit);
    }

    public class Liner
    {
        private readonly List<string> lines = new List<string>();
        private int indent = 0;

        public void Indent()
        {
            indent++;
        }

        public void Deindent()
        {
            indent--;
        }

        public void StartClosure(string name)
        {
            Add(name);
            Add("{");
            Indent();
        }

        public void EndClosure()
        {
            Deindent();
            Add("}");
            Add("");
        }

        public void Add(string l)
        {
            var line = "";
            for (var i = 0; i < indent; i++) line += "   ";
            line += l;
            lines.Add(line);
        }

        public string[] GetLines()
        {
            return lines.ToArray();
        }

        public void Write(string filename)
        {
            File.WriteAllLines(filename, lines);
        }
    }

}