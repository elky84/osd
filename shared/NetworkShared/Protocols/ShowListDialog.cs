using System.Linq;
// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;

public struct ShowListDialog : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static ShowListDialog GetRootAsShowListDialog(ByteBuffer _bb) { return GetRootAsShowListDialog(_bb, new ShowListDialog()); }
  public static ShowListDialog GetRootAsShowListDialog(ByteBuffer _bb, ShowListDialog obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public ShowListDialog __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public string Message { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetMessageBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
  public ArraySegment<byte>? GetMessageBytes() { return __p.__vector_as_arraysegment(4); }
#endif
  public byte[] GetMessageArray() { return __p.__vector_as_array<byte>(4); }
  public string Icon { get { int o = __p.__offset(6); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetIconBytes() { return __p.__vector_as_span<byte>(6, 1); }
#else
  public ArraySegment<byte>? GetIconBytes() { return __p.__vector_as_arraysegment(6); }
#endif
  public byte[] GetIconArray() { return __p.__vector_as_array<byte>(6); }
  public string List(int j) { int o = __p.__offset(8); return o != 0 ? __p.__string(__p.__vector(o) + j * 4) : null; }
  public int ListLength { get { int o = __p.__offset(8); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<ShowListDialog> CreateShowListDialog(FlatBufferBuilder builder,
      StringOffset messageOffset = default(StringOffset),
      StringOffset iconOffset = default(StringOffset),
      VectorOffset listOffset = default(VectorOffset)) {
    builder.StartTable(3);
    ShowListDialog.AddList(builder, listOffset);
    ShowListDialog.AddIcon(builder, iconOffset);
    ShowListDialog.AddMessage(builder, messageOffset);
    return ShowListDialog.EndShowListDialog(builder);
  }

  public static void StartShowListDialog(FlatBufferBuilder builder) { builder.StartTable(3); }
  public static void AddMessage(FlatBufferBuilder builder, StringOffset messageOffset) { builder.AddOffset(0, messageOffset.Value, 0); }
  public static void AddIcon(FlatBufferBuilder builder, StringOffset iconOffset) { builder.AddOffset(1, iconOffset.Value, 0); }
  public static void AddList(FlatBufferBuilder builder, VectorOffset listOffset) { builder.AddOffset(2, listOffset.Value, 0); }
  public static VectorOffset CreateListVector(FlatBufferBuilder builder, StringOffset[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateListVectorBlock(FlatBufferBuilder builder, StringOffset[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static void StartListVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<ShowListDialog> EndShowListDialog(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<ShowListDialog>(o);
  }

  public static byte[] Bytes(string message, string icon, List<string> list) {
    var builder = new FlatBufferBuilder(512);
    var messageOffset = builder.CreateString(message);
    var iconOffset = builder.CreateString(icon);
    var listOffset = CreateListVector(builder, list.Select(x => builder.CreateString(x)).ToArray());
    var offset = ShowListDialog.CreateShowListDialog(builder, messageOffset, iconOffset, listOffset);
    builder.Finish(offset.Value);
    return builder.DataBuffer.ToSizedArray();
  }
};

