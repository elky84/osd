using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
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

                var position = new Point();
                var begin = DateTime.Now;
                var bytes = Move.Bytes(position.X, position.Y, begin.Ticks, 2);
                var msg = Unpooled.Buffer();
                msg.WriteInt(bytes.Length);
                var name = nameof(Move);
                msg.WriteByte(name.Length);
                msg.WriteString(name, Encoding.Default);
                msg.WriteBytes(bytes);
                channel.WriteAndFlushAsync(msg);
                Log.Logger.Information("Started Client");
                Console.WriteLine("Client is starting to right direction.");
                Console.WriteLine("If you want to stop, input any words");

                Console.ReadLine();

                var end = DateTime.Now;
                var diff = (end - begin).TotalMilliseconds;
                var speed = 10;
                position = new Point(position.X + (int)(0 + diff * (speed / 1000.0)), position.Y);

                bytes = Stop.Bytes(position.X, position.Y, end.Ticks);
                msg = Unpooled.Buffer();
                msg.WriteInt(bytes.Length);
                name = nameof(Stop);
                msg.WriteByte(name.Length);
                msg.WriteString(name, Encoding.Default);
                msg.WriteBytes(bytes);
                
                Console.WriteLine("Move start (TO RIGHT)");
                Console.WriteLine("Move done");
                Console.WriteLine($"Stop position : {position}");

                // 고의로 응답 늦춤
                Thread.Sleep(1000);
                channel.WriteAndFlushAsync(msg);
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
