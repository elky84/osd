// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;
using System.Linq;

public struct ShowDialog : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static ShowDialog GetRootAsShowDialog(ByteBuffer _bb) { return GetRootAsShowDialog(_bb, new ShowDialog()); }
  public static ShowDialog GetRootAsShowDialog(ByteBuffer _bb, ShowDialog obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public ShowDialog __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

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
  public bool Next { get { int o = __p.__offset(8); return o != 0 ? 0!=__p.bb.Get(o + __p.bb_pos) : (bool)false; } }
  public bool Quit { get { int o = __p.__offset(10); return o != 0 ? 0!=__p.bb.Get(o + __p.bb_pos) : (bool)false; } }

  public static Offset<ShowDialog> CreateShowDialog(FlatBufferBuilder builder,
      StringOffset messageOffset = default(StringOffset),
      StringOffset iconOffset = default(StringOffset),
      bool next = false,
      bool quit = false) {
    builder.StartTable(4);
    ShowDialog.AddIcon(builder, iconOffset);
    ShowDialog.AddMessage(builder, messageOffset);
    ShowDialog.AddQuit(builder, quit);
    ShowDialog.AddNext(builder, next);
    return ShowDialog.EndShowDialog(builder);
  }

  public static void StartShowDialog(FlatBufferBuilder builder) { builder.StartTable(4); }
  public static void AddMessage(FlatBufferBuilder builder, StringOffset messageOffset) { builder.AddOffset(0, messageOffset.Value, 0); }
  public static void AddIcon(FlatBufferBuilder builder, StringOffset iconOffset) { builder.AddOffset(1, iconOffset.Value, 0); }
  public static void AddNext(FlatBufferBuilder builder, bool next) { builder.AddBool(2, next, false); }
  public static void AddQuit(FlatBufferBuilder builder, bool quit) { builder.AddBool(3, quit, false); }
  public static Offset<ShowDialog> EndShowDialog(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<ShowDialog>(o);
  }

  public struct Model
  {
    public string Message { get; set; }
    public string Icon { get; set; }
    public bool Next { get; set; }
    public bool Quit { get; set; }
  
    public Model(string message, string icon, bool next, bool quit)
    {
      Message = message;
      Icon = icon;
      Next = next;
      Quit = quit;
    }
  }

  public static byte[] Bytes(string message, string icon, bool next, bool quit) {
    var builder = new FlatBufferBuilder(512);
    var messageOffset = builder.CreateString(message);
    var iconOffset = builder.CreateString(icon);
    var offset = ShowDialog.CreateShowDialog(builder, messageOffset, iconOffset, next, quit);
    builder.Finish(offset.Value);
    return builder.DataBuffer.ToSizedArray();
  }
};