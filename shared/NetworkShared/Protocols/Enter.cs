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
  public struct Enter : IFlatbufferObject
  {
    private Table __p;
    public ByteBuffer ByteBuffer { get { return __p.bb; } }
    public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
    public static Enter GetRootAsEnter(ByteBuffer _bb) { return GetRootAsEnter(_bb, new Enter()); }
    public static Enter GetRootAsEnter(ByteBuffer _bb, Enter obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
    public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
    public Enter __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }
  
    public int Sequence { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
    public Map? Map { get { int o = __p.__offset(6); return o != 0 ? (Map?)(new Map()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
    public Position? Position { get { int o = __p.__offset(8); return o != 0 ? (Position?)(new Position()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
    public int Direction { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
    public Object? Objects(int j) { int o = __p.__offset(12); return o != 0 ? (Object?)(new Object()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
    public int ObjectsLength { get { int o = __p.__offset(12); return o != 0 ? __p.__vector_len(o) : 0; } }
    public Portal? Portals(int j) { int o = __p.__offset(14); return o != 0 ? (Portal?)(new Portal()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
    public int PortalsLength { get { int o = __p.__offset(14); return o != 0 ? __p.__vector_len(o) : 0; } }
  
    public static Offset<Enter> CreateEnter(FlatBufferBuilder builder,
        int sequence = 0,
        Offset<Map> mapOffset = default(Offset<Map>),
        Offset<Position> positionOffset = default(Offset<Position>),
        int direction = 0,
        VectorOffset objectsOffset = default(VectorOffset),
        VectorOffset portalsOffset = default(VectorOffset)) {
      builder.StartTable(6);
      Enter.AddPortals(builder, portalsOffset);
      Enter.AddObjects(builder, objectsOffset);
      Enter.AddDirection(builder, direction);
      Enter.AddPosition(builder, positionOffset);
      Enter.AddMap(builder, mapOffset);
      Enter.AddSequence(builder, sequence);
      return Enter.EndEnter(builder);
    }
  
    public static void StartEnter(FlatBufferBuilder builder) { builder.StartTable(6); }
    public static void AddSequence(FlatBufferBuilder builder, int sequence) { builder.AddInt(0, sequence, 0); }
    public static void AddMap(FlatBufferBuilder builder, Offset<Map> mapOffset) { builder.AddOffset(1, mapOffset.Value, 0); }
    public static void AddPosition(FlatBufferBuilder builder, Offset<Position> positionOffset) { builder.AddOffset(2, positionOffset.Value, 0); }
    public static void AddDirection(FlatBufferBuilder builder, int direction) { builder.AddInt(3, direction, 0); }
    public static void AddObjects(FlatBufferBuilder builder, VectorOffset objectsOffset) { builder.AddOffset(4, objectsOffset.Value, 0); }
    public static VectorOffset CreateObjectsVector(FlatBufferBuilder builder, Offset<Object>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
    public static VectorOffset CreateObjectsVectorBlock(FlatBufferBuilder builder, Offset<Object>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
    public static void StartObjectsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
    public static void AddPortals(FlatBufferBuilder builder, VectorOffset portalsOffset) { builder.AddOffset(5, portalsOffset.Value, 0); }
    public static VectorOffset CreatePortalsVector(FlatBufferBuilder builder, Offset<Portal>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
    public static VectorOffset CreatePortalsVectorBlock(FlatBufferBuilder builder, Offset<Portal>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
    public static void StartPortalsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
    public static Offset<Enter> EndEnter(FlatBufferBuilder builder) {
      int o = builder.EndTable();
      return new Offset<Enter>(o);
    }
  
    public struct Model
    {
      public int Sequence { get; set; }
      public FlatBuffers.Protocol.Map.Model Map { get; set; }
      public FlatBuffers.Protocol.Position.Model Position { get; set; }
      public int Direction { get; set; }
      public List<FlatBuffers.Protocol.Object.Model> Objects { get; set; }
      public List<FlatBuffers.Protocol.Portal.Model> Portals { get; set; }
    
      public Model(int sequence, FlatBuffers.Protocol.Map.Model map, FlatBuffers.Protocol.Position.Model position, int direction, List<FlatBuffers.Protocol.Object.Model> objects, List<FlatBuffers.Protocol.Portal.Model> portals)
      {
        Sequence = sequence;
        Map = map;
        Position = position;
        Direction = direction;
        Objects = objects;
        Portals = portals;
      }
    }
  
    public static byte[] Bytes(int sequence, FlatBuffers.Protocol.Map.Model map, FlatBuffers.Protocol.Position.Model position, int direction, List<FlatBuffers.Protocol.Object.Model> objects, List<FlatBuffers.Protocol.Portal.Model> portals) {
      var builder = new FlatBufferBuilder(512);
      var mapOffset = FlatBuffers.Protocol.Map.CreateMap(builder, builder.CreateString(map.Name));
      var positionOffset = FlatBuffers.Protocol.Position.CreatePosition(builder, position.X, position.Y);
      var objectsOffset = CreateObjectsVector(builder, objects.Select(x => FlatBuffers.Protocol.Object.CreateObject(builder, x.Sequence, builder.CreateString(x.Name), x.Type, FlatBuffers.Protocol.Position.CreatePosition(builder, x.Position.X, x.Position.Y), x.Moving, x.Direction)).ToArray());
      var portalsOffset = CreatePortalsVector(builder, portals.Select(x => FlatBuffers.Protocol.Portal.CreatePortal(builder, FlatBuffers.Protocol.Position.CreatePosition(builder, x.Position.X, x.Position.Y), builder.CreateString(x.Map))).ToArray());
      var offset = Enter.CreateEnter(builder, sequence, mapOffset, positionOffset, direction, objectsOffset, portalsOffset);
      builder.Finish(offset.Value);
      
      var bytes = builder.DataBuffer.ToSizedArray();
      using (var mstream = new MemoryStream())
      {
        using (var writer = new BinaryWriter(mstream))
        {
          writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(bytes.Length).Reverse().ToArray(), 0));
          writer.Write((byte)(nameof(Enter).Length));
          writer.Write(Encoding.Default.GetBytes(nameof(Enter)));
          writer.Write(bytes);
          writer.Flush();
          return mstream.ToArray();
        }
      }
    }
    
    public static byte[] Bytes(Model model) {
      return Bytes(model.Sequence, model.Map, model.Position, model.Direction, model.Objects, model.Portals);
    }
  };
}