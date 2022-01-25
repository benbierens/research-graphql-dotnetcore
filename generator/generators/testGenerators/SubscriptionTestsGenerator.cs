public class SubscriptionTestsGenerator : BaseGenerator
{
    public SubscriptionTestsGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void CreateSubscriptionTests()
    {
        var fm = StartTestFile("SubscriptionTests");
        var cm = fm.AddClass("SubscriptionTests");
        cm.AddUsing("NUnit.Framework");
        cm.AddUsing("System.Threading.Tasks");
        cm.AddUsing(Config.GenerateNamespace);
        cm.AddInherrit("BaseGqlTest");
        cm.Modifiers.Clear();

        cm.AddSubClass("CreateSubscriptionTests", createCm =>
        {
            createCm.AddInherrit("SubscriptionTests");
            createCm.Modifiers.Clear();
            foreach (var m in Models) AddCreateSubscriptionTest(cm, m);
        });

        cm.AddSubClass("UpdateSubscriptionTests", updateCm =>
        {
            updateCm.AddInherrit("SubscriptionTests");
            updateCm.Modifiers.Clear();
        });

        cm.AddSubClass("DeleteSubscriptionTests", deleteCm =>
        {
            deleteCm.AddInherrit("SubscriptionTests");
            deleteCm.Modifiers.Clear();
        });

        fm.Build();
    }

    private void AddCreateSubscriptionTest(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        cm.AddLine("[Test]");
        cm.AddClosure("public async Task ShouldPublishSubscriptionOnCreate" + m.Name + "()", liner =>
        {
            liner.Add("var handle = await Gql.SubscribeTo" + m.Name + Config.GraphQl.GqlSubscriptionCreatedMethod + "();");
            liner.AddBlankLine();
            liner.Add("await CreateTest" + m.Name + "();");
            liner.AddBlankLine();
            liner.Add("var entity = handle.AssertReceived();");
            foreach (var f in m.Fields)
            {
                liner.Add("Assert.That(entity." + f.Name + ", Is.EqualTo(TestData.Test" + m.Name + "." + f.Name + "), \"Incorrect " + m.Name + "." + f.Name + " received from subscription.\");");
            }
            var foreignProperties = GetForeignProperties(m);
            foreach (var f in foreignProperties)
            {
                if (!f.IsSelfReference)
                {
                    liner.Add("Assert.That(entity." + f.WithId + ", Is.EqualTo(TestData.Test" + f.Type + ".Id), \"Incorrect " + m.Name + "." + f.WithId + " received from subscription.\");");
                }
            }
        });
    }
}
