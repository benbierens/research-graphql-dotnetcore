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

        AddQueryMethods(cm);

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

    private void AddQueryMethods(ClassMaker cm)
    {
        foreach (var m in Models)
        {
            AddQueryMethod(cm, m);
            AddCreateMethod(cm, m);
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

    private void AddQueryMethod(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        cm.AddClosure("protected async Task<List<" + m.Name + ">> QueryAll" + m.Name + "s()", liner =>
        {
            liner.Add("var query = \"{ \\\"query\\\": \\\"query { " + m.Name.ToLowerInvariant() + "s { " + GetQueryFields(m) +" } } \\\" }\";");
            liner.Add("var data = await PostRequest<All" + m.Name + "sQuery>(query);");
            liner.Add("return data." + m.Name + "s;");
        });
    }

    private void AddCreateMethod(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {

        //private async Task CreateCouch(string location)
        //{
        //    TestContext.WriteLine("doing mutation...");

        //    var query = \"{ \\\"query\\\": \\\"mutation {    createCouch(input: {      location: \\\"" + location + "\\\"    }) { id }  }\\\" }";


        //    TestContext.WriteLine("mutation response:" + content);
        //}

        cm.AddClosure("protected async Task<" + Config.IdType + "> Create" + m.Name + "(" + GetCreateMutationArguments(m) + ")", liner =>
        {
            liner.Add("var mutation = \"{ \\\"query\\\": \\\"mutation { create" + m.Name + "(input: {" + GetCreateMutationInput(m) + "}) { id } }\\\"}\";");
            liner.Add("var data = await PostRequest<MutationResponse>(mutation);");
            liner.Add("return data.Id;");
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
        var argumentize = new Func<string, string>(f => f + ": \\\"\" + " + f + " + \"\\\"");

        var foreignProperties = GetForeignProperties(m);
        var fields = m.Fields.Select(f => f.Name.ToLowerInvariant()).Select(argumentize);
        var foreignIds = foreignProperties.Select(f => f.ToLowerInvariant() + "Id").Select(argumentize);
        var all = fields.Concat(foreignIds);
        return string.Join(" ", all);
    }
}


//        private async Task<Cat[]> QueryAllCats()
//        {
//            var query = "{  \"query\": \"query {  cats {    id    name    age  }}\" }";
//            var response = await http.PostAsync("http://localhost/graphql/", new StringContent(query, Encoding.UTF8, "application/json"));
//            var content = await response.Content.ReadAsStringAsync();
//            var data = JsonConvert.DeserializeObject<GqlData<AllCatsQuery>>(content);
//            return data.Data.Cats;
//        }


//        private async Task<Couch[]> QueryAllCouches()
//        {
//            TestContext.WriteLine("doing query...");

//            var query = "{  \"query\": \"query {  couchs {    id    location  }}\" }";

//            var response = await http.PostAsync("http://localhost/graphql/",
//                new StringContent(query, Encoding.UTF8, "application/json"));

//            //response:{"data":{"cats":[]}}
//            var content = await response.Content.ReadAsStringAsync();
//            TestContext.WriteLine("response:" + content);

//            var data = JsonConvert.DeserializeObject<GqlData<AllCouchesQuery>>(content);
//            return data.Data.Couchs;
//        }

//        private async Task CreateCouch(string location)
//        {
//            TestContext.WriteLine("doing mutation...");

//            var query = "{ \"query\": \"mutation {    createCouch(input: {      location: \\\"" + location + "\\\"    }) { id }  }\" }";

//            var response = await http.PostAsync("http://localhost/graphql/",
//                new StringContent(query, Encoding.UTF8, "application/json"));

//            var content = await response.Content.ReadAsStringAsync();
//            TestContext.WriteLine("mutation response:" + content);
//        }





//using System;
//using System.Net.Http;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using gqldemo_generated;
//using Newtonsoft.Json;
//using NUnit.Framework;

//namespace test
//{
//    public class Tests : BaseGqlTest
//    {
//        private readonly HttpClient http = new HttpClient();

//        [SetUp]
//        public void Setup()
//        {
//            //Docker.Start();
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            //Docker.StopAndClean();
//        }


//        [Test]
//        public async Task Test1()
//        {
//            var couches = await QueryAllCouches();
//            foreach (var c in couches)
//            {
//                TestContext.WriteLine("Couch: " + c.Location);
//            }

//            await CreateCouch("Super!");

//            var updatedcouches = await QueryAllCouches();
//            foreach (var c in updatedcouches)
//            {
//                TestContext.WriteLine("Updated Couch: " + c.Location);
//            }

//            Assert.Fail();
//        }
//    }
//}
