﻿using System;
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
                var builder = Host.CreateDefaultBuilder();

                var serviceHostBuilder = service as IServiceHostBuilder;

                serviceHostBuilder?.Configure(builder);

                var build = builder
                    .UseWindowsService()
#if NETSTANDARD2_1_OR_GREATER
                    .UseSystemd()
#endif
                    .Build();

                service.Start(build.Services);

                Console.WriteLine();
                ConsoleExtensions.WriteLine(ConsoleColor.Green,
                    $"[started] : '{Assembly.GetEntryAssembly()?.FullName ?? "(could not find the entry assembly)"}'.");
                Console.WriteLine();
                ConsoleExtensions.WriteLine(ConsoleColor.DarkYellow, "[press ctrl+c to stop]");
                Console.WriteLine();

                build.Run();

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