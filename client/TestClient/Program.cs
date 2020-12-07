using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using ClientShared.Config;
using ClientShared.DotNetty;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using FlatBuffers;
using FlatBuffers.Protocol;
using Microsoft.Extensions.Configuration;
using NetworkShared.NetworkHandler;
using Serilog;
using TestClient.Handler;

namespace TestClient
{
    class Program
    {
        public class Character
        {
            public IChannelHandlerContext Context { get; set; }
        }

        public class GameHandler : BaseHandler<Character>
        {
            public Character Character { get; set; }

            public GameHandler()
            { }

            public override void ChannelInactive(DotNetty.Transport.Channels.IChannelHandlerContext context)
            {
                base.ChannelInactive(context);
            }

            [FlatBufferEvent]
            public bool OnMove(Move x)
            {
                Log.Logger.Information($"OnMove() {x.Position.Value.X} {x.Position.Value.Y} {x.Direction} {x.Now}");
                return true;
            }

            [FlatBufferEvent]
            public bool OnStop(Stop x)
            {
                Log.Logger.Information($"OnStop() {x.Position.Value.X} {x.Position.Value.Y} {x.Now}");
                return true;
            }

            [FlatBufferEvent]
            public bool OnClick(Click x)
            {
                Log.Logger.Information($"OnClick()");
                return true;
            }

            [FlatBufferEvent]
            public bool OnSelectListDialog(SelectListDialog x)
            {
                Log.Logger.Information($"OnSelectListDialog()");
                return true;
            }

            protected override void OnConnected(IChannelHandlerContext context)
            {
                Character = new Character { Context = context };

                var msg = Unpooled.Buffer();
                var bytes = Click.Bytes(0);
                msg.WriteInt(bytes.Length);
                var name = nameof(Click);
                msg.WriteByte(name.Length);
                msg.WriteString(name, Encoding.Default);
                msg.WriteBytes(bytes);
                Character.Context.WriteAndFlushAsync(msg);


                bytes = SelectListDialog.Bytes(3);
                msg = Unpooled.Buffer();
                msg.WriteInt(bytes.Length);
                name = nameof(SelectListDialog);
                msg.WriteByte(name.Length);
                msg.WriteString(name, Encoding.Default);
                msg.WriteBytes(bytes);
                Character.Context.WriteAndFlushAsync(msg);



                var position = new Point();
                var begin = DateTime.Now;
                bytes = Move.Bytes(new Position.Model(position.X, position.Y), begin.Ticks, 2);
                msg = Unpooled.Buffer();
                msg.WriteInt(bytes.Length);
                name = nameof(Move);
                msg.WriteByte(name.Length);
                msg.WriteString(name, Encoding.Default);
                msg.WriteBytes(bytes);
                Character.Context.WriteAndFlushAsync(msg);

                Log.Logger.Information("Started Client");
                Log.Logger.Information("Client is starting to right direction.");
                Log.Logger.Information("If you want to stop, input any words");

                var end = DateTime.Now;
                var diff = (end - begin).TotalMilliseconds;
                var speed = 10;
                position = new Point(position.X + (int)(0 + diff * (speed / 1000.0)), position.Y);

                bytes = Stop.Bytes(new Position.Model(position.X, position.Y), end.Ticks);
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
                Character.Context.WriteAndFlushAsync(msg);
            }

            protected override void OnDisconnected(IChannelHandlerContext context)
            {
            }
        }

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
