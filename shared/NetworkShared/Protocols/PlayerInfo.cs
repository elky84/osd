// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;

public struct PlayerInfo : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static PlayerInfo GetRootAsPlayerInfo(ByteBuffer _bb) { return GetRootAsPlayerInfo(_bb, new PlayerInfo()); }
  public static PlayerInfo GetRootAsPlayerInfo(ByteBuffer _bb, PlayerInfo obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public PlayerInfo __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public string Name { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNameBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
  public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(4); }
#endif
  public byte[] GetNameArray() { return __p.__vector_as_array<byte>(4); }
  public string Name2 { get { int o = __p.__offset(6); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetName2Bytes() { return __p.__vector_as_span<byte>(6, 1); }
#else
  public ArraySegment<byte>? GetName2Bytes() { return __p.__vector_as_arraysegment(6); }
#endif
  public byte[] GetName2Array() { return __p.__vector_as_array<byte>(6); }
  public int Level { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }

  public static Offset<PlayerInfo> CreatePlayerInfo(FlatBufferBuilder builder,
      StringOffset nameOffset = default(StringOffset),
      StringOffset name2Offset = default(StringOffset),
      int level = 0) {
    builder.StartTable(3);
    PlayerInfo.AddLevel(builder, level);
    PlayerInfo.AddName2(builder, name2Offset);
    PlayerInfo.AddName(builder, nameOffset);
    return PlayerInfo.EndPlayerInfo(builder);
  }

  public static void StartPlayerInfo(FlatBufferBuilder builder) { builder.StartTable(3); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(0, nameOffset.Value, 0); }
  public static void AddName2(FlatBufferBuilder builder, StringOffset name2Offset) { builder.AddOffset(1, name2Offset.Value, 0); }
  public static void AddLevel(FlatBufferBuilder builder, int level) { builder.AddInt(2, level, 0); }
  public static Offset<PlayerInfo> EndPlayerInfo(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<PlayerInfo>(o);
  }

  public static byte[] Bytes(string name, string name2, int level) {
    var builder = new FlatBufferBuilder(512);
    var nameOffset = builder.CreateString(name);
    var name2Offset = builder.CreateString(name2);
    var offset = PlayerInfo.CreatePlayerInfo(builder, nameOffset, name2Offset, level);
    builder.Finish(offset.Value);
    return builder.DataBuffer.ToSizedArray();
  }
};

