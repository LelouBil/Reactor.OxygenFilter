using Newtonsoft.Json;

namespace Reactor.Greenhouse.Setup.Provider.Configurations
{
    public interface IProviderConfig
    {
        [JsonIgnore]
        public string Name { get;}

        Game GetProvider(string gameName, string workPath);
    }
}