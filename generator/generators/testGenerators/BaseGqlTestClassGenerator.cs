using System;
using System.Linq;

public class BaseGqlTestClassGenerator : BaseGenerator
{
    private readonly QueryAllMethodSubgenerator queryAllMethodSubgenerator;
    private readonly MutationMethodsSubgenerator mutationMethodsSubgenerator;
    private readonly SubscriptionMethodsSubgenerator subscriptionMethodsSubgenerator;

    public BaseGqlTestClassGenerator(GeneratorConfig config)
        : base(config)
    {
        queryAllMethodSubgenerator = new QueryAllMethodSubgenerator(config);
        mutationMethodsSubgenerator = new MutationMethodsSubgenerator(config);
        subscriptionMethodsSubgenerator = new SubscriptionMethodsSubgenerator(config);
    }
    
    public void CreateBaseGqlTestClass()
    {
        var fm = StartTestFile("BaseGqlTest");
        var cm = fm.AddClass("BaseGqlTest");
        cm.AddUsing("NUnit.Framework");
        cm.AddUsing("System");
        cm.AddUsing("System.Net.Http");
        cm.AddUsing("System.Text");
        cm.AddUsing("System.Threading.Tasks");
        cm.AddUsing("System.Collections.Generic");
        cm.AddUsing("Newtonsoft.Json");

        cm.AddAttribute("Category(\"" + Config.Tests.TestCategory + "\")");
        cm.AddProperty("Docker")
            .IsType("DockerController")
            .Build();

        cm.AddLine("private readonly HttpClient http = new HttpClient();");
        cm.AddLine("private readonly List<ISubscriptionHandle> handles = new List<ISubscriptionHandle>();");

        cm.AddBlankLine();
        cm.AddLine("[TearDown]");
        cm.AddClosure("public async Task GqlTearDown()", liner => 
        {
            liner.StartClosure("foreach (var h in handles)");
            liner.Add("await h.Unsubscribe();");
            liner.EndClosure();
            liner.Add("handles.Clear();");
        });

        AddModelMethods(cm);

        fm.Build();
    }
    
    private void AddModelMethods(ClassMaker cm)
    {
        foreach (var m in Models)
        {
            queryAllMethodSubgenerator.AddQueryAllMethod(cm, m);
            mutationMethodsSubgenerator.AddMutationMethods(cm, m);
            subscriptionMethodsSubgenerator.AddSubscribeMethods(cm, m);
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
    
    public class QueryAllMethodSubgenerator : BaseGenerator
    {
        public QueryAllMethodSubgenerator(GeneratorConfig config)
            : base(config)
        {
        }
        
        public void AddQueryAllMethod(ClassMaker cm, GeneratorConfig.ModelConfig m)
        {
            cm.AddClosure("protected async Task<List<" + m.Name + ">> QueryAll" + m.Name + "s()", liner =>
            {
                liner.Add("var query = \"{ \\\"query\\\": \\\"query { " + m.Name.ToLowerInvariant() + "s { " + GetQueryFields(m) +" } } \\\" }\";");
                liner.Add("var data = await PostRequest<All" + m.Name + "sQuery>(query);");
                liner.Add("return data." + m.Name + "s;");
            });
        }

        private string GetQueryFields(GeneratorConfig.ModelConfig m)
        {
            var foreignProperties = GetForeignProperties(m);
            var foreignIds = string.Join(" ", foreignProperties.Select(f => f.ToLowerInvariant() + "Id"));
            return "id " + string.Join(" ", m.Fields.Select(f => f.Name.ToLowerInvariant())) + " " + foreignIds;
        }
    }

    public class MutationMethodsSubgenerator : BaseGenerator
    {
        public MutationMethodsSubgenerator(GeneratorConfig config)
            : base(config)
        {
        }

        public void AddMutationMethods(ClassMaker cm, GeneratorConfig.ModelConfig m)
        {
            var inputNames = GetInputTypeNames(m);
            AddCreateMutationMethod(cm, m, inputNames);
            // update
            // delete
        }

        private void AddCreateMutationMethod(ClassMaker cm, GeneratorConfig.ModelConfig m, InputTypeNames inputNames)
        {
            cm.AddClosure("protected async Task<" + Config.IdType + "> Create" + m.Name + "(" + inputNames.Create + " input)", liner =>
            {
                liner.Add("var mutation = \"{ \\\"query\\\": \\\"mutation { create" + m.Name + "(input: {" + GetCreateMutationInput(m) + "}) { id } }\\\"}\";");
                liner.Add("var data = await PostRequest<MutationResponse>(mutation);");
                liner.Add("return data.Id;");
            });
        }

        private void AddUpdateMutationMethod(ClassMaker cm, GeneratorConfig.ModelConfig m, InputTypeNames inputNames)
        {
            cm.AddClosure("protected async Task<" + Config.IdType + "> Update" + m.Name + "(" + inputNames.Create + " input)", liner =>
            {
                this
                // liner.Add("var mutation = \"{ \\\"query\\\": \\\"mutation { create" + m.Name + "(input: {" + GetCreateMutationInput(m) + "}) { id } }\\\"}\";");
                // liner.Add("var data = await PostRequest<MutationResponse>(mutation);");
                // liner.Add("return data.Id;");
            });
        }

        private string GetCreateMutationInput(GeneratorConfig.ModelConfig m)
        {
            var argumentize = new Func<string, string>(f => FirstToLower(f) + ": \\\\\\\"\" + input." + f + " + \"\\\\\\\"");

            var foreignProperties = GetForeignProperties(m);
            var fields = m.Fields.Select(f => f.Name).Select(argumentize);
            var foreignIds = foreignProperties.Select(f => f + "Id").Select(argumentize);
            var all = fields.Concat(foreignIds);
            return string.Join(" ", all);
        }

        private string FirstToLower(string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0])) return str;
            return char.ToLower(str[0]) + str.Substring(1);
        }
    }

    public class SubscriptionMethodsSubgenerator : BaseGenerator
    {
        public SubscriptionMethodsSubgenerator(GeneratorConfig config)
            : base(config)
        {
        }

        public void AddSubscribeMethods(ClassMaker cm, GeneratorConfig.ModelConfig m)
        {
            AddSubscribeCreatedMethod(cm, m);
            // updated
            // deleted
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

        private string GetCreatedSubscriptionFields(GeneratorConfig.ModelConfig m)
        {
            var foreignProperties = GetForeignProperties(m);

            var fields = new []{ "\"id\""}
                .Concat(m.Fields.Select(f => "\"" + f.Name.ToLowerInvariant() + "\""))
                .Concat(foreignProperties.Select(f => "\"" + f.ToLowerInvariant() + "Id\""));

            return string.Join(", ", fields);
        }
    }
}
