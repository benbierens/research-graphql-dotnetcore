using System;
using System.Linq;

public class TestGenerator : BaseGenerator
{
    public TestGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void GenerateTests()
    {
        MakeTestDir(Config.Tests.SubFolder);
        CreateBaseGqlTestClass();
        CreateSubscriptionHandleClass();
        CreateDockerControllerClass();
        CreateQueryClasses();
    }

    private void CreateBaseGqlTestClass()
    {
        var fm = StartTestFile("BaseGqlTest");
        var cm = fm.AddClass("BaseGqlTest");
        cm.AddUsing("NUnit.Framework");
        cm.AddUsing("System");
        cm.AddUsing("System.Net.Http");
        cm.AddUsing("System.Text");
        cm.AddUsing("System.Threading.Tasks");
        cm.AddUsing("gqldemo_generated");
        cm.AddUsing("System.Collections.Generic");
        cm.AddUsing("Newtonsoft.Json");

        cm.AddAttribute("Category(\"" + Config.Tests.TestCategory + "\")");
        cm.AddProperty("Docker")
            .IsType("DockerController")
            .Build();

        cm.AddLine("private readonly HttpClient http = new HttpClient();");
        cm.AddLine("private readonly List<ISubscriptionHandle> handles = new List<ISubscriptionHandle>();");

        AddModelMethods(cm);

        fm.Build();
    }

    private void CreateSubscriptionHandleClass()
    {
        var fm = StartTestFile("SubscriptionHandle");
        var im = fm.AddInterface("ISubscriptionHandle");
        im.AddLine("Task Subscribe();");
        im.AddLine("Task Unsubscribe();");

        var cm = fm.AddClass("SubscriptionHandle<T>");
        cm.AddInherrit("ISubscriptionHandle");
        cm.AddUsing("System");
        cm.AddUsing("System.Collections.Generic");
        cm.AddUsing("System.Linq");
        cm.AddUsing("System.Net.WebSockets");
        cm.AddUsing("System.Text");
        cm.AddUsing("System.Threading");
        cm.AddUsing("System.Threading.Tasks");
        cm.AddUsing("Newtonsoft.Json");
        cm.AddUsing("NUnit.Framework");

        cm.AddLine("private readonly string subscription;");
        cm.AddLine("private readonly string[] fields;");
        cm.AddLine("private readonly CancellationTokenSource cts = new CancellationTokenSource();");
        cm.AddLine("private readonly ClientWebSocket ws = new ClientWebSocket();");
        cm.AddLine("private readonly List<string> received = new List<string>();");
        cm.AddLine("private bool running;");
        cm.AddBlankLine();

        cm.AddClosure("public SubscriptionHandle(string subscription, params string[] fields)", liner => 
        {
            liner.Add("this.subscription = subscription;");
            liner.Add("this.fields = fields;");
            liner.Add("running = true;");
            liner.Add("ws.Options.AddSubProtocol(\"graphql-ws\");");
        });

        cm.AddClosure("public async Task Subscribe()", liner => 
        {
            liner.Add("await ws.ConnectAsync(new Uri(\"ws://localhost/graphql\"), cts.Token);");
            liner.Add("var _ = Task.Run(ReceivingLoop);");
            liner.Add("var f = \"{\" + string.Join(\" \", fields) + \" }\";");
            liner.Add("await Send(\"{type: \\\"connection_init\\\", payload: {}}\");");
            liner.Add("await Send(\"{\\\"id\\\":\\\"1\\\",\\\"type\\\":\\\"start\\\",\\\"payload\\\":{\\\"query\\\":\\\"subscription { \" + subscription + f + \" }\\\"}}\");");
        });

        cm.AddClosure("public async Task Unsubscribe()", liner => 
        {
            liner.Add("running = false;");
            liner.Add("await Send(\"{\\\"id\\\":\\\"1\\\",\\\"type\\\":\\\"stop\\\"}\");");
        });

        cm.AddClosure("public T AssertReceived()", liner => 
        {
            liner.Add("var line = received.SingleOrDefault(l => l.Contains(subscription));");
            liner.StartClosure("if (line == null)");
            liner.Add("Assert.Fail(\"Expected subscription '\" + subscription + \"', but was not received.\");");
            liner.Add("throw new Exception();");
            liner.EndClosure();
            liner.StartClosure("if (line.Contains(\"errors\"))");
            liner.Add("Assert.Fail(\"Response contains errors:\" + line);");
            liner.Add("throw new Exception();");
            liner.EndClosure();
            liner.Add("var sub = line.Substring(line.IndexOf(subscription));");
            liner.Add("sub = sub.Substring(sub.IndexOf('{'));");
            liner.Add("sub = sub.Substring(0, sub.IndexOf('}') + 1);");
            liner.Add("return JsonConvert.DeserializeObject<T>(sub);");
        });

        cm.AddClosure("private async Task ReceivingLoop()", liner => 
        {
            liner.StartClosure("while (running)");
            liner.Add("var bytes = new byte[1024];");
            liner.Add("var buffer = new ArraySegment<byte>(bytes);");
            liner.Add("var receive = await ws.ReceiveAsync(buffer, cts.Token);");
            liner.Add("var l = bytes.Take(receive.Count).ToArray();");
            liner.Add("received.Add(Encoding.UTF8.GetString(l));");
            liner.EndClosure();
        });

        cm.AddClosure("private async Task Send(string query)", liner => 
        {            
            liner.Add("var qbytes = Encoding.UTF8.GetBytes(query);");
            liner.Add("var segment= new ArraySegment<byte>(qbytes);");
            liner.Add("await ws.SendAsync(segment, WebSocketMessageType.Text, true, cts.Token);");
        });

        fm.Build();
    }

    private void CreateDockerControllerClass()
    {
        var fm = StartTestFile("DockerController");
        var cm = fm.AddClass("DockerController");
        cm.AddUsing("System.Diagnostics");

        cm.AddClosure("public void Start()", liner =>
        {
            liner.Add("RunCommand(\"dotnet\", \"publish\", \"" + Config.Output.SourceFolder + "\", \"-c\", \"release\");");
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
            liner.Add("p.WaitForExit();");
        });

        fm.Build();
    }

    private void CreateQueryClasses()
    {
        var fm = StartTestFile("QueryClasses");
        CreateQueryDataClass(fm);

        foreach (var m in Models)
        {
            CreateQueryClassForModel(fm, m);
        }

        fm.Build();
    }

    private void CreateQueryClassForModel(FileMaker fm, GeneratorConfig.ModelConfig m)
    {
        var cm = fm.AddClass("All" + m.Name + "sQuery");
        cm.AddUsing("System.Collections.Generic");
        cm.AddProperty(m.Name)
            .IsListOfType(m.Name)
            .Build();
    }

    private void CreateQueryDataClass(FileMaker fm)
    {
        var cm = fm.AddClass("GqlData<T>");
        cm.AddProperty("Data")
            .IsType("T")
            .IsNullable()
            .Build();

        var cm2 = fm.AddClass("MutationResponse");
        cm2.AddProperty("Id")
            .IsType(Config.IdType)
            .Build();
    }

    private void AddModelMethods(ClassMaker cm)
    {
        foreach (var m in Models)
        {
            AddQueryAllMethod(cm, m);
            AddCreateMutationMethod(cm, m);
            AddSubscribeCreatedMethod(cm, m);
        }

        cm.AddClosure("private async Task<T> PostRequest<T>(string query)", liner =>
        {
            liner.Add("var response = await http.PostAsync(\"http://localhost/graphql/\", new StringContent(query, Encoding.UTF8, \"application/json\"));");
            liner.Add("var content = await response.Content.ReadAsStringAsync();");
            liner.Add("var result = JsonConvert.DeserializeObject<GqlData<T>>(content);");
            liner.Add("if (result.Data == null) throw new Exception(\"GraphQl operation failed.\");");
            liner.Add("return result.Data;");
        });
    }

    private void AddQueryAllMethod(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        cm.AddClosure("protected async Task<List<" + m.Name + ">> QueryAll" + m.Name + "s()", liner =>
        {
            liner.Add("var query = \"{ \\\"query\\\": \\\"query { " + m.Name.ToLowerInvariant() + "s { " + GetQueryFields(m) +" } } \\\" }\";");
            liner.Add("var data = await PostRequest<All" + m.Name + "sQuery>(query);");
            liner.Add("return data." + m.Name + "s;");
        });
    }

    private void AddCreateMutationMethod(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        cm.AddClosure("protected async Task<" + Config.IdType + "> Create" + m.Name + "(" + GetCreateMutationArguments(m) + ")", liner =>
        {
            liner.Add("var mutation = \"{ \\\"query\\\": \\\"mutation { create" + m.Name + "(input: {" + GetCreateMutationInput(m) + "}) { id } }\\\"}\";");
            liner.Add("var data = await PostRequest<MutationResponse>(mutation);");
            liner.Add("return data.Id;");
        });
    }

    private void AddSubscribeCreatedMethod(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        cm.AddClosure("protected async Task<SubscriptionHandle<" + m.Name + ">> SubscribeTo" + m.Name + Config.GraphQl.GqlSubscriptionCreatedMethod + "()", liner =>
        {
            var fields = GetCreatedSubscriptionFields(m);
            liner.Add("var s = new SubscriptionHandle<" + m.Name + ">(\"" + m.Name.ToLowerInvariant() + Config.GraphQl.GqlSubscriptionCreatedMethod + "\", " + fields + ");");
            liner.Add("await s.Subscribe();");
            liner.Add("handles.Add(s);");
            liner.Add("return s;");            
        });
    }

    private string GetQueryFields(GeneratorConfig.ModelConfig m)
    {
        var foreignProperties = GetForeignProperties(m);
        var foreignIds = string.Join(" ", foreignProperties.Select(f => f.ToLowerInvariant() + "Id"));
        return "id " + string.Join(" ", m.Fields.Select(f => f.Name.ToLowerInvariant())) + " " + foreignIds;
    }

    private string GetCreateMutationArguments(GeneratorConfig.ModelConfig m)
    {
        var foreignProperties = GetForeignProperties(m);
        var fields = m.Fields.Select(f => f.Type + " " + f.Name.ToLowerInvariant());
        var foreignIds = foreignProperties.Select(f => Config.IdType + " " + f.ToLowerInvariant() + "Id");
        var all = fields.Concat(foreignIds);
        return string.Join(", ", all);
    }

    private string GetCreateMutationInput(GeneratorConfig.ModelConfig m)
    {
        var argumentize = new Func<string, string>(f => f + ": \\\\\\\"\" + " + f + " + \"\\\\\\\"");

        var foreignProperties = GetForeignProperties(m);
        var fields = m.Fields.Select(f => f.Name.ToLowerInvariant()).Select(argumentize);
        var foreignIds = foreignProperties.Select(f => f.ToLowerInvariant() + "Id").Select(argumentize);
        var all = fields.Concat(foreignIds);
        return string.Join(" ", all);
    }

    private string GetCreatedSubscriptionFields(GeneratorConfig.ModelConfig m)
    {
        var foreignProperties = GetForeignProperties(m);

        var fields = new []{ "\"id\""}
            .Concat(m.Fields.Select(f => "\"" + f.Name.ToLowerInvariant() + "\""))
            .Concat(foreignProperties.Select(f => "\"" + f.ToLowerInvariant() + "Id\""));

        return string.Join(", ", fields);
    }
}
