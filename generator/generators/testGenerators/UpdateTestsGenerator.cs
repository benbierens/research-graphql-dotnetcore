﻿using System.Linq;

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
            if (m.Fields.Any())
            {
                AddUpdateTest(cm, m);
                AddUpdateAndQueryTest(cm, m);
            }
        }

        fm.Build();
    }

    private void AddUpdateTest(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        var inputTypes = GetInputTypeNames(m);

        cm.AddLine("[Test]");
        cm.AddClosure("public async Task UpdateShouldReturnUpdated" + m.Name + "()", liner =>
        {
            liner.Add("await CreateTest" + m.Name + "();");
            liner.AddBlankLine();

            liner.StartClosure("var entity = await Gql.Update" + m.Name + "(new " + inputTypes.Update);
            liner.Add(m.Name + "Id = TestData.Test" + m.Name + ".Id,");
            foreach (var f in m.Fields)
            {
                liner.Add(f.Name + " = TestData.Test" + f.Type.FirstToUpper() + ",");
            }
            liner.EndClosure(");");

            foreach (var f in m.Fields)
            {
                liner.Add("Assert.That(entity." + f.Name + ", Is.EqualTo(TestData.Test" + f.Type.FirstToUpper() + "), \"Update failed for " + m.Name + "." + f.Name + "\");");
            }
        });
    }

    private void AddUpdateAndQueryTest(ClassMaker cm, GeneratorConfig.ModelConfig m)
    {
        var inputTypes = GetInputTypeNames(m);

        cm.AddLine("[Test]");
        cm.AddClosure("public async Task QueryShouldReturnUpdated" + m.Name + "()", liner =>
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