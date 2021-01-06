using System.IO;

namespace Reactor.Greenhouse.Setup.Provider.Configurations
{
    public class ItchProviderConfig : IProviderConfig
    {
        public int GameId {get; init; }
        
        public int GameVersion { get; init; }
        
        public string Name => "Itch";
        public Game GetProvider(string gameName, string workPath)
        {
            return new(new ItchProvider(),gameName,workPath);
        }

        public override string ToString()
        {
            return $"Name : {Name}, GameId: {GameId}, GameVersion: {GameVersion}";
        }
    }
}