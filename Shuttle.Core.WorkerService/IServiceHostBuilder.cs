using Microsoft.Extensions.Hosting;

namespace Shuttle.Core.WorkerService
{
    public interface IServiceHostBuilder
    {
        void Configure(IHostBuilder hostBuilder);
    }
}