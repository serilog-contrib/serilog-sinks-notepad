#region Copyright 2020-2022 C. Augusto Proiete & Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

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
