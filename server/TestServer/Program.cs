using FlatBuffers;
using Serilog;
using ServerShared.DotNetty;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using ServerShared.Util;
using System;

namespace TestServer
{
    class Session : BaseSession
    { }

    class GameHandler : BaseHandler<Session>
    {
        public GameHandler()
        {
        }

        [FlatBufferEvent]
        public bool OnPlayerInfo(Session session, PlayerInfo x)
        {
            return true;
        }
    }

    class Program
    {
        static void Main()
        {
            var handler = new GameHandler();
            
            var builder = new FlatBufferBuilder(512);
            var name = builder.CreateString("cshyeon");
            var offset = PlayerInfo.CreatePlayerInfo(builder, name, 123);
            PlayerInfo.FinishPlayerInfoBuffer(builder, offset);
            handler.Call<PlayerInfo>(null, builder.DataBuffer.ToSizedArray());


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
