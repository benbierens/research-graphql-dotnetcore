
using System;
using System.Collections.Generic;

public class BaseGqlTestClassGenerator : BaseGenerator
{
    public BaseGqlTestClassGenerator(GeneratorConfig config)
        : base(config)
    {
    }

    public void CreateBaseGqlTestClass()
    {
        var fm = StartTestUtilsFile("BaseGqlTest");
        AddBaseGqlTestClass(fm.AddClass("BaseGqlTest"));
        AddDockerInitializer(fm.AddClass("DockerInitializer"));
        fm.Build();
    }

    private void AddBaseGqlTestClass(ClassMaker cm)
    {
        cm.AddUsing("NUnit.Framework");
        cm.AddUsing("System.Threading.Tasks");

        cm.AddAttribute("Category(\"" + Config.Tests.TestCategory + "\")");

        cm.AddLine("[SetUp]");
        cm.AddClosure("public void GqlSetUp()", liner =>
        {
            liner.Add("TestData = new TestData();");
            liner.Add("DockerController.Up();");
        });

        cm.AddLine("[TearDown]");
        cm.AddClosure("public async Task GqlTearDown()", liner =>
        {
            liner.Add("await Gql.CloseActiveSubscriptionHandles();");
            liner.Add("DockerController.Down();");
        });

        cm.AddProperty("TestData")
            .IsType("TestData")
            .Build();

        cm.AddProperty("Gql")
            .IsType("Gql")
            .Build();

        cm.AddBlankLine();
        AddCreateTestModelMethods(cm);
    }

    private void AddCreateTestModelMethods(ClassMaker cm)
    {
        IterateModelsInDependencyOrder(m =>
        {
            var foreignProperties = GetForeignProperties(m);
            cm.AddClosure("public async Task CreateTest" + m.Name + "()", liner =>
            {
                var arguments = new List<string>();
                foreach (var f in foreignProperties)
                {
                    if (!f.IsSelfReference)
                    {
                        liner.Add("await CreateTest" + f.Type + "();");
                        arguments.Add("TestData.Test" + f.Type + ".Id");
                    }
                    else
                    {
                        arguments.Add("null");
                    }
                }

                var args = string.Join(", ", arguments);
                liner.Add("TestData.Test" + m.Name + ".Id = await Gql.Create" + m.Name + "(TestData.Test" + m.Name + ".ToCreate(" + args + "));");
            });
        });
    }

    private void AddDockerInitializer(ClassMaker cm)
    {
        cm.AddAttribute("SetUpFixture");
        cm.AddLine("[OneTimeSetUp]");
        cm.AddClosure("public void OneTimeGqlSetUp()", liner =>
        {
            liner.Add("DockerController.BuildImage();");
        });

        cm.AddLine("[OneTimeTearDown]");
        cm.AddClosure("public void OneTimeGqlTearDown()", liner =>
        {
            liner.Add("DockerController.DeleteImage();");
        });
    }
}