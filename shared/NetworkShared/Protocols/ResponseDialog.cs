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
  public struct ResponseDialog : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static ResponseDialog GetRootAsResponseDialog(ByteBuffer _bb) { return GetRootAsResponseDialog(_bb, new ResponseDialog()); }
    public static ResponseDialog GetRootAsResponseDialog(ByteBuffer _bb, ResponseDialog obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public ResponseDialog __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public bool Next { get { int o = __p.__offset(4); return o != 0 ? 0!=__p.bb.Get(o + __p.bb_pos) : (bool)false; } }
  
    public static Offset<ResponseDialog> CreateResponseDialog(FlatBufferBuilder builder,
        bool next = false) {
      builder.StartTable(1);
      ResponseDialog.AddNext(builder, next);
      return ResponseDialog.EndResponseDialog(builder);
    }
  
    public static void StartResponseDialog(FlatBufferBuilder builder) { builder.StartTable(1); }
    public static void AddNext(FlatBufferBuilder builder, bool next) { builder.AddBool(0, next, false); }
    public static Offset<ResponseDialog> EndResponseDialog(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<ResponseDialog>(o);
    }
  
    public struct Model
    {
      public bool Next { get; set; }
    
      public Model(bool next)
      {
        Next = next;
      }
    }
  
    public static byte[] Bytes(bool next) {
      var builder = new FlatBufferBuilder(512);
    
      var offset = ResponseDialog.CreateResponseDialog(builder, next);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(nameof(ResponseDialog).Length));
          writer.Write(Encoding.Default.GetBytes(nameof(ResponseDialog)));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
  };
}