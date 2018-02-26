using System.Text;

namespace CoreDrawablesGenerator
{
    public class ResourceManager
    {
        public static string TextResource(byte[] res)
        {
            return Encoding.Default.GetString(res);
        }
    }
}
