using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;

namespace Shuttle.Core.WorkerService
{
    public sealed class ServiceHost
    {
        public static void Run<T>() where T : IServiceHostStart, new()
        {
            Run(new T());
        }

        public static void Run(IServiceHostStart service)
        {
            if (CommandProcessed())
            {
                return;
            }

            Guard.AgainstNull(service, nameof(service));

            var builder = Host.CreateDefaultBuilder();

            var serviceHostBuilder = service as IServiceHostBuilder;

            serviceHostBuilder?.Configure(builder);

            var host = builder
                .UseWindowsService()
#if NETSTANDARD2_1_OR_GREATER
                .UseSystemd()
#endif
                .Build();

            service.Start(host.Services);

            Console.WriteLine();
            ConsoleExtensions.WriteLine(ConsoleColor.Green,
                $"[starting] : '{Assembly.GetEntryAssembly()?.FullName ?? "(could not find the entry assembly)"}'.");
            Console.WriteLine();

            var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

            applicationLifetime.ApplicationStopping.Register(() =>
            {
                (service as IServiceHostStop)?.Stop();
                (service as IDisposable)?.Dispose();
            });

            host.Run();
        }

        private static bool CommandProcessed()
        {
            return new CommandProcessor().Execute();
        }
    }
}