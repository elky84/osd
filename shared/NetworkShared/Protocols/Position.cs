// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;
using global::System.Linq;
using global::System.IO;
using global::System.Text;

namespace FlatBuffers.Protocol
{
  public struct Position : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static Position GetRootAsPosition(ByteBuffer _bb) { return GetRootAsPosition(_bb, new Position()); }
    public static Position GetRootAsPosition(ByteBuffer _bb, Position obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public Position __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public double X { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }
    public double Y { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }
  
    public static Offset<Position> CreatePosition(FlatBufferBuilder builder,
        double x = 0.0,
        double y = 0.0) {
      builder.StartTable(2);
      Position.AddY(builder, y);
      Position.AddX(builder, x);
      return Position.EndPosition(builder);
    }
  
    public static void StartPosition(FlatBufferBuilder builder) { builder.StartTable(2); }
    public static void AddX(FlatBufferBuilder builder, double x) { builder.AddDouble(0, x, 0.0); }
    public static void AddY(FlatBufferBuilder builder, double y) { builder.AddDouble(1, y, 0.0); }
    public static Offset<Position> EndPosition(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<Position>(o);
    }
  
    public struct Model
    {
      public double X { get; set; }
      public double Y { get; set; }
    
      public Model(double x, double y)
      {
        X = x;
        Y = y;
      }
    }
  
    public static byte[] Bytes(double x, double y) {
      var builder = new FlatBufferBuilder(512);
    
      var offset = Position.CreatePosition(builder, x, y);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(typeof(Position).FullName.Length));
          writer.Write(Encoding.Default.GetBytes(typeof(Position).FullName));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
    
    public static byte[] Bytes(Model model) {
      return Bytes(model.X, model.Y);
    }
  };
}