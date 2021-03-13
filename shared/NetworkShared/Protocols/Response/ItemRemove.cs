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
  public struct ItemRemove : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static ItemRemove GetRootAsItemRemove(ByteBuffer _bb) { return GetRootAsItemRemove(_bb, new ItemRemove()); }
    public static ItemRemove GetRootAsItemRemove(ByteBuffer _bb, ItemRemove obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public ItemRemove __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public string Name { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
  #if ENABLE_SPAN_T
    public Span<byte> GetNameBytes() { return __p.__vector_as_span<byte>(4, 1); }
  #else
    public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(4); }
  #endif
    public byte[] GetNameArray() { return __p.__vector_as_array<byte>(4); }
  
    public static Offset<ItemRemove> CreateItemRemove(FlatBufferBuilder builder,
        StringOffset nameOffset = default(StringOffset)) {
      builder.StartTable(1);
      ItemRemove.AddName(builder, nameOffset);
      return ItemRemove.EndItemRemove(builder);
    }
  
    public static void StartItemRemove(FlatBufferBuilder builder) { builder.StartTable(1); }
    public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(0, nameOffset.Value, 0); }
    public static Offset<ItemRemove> EndItemRemove(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<ItemRemove>(o);
    }
  
    public struct Model
    {
      public string Name { get; set; }
    
      public Model(string name)
      {
        Name = name;
      }
    }
  
    public static byte[] Bytes(string name) {
      var builder = new FlatBufferBuilder(512);
      var nameOffset = builder.CreateString(name);
      var offset = ItemRemove.CreateItemRemove(builder, nameOffset);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(typeof(ItemRemove).FullName.Length));
          writer.Write(Encoding.Default.GetBytes(typeof(ItemRemove).FullName));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
    
    public static byte[] Bytes(Model model) {
      return Bytes(model.Name);
    }
  };
}