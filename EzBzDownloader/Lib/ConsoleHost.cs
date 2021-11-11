using System;
using System.Threading;
using System.Threading.Tasks;

namespace EzBzDownloader.Lib
{
    /// <summary>
    /// https://gist.github.com/YahuiWong/a1bcb1e53e600c2c9f3942e0407c9a88
    /// </summary>
    public static class ConsoleHost
    {
        public static Task<int> WaitForShutdownAsync(Func<CancellationToken, Task<int>> process)
        {
            var done = new ManualResetEventSlim(false);
            using var cts = new CancellationTokenSource();
            AttachCtrlcSigtermShutdown(cts, done, string.Empty);

            try
            {
                return process(cts.Token);
            }
            finally
            {
                done.Set();
            }
        }

        private static void AttachCtrlcSigtermShutdown(CancellationTokenSource cts, ManualResetEventSlim resetEvent, string shutdownMessage)
        {
            void ShutDown()
            {
                if (!cts.IsCancellationRequested)
                {
                    if (!string.IsNullOrWhiteSpace(shutdownMessage))
                        Console.WriteLine(shutdownMessage);

                    try
                    {
                        cts.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Ignored
                    }
                }

                //Wait on the given reset event
                resetEvent.Wait();
            }

            AppDomain.CurrentDomain.ProcessExit += delegate { ShutDown(); };
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                ShutDown();
                //Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }
    }
}