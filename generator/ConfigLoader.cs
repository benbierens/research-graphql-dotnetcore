using System.IO;
using Newtonsoft.Json;

public class ConfigLoader
{
    public GeneratorConfig Get(string[] args)
    {
        foreach (var s in args)
        {
            var c = TryParse(s);
            if (c != null) return c;
        }

        return TryParse("generatorConfig.json");
    }

    private GeneratorConfig TryParse(string filename)
    {
        try 
        {
            if (!File.Exists(filename)) return null;
            var lines = File.ReadAllLines(filename);
            return JsonConvert.DeserializeObject<GeneratorConfig>(string.Join(" ", lines));
        }
        catch
        {
            return null;
        }
    }
}

public class GeneratorConfig
{
    public class ConfigSection
    {
        public string ConnectionString {get;set;}
        public string GenerateNamespace {get;set;}
        public string Output {get;set;}
        public string IdType {get;set;}
    }
    
    public class ModelConfig
    {
        public string Name {get;set;}
        public ModelField[] Fields {get;set;}
        public string[] HasMany {get;set;}
    }

    public class ModelField
    {
        public string Name {get;set;}
        public string Type {get;set;}
    }

    public ConfigSection Config {get;set;}
    public ModelConfig[] Models {get;set;}
}