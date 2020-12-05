// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;
using System.Linq;

public struct ShowConfirmDialog : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static ShowConfirmDialog GetRootAsShowConfirmDialog(ByteBuffer _bb) { return GetRootAsShowConfirmDialog(_bb, new ShowConfirmDialog()); }
  public static ShowConfirmDialog GetRootAsShowConfirmDialog(ByteBuffer _bb, ShowConfirmDialog obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public ShowConfirmDialog __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

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

  public static Offset<ShowConfirmDialog> CreateShowConfirmDialog(FlatBufferBuilder builder,
      StringOffset messageOffset = default(StringOffset),
      StringOffset iconOffset = default(StringOffset)) {
    builder.StartTable(2);
    ShowConfirmDialog.AddIcon(builder, iconOffset);
    ShowConfirmDialog.AddMessage(builder, messageOffset);
    return ShowConfirmDialog.EndShowConfirmDialog(builder);
  }

  public static void StartShowConfirmDialog(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddMessage(FlatBufferBuilder builder, StringOffset messageOffset) { builder.AddOffset(0, messageOffset.Value, 0); }
  public static void AddIcon(FlatBufferBuilder builder, StringOffset iconOffset) { builder.AddOffset(1, iconOffset.Value, 0); }
  public static Offset<ShowConfirmDialog> EndShowConfirmDialog(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<ShowConfirmDialog>(o);
  }

  public struct Model
  {
    public string Message { get; set; }
    public string Icon { get; set; }
  
    public Model(string message, string icon)
    {
      Message = message;
      Icon = icon;
    }
  }

  public static byte[] Bytes(string message, string icon) {
    var builder = new FlatBufferBuilder(512);
    var messageOffset = builder.CreateString(message);
    var iconOffset = builder.CreateString(icon);
    var offset = ShowConfirmDialog.CreateShowConfirmDialog(builder, messageOffset, iconOffset);
    builder.Finish(offset.Value);
    return builder.DataBuffer.ToSizedArray();
  }
};