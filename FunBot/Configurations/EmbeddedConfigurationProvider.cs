using Microsoft.Extensions.Configuration.Json;

namespace FunBot.Configurations
{
    internal sealed class EmbeddedConfigurationProvider : JsonConfigurationProvider, IRemoveConfigurationPath
    {
        public EmbeddedConfigurationProvider(JsonConfigurationSource source)
            : base(source)
        {
        }

        public void Remove(IEnumerable<string> paths)
        {
            Data = Data.Where((KeyValuePair<string, string> u) => !paths.Contains(u.Key)).ToDictionary((k) => k.Key, (v) => v.Value);
        }
    }
}
