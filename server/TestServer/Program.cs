using System;
using Microsoft.Extensions.Configuration;
using ServerShared.Util;
using ServerShared.DotNetty;
using Serilog;

namespace TestServer
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
                ServerService.Register();

                var bootstrap = bootstrapHelper.Create();
                var listenTask = bootstrap.BindAsync(ServerShared.Config.ServerSettings.Port);
                listenTask.Wait();
                var channel = listenTask.Result;
                Log.Logger.Information("Server Started");
                Console.ReadLine();
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
