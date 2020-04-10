using System;
using System.IO;
using System.Linq;

namespace GlobalArticleDatabaseAPI.Helpers
{
    public class ResourcesHelper
    {
        public static string GetResource(string resource)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(w => !w.IsDynamic))
            {
                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    if (resourceName.EndsWith(resource))
                    {
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
