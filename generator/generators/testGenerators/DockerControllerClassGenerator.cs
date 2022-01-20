public class DockerControllerClassGenerator : BaseGenerator
{
    public DockerControllerClassGenerator(GeneratorConfig config)
    : base(config)
    {
    }
    
    public void CreateDockerControllerClass()
    {
        var fm = StartTestFile("DockerController");
        var cm = fm.AddClass("DockerController");
        cm.AddUsing("System");
        cm.AddUsing("System.Diagnostics");

        cm.AddClosure("public void Start()", liner =>
        {
            liner.Add("RunCommand(\"dotnet\", \"publish\", \"../../../../" + Config.Output.SourceFolder + "\", \"-c\", \"release\");");
            liner.Add("RunCommand(\"docker-compose\", \"up\", \"-d\");");
        });

        cm.AddClosure("public void Stop()", liner =>
        {
            liner.Add("RunCommand(\"docker-compose\", \"down\", \"--rmi\", \"all\", \"-v\");");
        });

        cm.AddClosure("private void RunCommand(string cmd, params string[] args)", liner =>
        {
            liner.Add("var info = new ProcessStartInfo();");
            liner.Add("info.Arguments = string.Join(\" \", args);");
            liner.Add("info.FileName = cmd;");
            liner.Add("var p = Process.Start(info);");
            liner.Add("if (p == null) throw new Exception(\"Failed to start process.\");");
            liner.Add("p.WaitForExit();");
        });

        fm.Build();
    }
}
