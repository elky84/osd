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
  public struct ShowList : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static ShowList GetRootAsShowList(ByteBuffer _bb) { return GetRootAsShowList(_bb, new ShowList()); }
    public static ShowList GetRootAsShowList(ByteBuffer _bb, ShowList obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public ShowList __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public Object? Objects(int j) { int o = __p.__offset(4); return o != 0 ? (Object?)(new Object()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
    public int ObjectsLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }
  
    public static Offset<ShowList> CreateShowList(FlatBufferBuilder builder,
        VectorOffset objectsOffset = default(VectorOffset)) {
      builder.StartTable(1);
      ShowList.AddObjects(builder, objectsOffset);
      return ShowList.EndShowList(builder);
    }
  
    public static void StartShowList(FlatBufferBuilder builder) { builder.StartTable(1); }
    public static void AddObjects(FlatBufferBuilder builder, VectorOffset objectsOffset) { builder.AddOffset(0, objectsOffset.Value, 0); }
    public static VectorOffset CreateObjectsVector(FlatBufferBuilder builder, Offset<Object>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
    public static VectorOffset CreateObjectsVectorBlock(FlatBufferBuilder builder, Offset<Object>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
    public static void StartObjectsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
    public static Offset<ShowList> EndShowList(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<ShowList>(o);
    }
  
    public struct Model
    {
      public List<FlatBuffers.Protocol.Object.Model> Objects { get; set; }
    
      public Model(List<FlatBuffers.Protocol.Object.Model> objects)
      {
        Objects = objects;
      }
    }
  
    public static byte[] Bytes(List<FlatBuffers.Protocol.Object.Model> objects) {
      var builder = new FlatBufferBuilder(512);
      var objectsOffset = CreateObjectsVector(builder, objects.Select(x => FlatBuffers.Protocol.Object.CreateObject(builder, x.Sequence, FlatBuffers.Protocol.Position.CreatePosition(builder, x.Position.X, x.Position.Y))).ToArray());
      var offset = ShowList.CreateShowList(builder, objectsOffset);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(nameof(ShowList).Length));
          writer.Write(Encoding.Default.GetBytes(nameof(ShowList)));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
  };
}