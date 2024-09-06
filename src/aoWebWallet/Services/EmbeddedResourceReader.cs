using System.Reflection;

namespace aoWebWallet.Services
{
    public static class EmbeddedResourceReader
    {
        public static string ReadResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly == null)
                throw new NullReferenceException();

            using (Stream? stream = assembly!.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new Exception($"Resource '{resourceName}' not found.");
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
