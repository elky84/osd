using MasterData;
using Serilog;
using ServerShared.DotNetty;
using System;
using System.Threading.Tasks;
using TestServer.Handler;

namespace TestServer
{
    class Program
    {
        static async Task Main()
        {
            MasterTable.Load("ServerShared");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var bootstrapHelper = new BootstrapHelper();
            try
            {
                var bootstrap = bootstrapHelper.Create(GameHandler.Instance);
                var channel = await bootstrap.BindAsync(ServerShared.Config.ServerSettings.Port);
                Log.Logger.Information("Server Started");
                Console.ReadLine();

                GameHandler.Instance.Release();
                await channel.CloseAsync();
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
                await bootstrapHelper.GracefulClose();
            }
        }
    }
}
