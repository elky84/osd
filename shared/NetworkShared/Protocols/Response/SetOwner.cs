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
  public struct SetOwner : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static SetOwner GetRootAsSetOwner(ByteBuffer _bb) { return GetRootAsSetOwner(_bb, new SetOwner()); }
    public static SetOwner GetRootAsSetOwner(ByteBuffer _bb, SetOwner obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public SetOwner __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public int Sequences(int j) { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(__p.__vector(o) + j * 4) : (int)0; }
    public int SequencesLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }
  #if ENABLE_SPAN_T
    public Span<int> GetSequencesBytes() { return __p.__vector_as_span<int>(4, 4); }
  #else
    public ArraySegment<byte>? GetSequencesBytes() { return __p.__vector_as_arraysegment(4); }
  #endif
    public int[] GetSequencesArray() { return __p.__vector_as_array<int>(4); }
  
    public static Offset<SetOwner> CreateSetOwner(FlatBufferBuilder builder,
        VectorOffset sequencesOffset = default(VectorOffset)) {
      builder.StartTable(1);
      SetOwner.AddSequences(builder, sequencesOffset);
      return SetOwner.EndSetOwner(builder);
    }
  
    public static void StartSetOwner(FlatBufferBuilder builder) { builder.StartTable(1); }
    public static void AddSequences(FlatBufferBuilder builder, VectorOffset sequencesOffset) { builder.AddOffset(0, sequencesOffset.Value, 0); }
    public static VectorOffset CreateSequencesVector(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddInt(data[i]); return builder.EndVector(); }
    public static VectorOffset CreateSequencesVectorBlock(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
    public static void StartSequencesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
    public static Offset<SetOwner> EndSetOwner(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<SetOwner>(o);
    }
  
    public struct Model
    {
      public List<int> Sequences { get; set; }
    
      public Model(List<int> sequences)
      {
        Sequences = sequences;
      }
    }
  
    public static byte[] Bytes(List<int> sequences) {
      var builder = new FlatBufferBuilder(512);
      var sequencesOffset = FlatBuffers.Protocol.Response.SetOwner.CreateSequencesVector(builder, sequences.ToArray());
      var offset = SetOwner.CreateSetOwner(builder, sequencesOffset);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(typeof(SetOwner).FullName.Length));
          writer.Write(Encoding.Default.GetBytes(typeof(SetOwner).FullName));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
    
    public static byte[] Bytes(Model model) {
      return Bytes(model.Sequences);
    }
  };
}