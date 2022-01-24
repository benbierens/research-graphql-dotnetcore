public class UpdateTestsGenerator : BaseGenerator
{
    public UpdateTestsGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void CreateUpdateTests()
    {
        var fm = StartTestFile("UpdateTests");
        var cm = fm.AddClass("UpdateTests");
        cm.AddUsing("NUnit.Framework");
        cm.AddUsing("System.Threading.Tasks");
        cm.AddUsing(Config.GenerateNamespace);
        cm.AddInherrit("BaseGqlTest");
        cm.Modifiers.Clear();

        foreach (var m in Models)
        {
            AddUpdateAllTest(cm, m);
        }

        fm.Build();
    }

    //[Test]
    //public async Task ShouldSubscribeToUser()
    //{
    //    await CreateTestUser();

    //    await Gql.UpdateUser(new UpdateUserInput
    //    {
    //        UserId = TestData.TestUser.Id,
    //        Name = TestData.TestString
    //    });

    //    var allUsers = await Gql.QueryAllUsers();
    //    Assert.That(allUsers.Count, Is.EqualTo(1));
    //    Assert.That(allUsers[0].Name, Is.EqualTo(TestData.TestString));
    //}

    private void AddUpdateAllTest(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        var inputTypes = GetInputTypeNames(m);

        cm.AddLine("[Test]");
        cm.AddClosure("public async Task ShouldUpdate" + m.Name + "()", liner =>
        {
            liner.Add("await CreateTest" + m.Name + "();");
            liner.AddBlankLine();

            liner.StartClosure("await Gql.Update" + m.Name + "(new " + inputTypes.Update);
            liner.Add(m.Name + "Id = TestData.Test" + m.Name + ".Id,");
            foreach (var f in m.Fields)
            {
                liner.Add(f.Name + " = TestData.Test" + f.Type.FirstToUpper() + ",");
            }
            liner.EndClosure(");");

            liner.Add("var all = await Gql.QueryAll" + m.Name + "s();");
            liner.Add("Assert.That(all.Count, Is.EqualTo(1), \"Expected only 1 " + m.Name + "\");");
            foreach (var f in m.Fields)
            {
                liner.Add("Assert.That(all[0]." + f.Name + ", Is.EqualTo(TestData.Test" + f.Type.FirstToUpper() + "), \"Update failed for " + m.Name + "." + f.Name + "\");");
            }
        });
    }
}
