using System.IO;
using System.Reflection;
using System.Text;

namespace CoreDrawablesGenerator
{
    public class ResourceManager
    {
        /// <summary>
        /// Parses text bytes, and returns it as a string.
        /// Can be used for <see cref="Properties.Resources"/> text resources.
        /// </summary>
        /// <param name="resource">Text resource bytes.</param>
        /// <returns>Text resource.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DecoderFallbackException"></exception>
        public static string TextResource(byte[] resource)
        {
            return Encoding.Default.GetString(resource);
        }

        /// <summary>
        /// Fetches a text resource from the assemby.
        /// </summary>
        /// <param name="assembly">Assembly the resource resides in.</param>
        /// <param name="name">Name/Assembly path of the resource.</param>
        /// <returns></returns>
        public static string TextResource(Assembly assembly, string name)
        {
            using (Stream resource = assembly.GetManifestResourceStream(name))
            {
                byte[] resBytes = new byte[resource.Length];
                resource.Read(resBytes, 0, resBytes.Length);
                return TextResource(resBytes);
            }
        }
    }
}
