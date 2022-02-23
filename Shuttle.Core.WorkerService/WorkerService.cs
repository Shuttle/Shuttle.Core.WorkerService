using System;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;

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

            try
            {
                service.Start();

                Console.WriteLine();
                ConsoleExtensions.WriteLine(ConsoleColor.Green, $"[started] : '{Assembly.GetEntryAssembly()?.FullName ?? "(could not find the entry assembly)"}'.");
                Console.WriteLine();
                ConsoleExtensions.WriteLine(ConsoleColor.DarkYellow, "[press ctrl+c to stop]");
                Console.WriteLine();

                Host.CreateDefaultBuilder()
                    .UseWindowsService()
                    .UseSystemd()
                    .Build()
                    .Run();

                (service as IServiceHostStop)?.Stop();
                (service as IDisposable)?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
                throw;
            }
        }

        private static bool CommandProcessed()
        {
            return new CommandProcessor().Execute();
        }
    }
}