using Reactor.Greenhouse.Setup.Provider.Configurations;

namespace TestProject
{
    public class GreenhouseConfiguration
    {
        public string GameName { get; init; }

        public IProviderConfig SourceProviderConfig { get; set; }
        public IProviderConfig LatestProviderConfig { get; set; }

        public override string ToString()
        {
            return
                $"GameName : {GameName}, ProviderConfigType : {SourceProviderConfig.GetType().FullName}, ProviderConfig : {SourceProviderConfig.ToString()}";
        }
    }
}