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
  public struct Pickup : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static Pickup GetRootAsPickup(ByteBuffer _bb) { return GetRootAsPickup(_bb, new Pickup()); }
    public static Pickup GetRootAsPickup(ByteBuffer _bb, Pickup obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public Pickup __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
  
    public static void StartPickup(FlatBufferBuilder builder) { builder.StartTable(0); }
    public static Offset<Pickup> EndPickup(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<Pickup>(o);
    }
  
  
  
    public static byte[] Bytes() {
      var builder = new FlatBufferBuilder(512);
      StartPickup(builder);
      var offset = EndPickup(builder);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(typeof(Pickup).FullName.Length));
          writer.Write(Encoding.Default.GetBytes(typeof(Pickup).FullName));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
  };
}