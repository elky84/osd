using ClientShared.Config;
using ClientShared.DotNetty;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TestClient.Model;

namespace TestClient
{
    class Program
    {
        public static async Task Main()
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
                var channel = await bootstrap.ConnectAsync(new IPEndPoint(ClientSettings.Host, ClientSettings.Port));
                var handler = channel.Pipeline.Get<GameHandler>();

                while (true)
                {
                    Console.Write("command : ");
                    var line = Console.ReadLine();
                    var splitted = line.Split(' ');

                    var cmd = splitted.First();
                    if (cmd.ToLower() == "quit")
                        break;

                    try
                    {
                        var parameters = splitted.Skip(1).ToArray();
                        handler.Command(cmd, parameters);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Information($"Command or Parameter Error. [Message:{ex.Message}] [Command:{cmd}]");
                    }
                }

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
                bootstrapHelper.GracefulClose().Wait();
            }
        }
    }
}
