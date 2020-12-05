using System.Linq;
// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;

public struct Move : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static Move GetRootAsMove(ByteBuffer _bb) { return GetRootAsMove(_bb, new Move()); }
  public static Move GetRootAsMove(ByteBuffer _bb, Move obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public Move __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public double X { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }
  public double Y { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }
  public long Now { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetLong(o + __p.bb_pos) : (long)0; } }
  public int Direction { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }

  public static Offset<Move> CreateMove(FlatBufferBuilder builder,
      double x = 0.0,
      double y = 0.0,
      long now = 0,
      int direction = 0) {
    builder.StartTable(4);
    Move.AddNow(builder, now);
    Move.AddY(builder, y);
    Move.AddX(builder, x);
    Move.AddDirection(builder, direction);
    return Move.EndMove(builder);
  }

  public static void StartMove(FlatBufferBuilder builder) { builder.StartTable(4); }
  public static void AddX(FlatBufferBuilder builder, double x) { builder.AddDouble(0, x, 0.0); }
  public static void AddY(FlatBufferBuilder builder, double y) { builder.AddDouble(1, y, 0.0); }
  public static void AddNow(FlatBufferBuilder builder, long now) { builder.AddLong(2, now, 0); }
  public static void AddDirection(FlatBufferBuilder builder, int direction) { builder.AddInt(3, direction, 0); }
  public static Offset<Move> EndMove(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<Move>(o);
  }

  public static byte[] Bytes(double x, double y, long now, int direction) {
    var builder = new FlatBufferBuilder(512);
  
    var offset = Move.CreateMove(builder, x, y, now, direction);
    builder.Finish(offset.Value);
    return builder.DataBuffer.ToSizedArray();
  }
};

