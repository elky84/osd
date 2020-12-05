using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
                    _sectors.Add(new Sector(i, OnSectorStateChanged));
            }

            private void OnSectorStateChanged(Sector sector)
            {
                if (sector.Activated)
                    ActivatedSectors.Add(sector.Id, sector);
                else
                    ActivatedSectors.Remove(sector.Id);
            }

            private uint Index(Position position) => (uint)(position.Y / _sectorSize.Height) * Columns + (uint)(position.X / _sectorSize.Width);

            public IEnumerator<Sector> GetEnumerator() => _sectors.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _sectors.GetEnumerator();

            public Sector this[Position position] => this[Index(position)];

            public Sector this[uint index]
            {
                get
                {
                    if (index > _sectors.Count)
                        return null;

                    return _sectors[(int)index];
                }
            }

            public Sector Add(Object obj)
            {
                var sector = this[obj.Position];
                if (sector == null)
                    return null;

                sector.Add(obj);
                return sector;
            }

            public Sector Remove(Object obj)
            {
                var sector = this[obj.Position];
                if (sector == null)
                    return null;

                sector.Remove(obj);
                return sector;
            }

            public List<Sector> Nears(Position position)
            {
                try
                {
                    var pivot = this[position] ??
                        throw new System.Exception("invalid position");

                    var sectors = new List<Sector>();
                    sectors.Add(pivot);

                    var index = Index(position);

                    var isLeft = index % Columns == 0;
                    if (isLeft == false)
                    {
                        var sector = this[index - 1];
                        if (sector != null)
                            sectors.Add(sector);
                    }

                    var isRight = index % Columns == Columns - 1;
                    if (isRight)
                    {
                        var sector = this[index + 1];
                        if (sector != null)
                            sectors.Add(sector);
                    }

                    var isTop = index < Columns;
                    if (isTop)
                    {
                        var sector = this[index - Columns];
                        if (sector != null)
                            sectors.Add(sector);
                    }

                    var isBottom = index > Columns * (Rows - 1) - 1;
                    if (isBottom)
                    {
                        var sector = this[index + Columns];
                        if (sector != null)
                            sectors.Add(sector);
                    }

                    if (isLeft == false && isTop == false)
                    {
                        var sector = this[index - Columns - 1];
                        if (sector != null)
                            sectors.Add(sector);
                    }

                    if (isRight == false && isTop == false)
                    {
                        var sector = this[index - Columns + 1];
                        if (sector != null)
                            sectors.Add(sector);
                    }

                    if (isLeft == false && isBottom == false)
                    {
                        var sector = this[index + Columns - 1];
                        if (sector != null)
                            sectors.Add(sector);
                    }

                    if (isRight == false && isBottom == false)
                    {
                        var sector = this[index + Columns + 1];
                        if (sector != null)
                            sectors.Add(sector);
                    }

                    return sectors;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

            public List<T> Objects<T>(Position position) where T : Object
            {
                var sectors = Nears(position);
                return sectors.SelectMany(x => x.Objects).Select(x => x as T).Where(x => x != null).ToList();
            }
        }
    }
}
