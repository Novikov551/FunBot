using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace FunBot.Configurations
{
    public static class EmbeddedConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddEmbeddedConfiguration(this IConfigurationBuilder builder, string path, bool optional = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path", "Входной параметр path не может быть равным null");
            }

            return builder.Add(delegate (EmbeddedConfigurationSource source)
            {
                source.Path = path;
                source.Optional = optional;
                source.ReloadOnChange = false;
                EmbeddedFileProvider embeddedFileProvider = (EmbeddedFileProvider)(source.FileProvider = new EmbeddedFileProvider(Assembly.GetEntryAssembly()));
            });
        }
    }
}
