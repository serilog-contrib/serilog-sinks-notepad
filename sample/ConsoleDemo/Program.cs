using System;
using System.Threading;
using Serilog;

namespace ConsoleDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(s => Console.WriteLine($"Internal Error with Serilog: {s}"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Notepad()
                .CreateLogger();

            try
            {
                Console.WriteLine("Open a `notepad.exe` instance and press <enter> to continue...");
                Console.ReadLine();

                Console.WriteLine("Writing messages to the most recent Notepad you opened...");

                Log.Debug("Getting started");

                Log.Information("Hello {Name} from thread {ThreadId}", Environment.GetEnvironmentVariable("USERNAME"),
                    Thread.CurrentThread.ManagedThreadId);

                Log.Warning("No coins remain at position {@Position}", new { Lat = 25, Long = 134 });

                try
                {
                    Fail();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Oops... Something went wrong");
                }

                Console.WriteLine("Done.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void Fail()
        {
            throw new DivideByZeroException();
        }
    }
}
