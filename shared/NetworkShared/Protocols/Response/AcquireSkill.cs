// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;
using global::System.Linq;
using global::System.IO;
using global::System.Text;

namespace FlatBuffers.Protocol.Response
{
  public struct AcquireSkill : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static AcquireSkill GetRootAsAcquireSkill(ByteBuffer _bb) { return GetRootAsAcquireSkill(_bb, new AcquireSkill()); }
    public static AcquireSkill GetRootAsAcquireSkill(ByteBuffer _bb, AcquireSkill obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public AcquireSkill __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public string Id { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
  #if ENABLE_SPAN_T
    public Span<byte> GetIdBytes() { return __p.__vector_as_span<byte>(4, 1); }
  #else
    public ArraySegment<byte>? GetIdBytes() { return __p.__vector_as_arraysegment(4); }
  #endif
    public byte[] GetIdArray() { return __p.__vector_as_array<byte>(4); }
  
    public static Offset<AcquireSkill> CreateAcquireSkill(FlatBufferBuilder builder,
        StringOffset idOffset = default(StringOffset)) {
      builder.StartTable(1);
      AcquireSkill.AddId(builder, idOffset);
      return AcquireSkill.EndAcquireSkill(builder);
    }
  
    public static void StartAcquireSkill(FlatBufferBuilder builder) { builder.StartTable(1); }
    public static void AddId(FlatBufferBuilder builder, StringOffset idOffset) { builder.AddOffset(0, idOffset.Value, 0); }
    public static Offset<AcquireSkill> EndAcquireSkill(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<AcquireSkill>(o);
    }
  
    public struct Model
    {
      public string Id { get; set; }
    
      public Model(string id)
      {
        Id = id;
      }
    }
  
    public static byte[] Bytes(string id) {
      var builder = new FlatBufferBuilder(512);
      var idOffset = builder.CreateString(id);
      var offset = AcquireSkill.CreateAcquireSkill(builder, idOffset);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(typeof(AcquireSkill).FullName.Length));
          writer.Write(Encoding.Default.GetBytes(typeof(AcquireSkill).FullName));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
    
    public static byte[] Bytes(Model model) {
      return Bytes(model.Id);
    }
  };
}