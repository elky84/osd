using System;
using System.IO;
using System.Net;
using ClientShared.Config;
using ClientShared.DotNetty;
using DotNetty.Buffers;
using FlatBuffers;
using Microsoft.Extensions.Configuration;
using Serilog;
using TestClient.Handler;

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
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("nlog.json", optional: true, reloadOnChange: true)
                    .Build();

                var bootstrap = bootstrapHelper.Create(new PacketDispatcher());
                var connectTask = bootstrap.ConnectAsync(new IPEndPoint(ClientSettings.Host, ClientSettings.Port));
                connectTask.Wait();
                var channel = connectTask.Result;

                var builder = new FlatBufferBuilder(512);
                var name = builder.CreateString("cshyeon");
                var offset = PlayerInfo.CreatePlayerInfo(builder, name, 123);
                PlayerInfo.FinishPlayerInfoBuffer(builder, offset);
                var bytes = builder.DataBuffer.ToSizedArray();

                using (var memoryStream = new MemoryStream())
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(bytes.Length);
                    binaryWriter.Write(nameof(PlayerInfo));
                    binaryWriter.Write(bytes);
                    binaryWriter.Flush();

                    var buffer = memoryStream.GetBuffer();

                    var msg = Unpooled.Buffer();
                    msg.WriteBytes(buffer);

                    channel.WriteAndFlushAsync(msg);
                }

                Log.Logger.Information("Started Client");
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
