using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DepotDownloader;
using SteamKit2;

namespace Reactor.Greenhouse.Setup.Provider
{
    public class SteamProvider : BaseProvider
    {
        public uint AppId { get; init; } = 945360;
        public uint DepotId { get; init; } = 945361;
        public ulong? Manifest { get; init; } = 3596575937380717449;


        public SteamProvider()
        {
        }

        public ulong GetLatestManifest()
        {
            DepotConfigStore.LoadFromFile(Path.Combine(Game.Path, ".DepotDownloader", "depot.config"));

            ContentDownloader.steam3!.RequestAppInfo(AppId);

            var depots = ContentDownloader.GetSteam3AppSection(AppId, EAppInfoSection.Depots);
            var latestManifest = depots[DepotId.ToString()]["manifests"][ContentDownloader.DEFAULT_BRANCH].AsUnsignedLong();

            return latestManifest;
        }

        public override bool IsUpdateNeeded()
        {
            if (DepotConfigStore.Instance.InstalledManifestIDs.TryGetValue(DepotId, out var installedManifest))
            {
                return GetLatestManifest() != installedManifest;
            }

            return true;
        }

        public override void Setup()
        {
            if (ContentDownloader.steam3 != null && ContentDownloader.steam3.bConnected)
            {
                return;
            }

            AccountSettingsStore.LoadFromFile("account.config");

            var environmentVariable = Environment.GetEnvironmentVariable("STEAM");

            if (environmentVariable != null)
            {
                var split = environmentVariable.Split(":");
                if (!ContentDownloader.InitializeSteam3(split[0], split[1]))
                {
                    throw new SteamProviderConnectionException("Wrong password");
                }

                if (ContentDownloader.steam3 == null || !ContentDownloader.steam3.bConnected)
                {
                    throw new SteamProviderConnectionException("Unable to initialize Steam");
                }
            }
            else
            {
                ContentDownloader.Config.RememberPassword = true;

                Console.Write("Steam username: ");
                var username = Console.ReadLine();

                string password = null;

                if (!AccountSettingsStore.Instance.LoginKeys.ContainsKey(username))
                {
                    Console.Write("Steam password: ");
                    password = ContentDownloader.Config.SuppliedPassword = Util.ReadPassword();
                    Console.WriteLine();
                }

                if (!ContentDownloader.InitializeSteam3(username, password))
                {
                    throw new SteamProviderConnectionException("Unable to initialize Steam");
                }
            }

            ContentDownloader.Config.UsingFileList = true;
            ContentDownloader.Config.FilesToDownload = new List<string>
            {
                "GameAssembly.dll"
            };
            ContentDownloader.Config.FilesToDownloadRegex = new List<Regex>
            {
                new Regex(@"^(.+)_Data[\\|/]il2cpp_data[\\|/]Metadata[\\|/]global-metadata.dat$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^(.+)_Data[\\|/]globalgamemanagers$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
        }

        public override Task DownloadAsync()
        {
            ContentDownloader.Config.InstallDirectory = Game.Path;
            return ContentDownloader.DownloadAppAsync(AppId, DepotId, Manifest ?? GetLatestManifest());
        }
    }

    public class SteamProviderConnectionException : ProviderConnectionException
    {
        public SteamProviderConnectionException(string message) : base(message)
        {
        }
    }
}