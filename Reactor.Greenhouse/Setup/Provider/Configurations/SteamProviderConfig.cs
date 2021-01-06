using System.IO;

namespace Reactor.Greenhouse.Setup.Provider.Configurations
{
    public class SteamProviderConfig : IProviderConfig
    {
        public uint AppId {get; init; }
        
        public uint DepotId {get; init; }
        
        public ulong? Manifest {get; init; }
        public string Name => "Steam";
        
        public Game GetProvider(string gameName, string workPath)
        {
            return new(new SteamProvider()
            {
                AppId = AppId,
                DepotId = DepotId,
                Manifest = Manifest
            },gameName,workPath);
        }
        
        public override string ToString()
        {
            return $"Name : {Name}, AppId: {AppId}, DepotId: {DepotId}, Manifest: {Manifest}";
        }
    }
}