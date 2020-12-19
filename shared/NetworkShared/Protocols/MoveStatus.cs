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
  public struct MoveStatus : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static MoveStatus GetRootAsMoveStatus(ByteBuffer _bb) { return GetRootAsMoveStatus(_bb, new MoveStatus()); }
    public static MoveStatus GetRootAsMoveStatus(ByteBuffer _bb, MoveStatus obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public MoveStatus __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public Position? Position { get { int o = __p.__offset(4); return o != 0 ? (Position?)(new Position()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
    public bool Moving { get { int o = __p.__offset(6); return o != 0 ? 0!=__p.bb.Get(o + __p.bb_pos) : (bool)false; } }
    public int Direction { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  
    public static Offset<MoveStatus> CreateMoveStatus(FlatBufferBuilder builder,
        Offset<Position> positionOffset = default(Offset<Position>),
        bool moving = false,
        int direction = 0) {
      builder.StartTable(3);
      MoveStatus.AddDirection(builder, direction);
      MoveStatus.AddPosition(builder, positionOffset);
      MoveStatus.AddMoving(builder, moving);
      return MoveStatus.EndMoveStatus(builder);
    }
  
    public static void StartMoveStatus(FlatBufferBuilder builder) { builder.StartTable(3); }
    public static void AddPosition(FlatBufferBuilder builder, Offset<Position> positionOffset) { builder.AddOffset(0, positionOffset.Value, 0); }
    public static void AddMoving(FlatBufferBuilder builder, bool moving) { builder.AddBool(1, moving, false); }
    public static void AddDirection(FlatBufferBuilder builder, int direction) { builder.AddInt(2, direction, 0); }
    public static Offset<MoveStatus> EndMoveStatus(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<MoveStatus>(o);
    }
  
    public struct Model
    {
      public FlatBuffers.Protocol.Position.Model Position { get; set; }
      public bool Moving { get; set; }
      public int Direction { get; set; }
    
      public Model(FlatBuffers.Protocol.Position.Model position, bool moving, int direction)
      {
        Position = position;
        Moving = moving;
        Direction = direction;
      }
    }
  
    public static byte[] Bytes(FlatBuffers.Protocol.Position.Model position, bool moving, int direction) {
      var builder = new FlatBufferBuilder(512);
      var positionOffset = FlatBuffers.Protocol.Position.CreatePosition(builder, position.X, position.Y);
      var offset = MoveStatus.CreateMoveStatus(builder, positionOffset, moving, direction);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(nameof(MoveStatus).Length));
          writer.Write(Encoding.Default.GetBytes(nameof(MoveStatus)));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
    
    public static byte[] Bytes(Model model) {
      return Bytes(model.Position, model.Moving, model.Direction);
    }
  };
}