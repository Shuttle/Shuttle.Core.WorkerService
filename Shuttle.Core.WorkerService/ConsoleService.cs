using System;
using System.Reflection;
using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Core.WorkerService
{
    public class ConsoleService
    {
        private readonly IServiceHostStart _service;

        public ConsoleService(IServiceHostStart service)
        {
            Guard.AgainstNull(service, nameof(service));

            _service = service;
        }

        public void Execute()
        {
            var waitHandle = new ManualResetEvent(false);
            var waitHandles = new WaitHandle[] { waitHandle };
            var stopping = false;

            Console.CancelKeyPress += (sender, e) =>
            {
                if (stopping)
                {
                    return;
                }

                ConsoleExtensions.WriteLine(ConsoleColor.Green, "[stopping]");

                waitHandle.Set();

                e.Cancel = true;
                stopping = true;
            };

            _service.Start();

            Console.WriteLine();
            ConsoleExtensions.WriteLine(ConsoleColor.Green, $"[started] : '{Assembly.GetEntryAssembly()?.FullName ?? "(could not find the entry assembly)"}'.");
            Console.WriteLine();
            ConsoleExtensions.WriteLine(ConsoleColor.DarkYellow, "[press ctrl+c to stop]");
            Console.WriteLine();

            WaitHandle.WaitAny(waitHandles);

            (_service as IServiceHostStop)?.Stop();
            (_service as IDisposable)?.Dispose();
        }
    }
}