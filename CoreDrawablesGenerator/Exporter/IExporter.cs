using Newtonsoft.Json.Linq;

namespace CoreDrawablesGenerator.Exporter
{
    public interface ICommandExporter
    {
        string GetCommand(bool addInventoryIcon);
    }

    public interface IDescriptorExporter
    {
        JObject GetDescriptor(bool addInventoryIcon);
    }
}
