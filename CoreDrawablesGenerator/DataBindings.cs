using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

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

    public class DataBindings
    {
        public List<Template> Templates { get; set; } = new List<Template>();
        public bool IgnoreWhite { get; set; } = true;
        public bool WeaponGroup { get; set; } = false;
        public bool InventoryIcon { get; set; } = false;
    }
}
