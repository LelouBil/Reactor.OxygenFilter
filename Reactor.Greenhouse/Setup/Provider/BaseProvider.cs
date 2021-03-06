using System;
using System.Threading.Tasks;

namespace Reactor.Greenhouse.Setup.Provider
{
    public abstract class BaseProvider
    {
        public Game Game { get; internal set; }

        public abstract void Setup();
        public abstract Task DownloadAsync();
        public abstract bool IsUpdateNeeded();
    }

    public abstract class ProviderConnectionException : Exception
    {
        protected ProviderConnectionException(string message) : base(message)
        {
        }
    }
}