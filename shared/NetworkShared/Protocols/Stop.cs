// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;
using System.Linq;

namespace FlatBuffers.Protocol
{
  public struct Stop : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static Stop GetRootAsStop(ByteBuffer _bb) { return GetRootAsStop(_bb, new Stop()); }
    public static Stop GetRootAsStop(ByteBuffer _bb, Stop obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public Stop __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public Position? Position { get { int o = __p.__offset(4); return o != 0 ? (Position?)(new Position()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
    public long Now { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetLong(o + __p.bb_pos) : (long)0; } }
  
    public static Offset<Stop> CreateStop(FlatBufferBuilder builder,
        Offset<Position> positionOffset = default(Offset<Position>),
        long now = 0) {
      builder.StartTable(2);
      Stop.AddNow(builder, now);
      Stop.AddPosition(builder, positionOffset);
      return Stop.EndStop(builder);
    }
  
    public static void StartStop(FlatBufferBuilder builder) { builder.StartTable(2); }
    public static void AddPosition(FlatBufferBuilder builder, Offset<Position> positionOffset) { builder.AddOffset(0, positionOffset.Value, 0); }
    public static void AddNow(FlatBufferBuilder builder, long now) { builder.AddLong(1, now, 0); }
    public static Offset<Stop> EndStop(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<Stop>(o);
    }
  
    public struct Model
    {
      public FlatBuffers.Protocol.Position.Model Position { get; set; }
      public long Now { get; set; }
    
      public Model(FlatBuffers.Protocol.Position.Model position, long now)
      {
        Position = position;
        Now = now;
      }
    }
  
    public static byte[] Bytes(FlatBuffers.Protocol.Position.Model position, long now) {
      var builder = new FlatBufferBuilder(512);
      var positionOffset = FlatBuffers.Protocol.Position.CreatePosition(builder, position.X, position.Y);
      var offset = Stop.CreateStop(builder, positionOffset, now);
      builder.Finish(offset.Value);
      return builder.DataBuffer.ToSizedArray();
    }
  };
}