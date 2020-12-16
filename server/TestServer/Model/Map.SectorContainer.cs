using NetworkShared.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TestServer.Model
{
    public partial class Map
    {
        public class SectorContainer : IEnumerable<Sector>
        {
            private Map _owner;
            private Size _sectorSize;
            private List<Sector> _sectors = new List<Sector>();

            public uint Rows { get; private set; }
            public uint Columns { get; private set; }
            public uint Count { get; private set; }
            public Dictionary<uint, Sector> ActivatedSectors { get; private set; } = new Dictionary<uint, Sector>(); // 캐싱


            public SectorContainer(Map owner, Size sectorSize)
            {
                _owner = owner;
                _sectorSize = sectorSize;

                Rows = (uint)(_owner.Size.Height / sectorSize.Height);
                if (_owner.Size.Height % sectorSize.Height > 0)
                    Rows++;

                Columns = (uint)(_owner.Size.Width / sectorSize.Width);
                if (_owner.Size.Width % sectorSize.Width > 0)
                    Columns++;

                Count = Rows * Columns;
                for (uint i = 0; i < Count; i++)
                    _sectors.Add(new Sector(this, i, OnSectorStateChanged));
            }

            private void OnSectorStateChanged(Sector sector)
            {
                if (sector.Activated)
                    ActivatedSectors.Add(sector.Id, sector);
                else
                    ActivatedSectors.Remove(sector.Id);
            }

            private uint Index(Point position) => (uint)(position.Y / _sectorSize.Height) * Columns + (uint)(position.X / _sectorSize.Width);

            public IEnumerator<Sector> GetEnumerator() => _sectors.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _sectors.GetEnumerator();

            public Sector this[Point position] => this[Index(position)];

            public Sector this[uint index]
            {
                get
                {
                    if (index >= _sectors.Count)
                        return null;

                    return _sectors[(int)index];
                }
            }

            public Sector Add(Object obj)
            {
                if (obj.Sequence.HasValue == false)
                    return null;

                var sector = this[obj.Position];
                if (sector == null)
                    return null;

                if (obj.Sector == sector)
                    return sector;

                obj.Sector?.Remove(obj.Sequence.Value);
                obj.Sector = sector;
                sector.Add(obj.Sequence.Value, obj);
                return sector;
            }

            public Sector Remove(Object obj)
            {
                if (obj.Sequence.HasValue == false)
                    return null;

                var sector = this[obj.Position];
                if (sector == null)
                    return null;

                sector.Remove(obj.Sequence.Value);
                obj.Sector = null;
                return sector;
            }

            public IEnumerable<Sector> Nears(Point position)
            {
                try
                {
                    var pivot = this[position] ??
                        throw new System.Exception("invalid position");

                    return new List<Sector>
                    {
                        pivot,
                        pivot.Left,
                        pivot.Right,
                        pivot.Top,
                        pivot.Bottom,
                        pivot.LeftTop,
                        pivot.RightTop,
                        pivot.LeftBottom,
                        pivot.RightBottom
                    }.Where(x => x != null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

            public List<T> Objects<T>(Point position) where T : Object
            {
                var sectors = Nears(position);
                return sectors.SelectMany(x => x.Objects).Select(x => x as T).Where(x => x != null).ToList();
            }
        }
    }
}
