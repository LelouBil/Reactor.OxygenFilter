using System;
using System.IO;
using System.Threading.Tasks;
using DepotDownloader;
using Reactor.Greenhouse.Setup.Provider;
using Reactor.Greenhouse.Setup.Provider.Configurations;
using ItchProvider = Reactor.Greenhouse.Setup.Provider.ItchProvider;

namespace Reactor.Greenhouse.Setup
{
    public class GameManager
    {
        public string LatestPath;
        public string SourcePath;
        public string WorkPath { get; }

        public Game SourceProvider { get; private set; }
        public Game LatestProvider { get; private set; }


        public GameManager()
        {
            WorkPath = Path.GetFullPath("work");
        }

        public async Task SetupAsync(IProviderConfig sourceConfig, IProviderConfig latestConfig, string gameName)
        {
            SourcePath = Path.Join(WorkPath,"source");
            SourceProvider = sourceConfig.GetProvider(gameName, SourcePath);
            LatestPath = Path.Join(WorkPath,"latest");
            LatestProvider = latestConfig.GetProvider(gameName, LatestPath);

            await SourceProvider.DownloadAsync();

            await LatestProvider.DownloadAsync();
            
            SourceProvider.Dump();
            LatestProvider.Dump();



        }
    }
}
