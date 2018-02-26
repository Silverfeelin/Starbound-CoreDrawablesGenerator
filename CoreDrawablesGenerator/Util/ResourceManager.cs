using System.Text;

namespace CoreDrawablesGenerator
{
    public class ResourceManager
    {
        /// <summary>
        /// Parses a text resource, and returns it as a string.
        /// Can be used for <see cref="Properties.Resources"/> text resources.
        /// </summary>
        /// <param name="resource">Text resource bytes.</param>
        /// <returns>Text resource.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DecoderFallbackException"></exception>
        public static string TextResource(byte[] resource) => Encoding.Default.GetString(resource);
    }
}
