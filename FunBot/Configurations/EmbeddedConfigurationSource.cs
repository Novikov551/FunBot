using Microsoft.Extensions.Configuration.Json;

namespace FunBot.Configurations
{
    public sealed class EmbeddedConfigurationSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new EmbeddedConfigurationProvider(this);
        }
    }
}
