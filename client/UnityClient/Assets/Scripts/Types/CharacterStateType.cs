using System;
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

public enum CharacterStateType
{
    Idle,
    Move,
    Attack,
    Hit
}