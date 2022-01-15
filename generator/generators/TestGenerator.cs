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

    }

    private void CreateBaseGqlTestClass()
    {
        var fm = StartTestFile("BaseGqlTese");
        var cm = fm.AddClass("BaseGqlTest");
        cm.AddUsing("NUnit.Framework");
        cm.AddAttribute("Category(\"" + Config.Tests.TestCategory + "\")");
        cm.AddProperty("Docker")
            .IsType("DockerController")
            .Build();

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
}

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

//        private async Task<Cat[]> QueryAllCats()
//        {
//            TestContext.WriteLine("doing query...");

//            var query = "{  \"query\": \"query {  cats {    id    name    age  }}\" }";

//            var response = await http.PostAsync("http://localhost/graphql/",
//                new StringContent(query, Encoding.UTF8, "application/json"));

//            //response:{"data":{"cats":[]}}
//            var content = await response.Content.ReadAsStringAsync();
//            TestContext.WriteLine("response:" + content);

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

//        public class GqlData<T>
//        {
//            public T Data { get; set; }
//        }

//        public class AllCatsQuery
//        {
//            public Cat[] Cats { get; set; }
//        }

//        public class AllCouchesQuery
//        {
//            public Couch[] Couchs { get; set; }
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
