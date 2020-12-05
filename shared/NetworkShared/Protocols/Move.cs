// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;
using System.Linq;

namespace FlatBuffers.Protocol
{
  public struct Move : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static Move GetRootAsMove(ByteBuffer _bb) { return GetRootAsMove(_bb, new Move()); }
    public static Move GetRootAsMove(ByteBuffer _bb, Move obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public Move __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public Position? Position { get { int o = __p.__offset(4); return o != 0 ? (Position?)(new Position()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
    public long Now { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetLong(o + __p.bb_pos) : (long)0; } }
    public int Direction { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  
    public static Offset<Move> CreateMove(FlatBufferBuilder builder,
        Offset<Position> positionOffset = default(Offset<Position>),
        long now = 0,
        int direction = 0) {
      builder.StartTable(3);
      Move.AddNow(builder, now);
      Move.AddDirection(builder, direction);
      Move.AddPosition(builder, positionOffset);
      return Move.EndMove(builder);
    }
  
    public static void StartMove(FlatBufferBuilder builder) { builder.StartTable(3); }
    public static void AddPosition(FlatBufferBuilder builder, Offset<Position> positionOffset) { builder.AddOffset(0, positionOffset.Value, 0); }
    public static void AddNow(FlatBufferBuilder builder, long now) { builder.AddLong(1, now, 0); }
    public static void AddDirection(FlatBufferBuilder builder, int direction) { builder.AddInt(2, direction, 0); }
    public static Offset<Move> EndMove(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<Move>(o);
    }
  
    public struct Model
    {
      public FlatBuffers.Protocol.Position.Model Position { get; set; }
      public long Now { get; set; }
      public int Direction { get; set; }
    
      public Model(FlatBuffers.Protocol.Position.Model position, long now, int direction)
      {
        Position = position;
        Now = now;
        Direction = direction;
      }
    }
  
    public static byte[] Bytes(FlatBuffers.Protocol.Position.Model position, long now, int direction) {
      var builder = new FlatBufferBuilder(512);
      var positionOffset = FlatBuffers.Protocol.Position.CreatePosition(builder, position.X, position.Y);
      var offset = Move.CreateMove(builder, positionOffset, now, direction);
      builder.Finish(offset.Value);
      return builder.DataBuffer.ToSizedArray();
    }
  };
}