using Newtonsoft.Json.Linq;

namespace CoreDrawablesGenerator
{
    public class Template
    {
        public string Key { get; set; }
        public JObject Config { get; set; }

        public Template(string key, JObject template)
        {
            Key = key;
            Config = template;
        }

        public Template(string key, string template) : this(key, JObject.Parse(template)) { }

        public override string ToString()
        {
            return Key;
        }
    }
}
