﻿using System;
using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Newtonsoft.Json;
using UnityEngine;

public class NettyClient
{
    private static readonly Lazy<NettyClient> StaticInstance = new Lazy<NettyClient>(() => new NettyClient());

    public static NettyClient Instance { get { return StaticInstance.Value; } }

    public event Action OnClose;
    public event Action OnConnected;
    public event Action OnConnectFailed;

    private IChannel bootstrapChannel;
    private Bootstrap bootstrap;
    private MultithreadEventLoopGroup group;
    private string ip;
    private int port;

    public bool TryConnect = false;

    public NettyClient()
    {
    }

    public void Init(GameController gameController)
    {
        group = new MultithreadEventLoopGroup();
        bootstrap = new Bootstrap();
        bootstrap
            .Group(group)
            .Channel<TcpSocketChannel>()
            .Option(ChannelOption.TcpNodelay, true)
            .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(ByteOrder.BigEndian, 1024 * 1024, 0, 4, 0, 4, true));
                pipeline.AddLast(new LengthFieldPrepender(ByteOrder.BigEndian, 4, 0, false));

                pipeline.AddLast(new IdleStateHandler(0, 30, 0));
                var handler = new ClientHandler() { OnClose = OnCloseCallback };
                handler.BindEventHandler(gameController);
                pipeline.AddLast(handler);
            }));
    }

    public void SetDestination(string ip, int port)
    {
        this.ip = ip;
        this.port = port;
    }

    public async void Connect()
    {
        if (TryConnect)
        {
            return;
        }

        TryConnect = true;
        if (bootstrapChannel != null && bootstrapChannel.Active)
            await bootstrapChannel.CloseAsync();

        try
        {
            bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
            MainThreadDispatcher.Instance.Enqueue(() => OnConnected?.Invoke());
            TryConnect = false;
        }
        catch (Exception)
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnConnectFailedCallback();
            });
        }
    }

    void OnConnectFailedCallback()
    {
        TryConnect = false;
        MainThreadDispatcher.Instance.Enqueue(() => OnConnectFailed?.Invoke());
    }

    public async void ReConnect()
    {
        if (bootstrapChannel != null && bootstrapChannel.Active)
            await bootstrapChannel.CloseAsync();

        try
        {
            bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
            OnConnected?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void Send(byte[] data)
    {
        bootstrapChannel?.WriteAndFlushAsync(Unpooled.Buffer().WriteBytes(data));
    }

    void OnCloseCallback()
    {
        OnClose?.Invoke();
    }

    public void OnApplicationQuit()
    {
        bootstrapChannel?.CloseAsync();
        group.ShutdownGracefullyAsync().Wait(1000);
    }

    public void Close()
    {
        bootstrapChannel?.CloseAsync();
    }
    public IChannel GetChannel()
    {
        return bootstrapChannel;
    }
}
