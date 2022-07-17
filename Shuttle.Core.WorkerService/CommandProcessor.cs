using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Shuttle.Core.Cli;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.WorkerService
{
    public class CommandProcessor
    {
        public bool Execute()
        {
            var result = false;

            try
            {
                var arguments = new Arguments(Environment.GetCommandLineArgs());

                if (ShouldShowHelp(arguments))
                {
                    ShowHelp();

                    return true;
                }

                if (arguments.Contains("debug"))
                {
                    Debugger.Launch();
                }
            }
            catch (Exception ex)
            {
                if (Environment.UserInteractive)
                {
                    ConsoleExtensions.WriteLine(ConsoleColor.Red, ex.AllMessages());

                    Console.WriteLine();
                    ConsoleExtensions.WriteLine(ConsoleColor.Gray, "Press any key to close...");
                    Console.ReadKey();
                }
                else
                {
                    throw;
                }

                result = true;
            }

            return result;
        }

        public bool ShouldShowHelp(Arguments arguments)
        {
            Guard.AgainstNull(arguments, "arguments");

            return arguments.Get("help", false) || arguments.Get("h", false) || arguments.Get("?", false);
        }

        protected static void ShowHelp()
        {
            try
            {
                using (
                    var stream =
                    Assembly.GetCallingAssembly()
                        .GetManifestResourceStream("Shuttle.Core.ServiceHost.Content.Help.txt"))
                {
                    if (stream == null)
                    {
                        Console.WriteLine("Error retrieving help content stream.");

                        return;
                    }

                    Console.WriteLine(new StreamReader(stream).ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}