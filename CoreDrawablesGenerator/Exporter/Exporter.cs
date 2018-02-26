using System;
using System.Collections.Generic;
using CoreDrawablesGenerator.Generator;
using Newtonsoft.Json.Linq;

namespace CoreDrawablesGenerator.Exporter
{
    public class Exporter : ICommandExporter, IDescriptorExporter
    {
        public virtual JObject Template { get; private set; }

        public DrawablesOutput Output { get; set; }

        public HashSet<string> Groups { get; } = new HashSet<string>();

        public Exporter(DrawablesOutput output, JObject template)
        {
            Template = template;
            Output = output;
        }

        public virtual string GetCommand(bool addInventoryIcon)
        {
            throw new NotImplementedException();
        }

        public virtual JObject GetDescriptor(bool addInventoryIcon)
        {
            JObject descriptor = (JObject)Template.DeepClone();

            if (descriptor["name"] == null)
                descriptor["name"] = "perfectlygenericitem";

            if (descriptor["count"] == null)
                descriptor["count"] = 1;

            if (descriptor["parameters"] == null)
                descriptor["parameters"] = new JObject();

            JObject parameters = (JObject)descriptor["parameters"];

            if (parameters == null)
                parameters = new JObject();
            if (parameters["animationCustom"] == null)
                parameters["animationCustom"] = new JObject();
            if (parameters["animationCustom"]["animatedParts"] == null)
                parameters["animationCustom"]["animatedParts"] = new JObject();
            if (parameters["animationCustom"]["animatedParts"]["parts"] == null)
                parameters["animationCustom"]["animatedParts"]["parts"] = new JObject();

            JToken parts = parameters["animationCustom"]["animatedParts"]["parts"];

            string prefix = "D_";
            int i = 1;

            JArray groups = new JArray();
            foreach (var item in Groups)
            {
                groups.Add(item);
            }

            foreach (Drawable item in Output.Drawables)
            {
                if (item == null) continue;
                JObject part = JObject.Parse("{'properties':{'centered':false,'offset':[0,0]}}");
                part["properties"]["image"] = item.ResultImage;
                part["properties"]["offset"][0] = item.BlockX + Math.Round(Output.OffsetX / 8d, 3);
                part["properties"]["offset"][1] = item.BlockY + Math.Round(Output.OffsetY / 8d, 3);
                part["properties"]["transformationGroups"] = groups;

                parts[prefix + i++] = part;
            }

            if (addInventoryIcon)
            {
                parameters["inventoryIcon"] = GenerateInventoryIcon();
            }

            return descriptor;
        }

        private JArray GenerateInventoryIcon()
        {
            JArray drawables = new JArray();

            for (int i = 0; i < Output.Drawables.GetLength(0); i++)
            {
                for (int j = 0; j < Output.Drawables.GetLength(1); j++)
                {
                    Drawable item = Output.Drawables[i, j];

                    if (item == null) continue;

                    JObject drawable = new JObject();
                    drawable["image"] = item.ResultImage;

                    bool cropH = false, cropV = false;
                    int hRest = 0, vRest = 0;
                    if (i == Output.Drawables.GetLength(0) - 1)
                    {
                        hRest = Output.ImageWidth % 32;
                        if (hRest != 0)
                            cropH = true;
                    }
                    if (j == Output.Drawables.GetLength(1) - 1)
                    {
                        vRest = Output.ImageHeight % 8;
                        if (vRest != 0)
                            cropH = true;
                    }

                    if (cropH || cropV)
                    {

                        drawable["image"] += "?crop;0;0;" + (cropH ? hRest : 32) + ";" + (cropV ? vRest : 8);
                    }

                    JArray position = new JArray();
                    position.Add(item.X);
                    position.Add(item.Y);
                    drawable["position"] = position;
                    drawables.Add(drawable);
                }
            }
            return drawables;
        }
    }
}
