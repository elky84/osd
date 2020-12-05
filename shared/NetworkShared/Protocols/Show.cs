// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;
using System.Linq;

namespace FlatBuffers.Protocol
{
  public struct Show : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static Show GetRootAsShow(ByteBuffer _bb) { return GetRootAsShow(_bb, new Show()); }
    public static Show GetRootAsShow(ByteBuffer _bb, Show obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public Show __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public int Sequence { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
    public Position? Position { get { int o = __p.__offset(6); return o != 0 ? (Position?)(new Position()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
  
    public static Offset<Show> CreateShow(FlatBufferBuilder builder,
        int sequence = 0,
        Offset<Position> positionOffset = default(Offset<Position>)) {
      builder.StartTable(2);
      Show.AddPosition(builder, positionOffset);
      Show.AddSequence(builder, sequence);
      return Show.EndShow(builder);
    }
  
    public static void StartShow(FlatBufferBuilder builder) { builder.StartTable(2); }
    public static void AddSequence(FlatBufferBuilder builder, int sequence) { builder.AddInt(0, sequence, 0); }
    public static void AddPosition(FlatBufferBuilder builder, Offset<Position> positionOffset) { builder.AddOffset(1, positionOffset.Value, 0); }
    public static Offset<Show> EndShow(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<Show>(o);
    }
    public static void FinishShowBuffer(FlatBufferBuilder builder, Offset<Show> offset) { builder.Finish(offset.Value); }
    public static void FinishSizePrefixedShowBuffer(FlatBufferBuilder builder, Offset<Show> offset) { builder.FinishSizePrefixed(offset.Value); }
  
    public struct Model
    {
      public int Sequence { get; set; }
      public FlatBuffers.Protocol.Position.Model Position { get; set; }
    
      public Model(int sequence, FlatBuffers.Protocol.Position.Model position)
      {
        Sequence = sequence;
        Position = position;
      }
    }
  
    public static byte[] Bytes(int sequence, FlatBuffers.Protocol.Position.Model position) {
      var builder = new FlatBufferBuilder(512);
      var positionOffset = FlatBuffers.Protocol.Position.CreatePosition(builder, position.X, position.Y);
      var offset = Show.CreateShow(builder, sequence, positionOffset);
      builder.Finish(offset.Value);
      return builder.DataBuffer.ToSizedArray();
    }
  };
}