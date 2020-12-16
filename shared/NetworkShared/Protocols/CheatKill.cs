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
  public struct CheatKill : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static CheatKill GetRootAsCheatKill(ByteBuffer _bb) { return GetRootAsCheatKill(_bb, new CheatKill()); }
    public static CheatKill GetRootAsCheatKill(ByteBuffer _bb, CheatKill obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public CheatKill __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public int Sequence { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  
    public static Offset<CheatKill> CreateCheatKill(FlatBufferBuilder builder,
        int sequence = 0) {
      builder.StartTable(1);
      CheatKill.AddSequence(builder, sequence);
      return CheatKill.EndCheatKill(builder);
    }
  
    public static void StartCheatKill(FlatBufferBuilder builder) { builder.StartTable(1); }
    public static void AddSequence(FlatBufferBuilder builder, int sequence) { builder.AddInt(0, sequence, 0); }
    public static Offset<CheatKill> EndCheatKill(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<CheatKill>(o);
    }
  
    public struct Model
    {
      public int Sequence { get; set; }
    
      public Model(int sequence)
      {
        Sequence = sequence;
      }
    }
  
    public static byte[] Bytes(int sequence) {
      var builder = new FlatBufferBuilder(512);
    
      var offset = CheatKill.CreateCheatKill(builder, sequence);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(nameof(CheatKill).Length));
          writer.Write(Encoding.Default.GetBytes(nameof(CheatKill)));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
  };
}