using ClientShared.Config;
using ClientShared.DotNetty;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Net;
using TestClient.Model;

namespace TestClient
{
    class Program
    {
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var bootstrapHelper = new BootstrapHelper();
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("nlog.json", optional: true, reloadOnChange: true)
                    .Build();

                var bootstrap = bootstrapHelper.Create<GameHandler, Character>();

                var connectTask = bootstrap.ConnectAsync(new IPEndPoint(ClientSettings.Host, ClientSettings.Port));
                connectTask.Wait();
                var channel = connectTask.Result;

                channel.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                Log.Logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                bootstrapHelper.GracefulClose().Wait();
            }
        }
    }
}
