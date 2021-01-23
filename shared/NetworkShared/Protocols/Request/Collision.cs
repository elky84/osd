// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;
using global::System.Linq;
using global::System.IO;
using global::System.Text;

namespace FlatBuffers.Protocol.Request
{
  public struct Collision : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static Collision GetRootAsCollision(ByteBuffer _bb) { return GetRootAsCollision(_bb, new Collision()); }
    public static Collision GetRootAsCollision(ByteBuffer _bb, Collision obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public Collision __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public Vector2? Position { get { int o = __p.__offset(4); return o != 0 ? (Vector2?)(new Vector2()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
    public int Axis { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  
    public static Offset<Collision> CreateCollision(FlatBufferBuilder builder,
        Offset<Vector2> positionOffset = default(Offset<Vector2>),
        int axis = 0) {
      builder.StartTable(2);
      Collision.AddAxis(builder, axis);
      Collision.AddPosition(builder, positionOffset);
      return Collision.EndCollision(builder);
    }
  
    public static void StartCollision(FlatBufferBuilder builder) { builder.StartTable(2); }
    public static void AddPosition(FlatBufferBuilder builder, Offset<Vector2> positionOffset) { builder.AddOffset(0, positionOffset.Value, 0); }
    public static void AddAxis(FlatBufferBuilder builder, int axis) { builder.AddInt(1, axis, 0); }
    public static Offset<Collision> EndCollision(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<Collision>(o);
    }
  
    public struct Model
    {
      public FlatBuffers.Protocol.Request.Vector2.Model Position { get; set; }
      public int Axis { get; set; }
    
      public Model(FlatBuffers.Protocol.Request.Vector2.Model position, int axis)
      {
        Position = position;
        Axis = axis;
      }
    }
  
    public static byte[] Bytes(FlatBuffers.Protocol.Request.Vector2.Model position, int axis) {
      var builder = new FlatBufferBuilder(512);
      var positionOffset = FlatBuffers.Protocol.Request.Vector2.CreateVector2(builder, position.X, position.Y);
      var offset = Collision.CreateCollision(builder, positionOffset, axis);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(typeof(Collision).FullName.Length));
          writer.Write(Encoding.Default.GetBytes(typeof(Collision).FullName));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
    
    public static byte[] Bytes(Model model) {
      return Bytes(model.Position, model.Axis);
    }
  };
}