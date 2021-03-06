using MasterData;
using MasterData.Table;
using TestServer.Model;

namespace TestServer.Factory
{
    public static class ItemFactory
    {
        private static ulong _sequence = 0;
        private static ulong Sequence => _sequence++;

        private static Model.Item Create(MasterData.Table.EquipmentOption equipmentOption)
        {
            var itemCase = MasterTable.From<TableItem>()[equipmentOption.Id];
            if (itemCase == null)
                return null;

            switch (equipmentOption.Type)
            {
                case NetworkShared.EquipmentType.Weapon:
                    return new Weapon(Sequence, itemCase);

                case NetworkShared.EquipmentType.Shield:
                    return new Shield(Sequence, itemCase);

                case NetworkShared.EquipmentType.Armor:
                    return new Armor(Sequence, itemCase);

                case NetworkShared.EquipmentType.Shoes:
                    return new Shoes(Sequence, itemCase);

                case NetworkShared.EquipmentType.Helmet:
                    return new Helmet(Sequence, itemCase);

                default:
                    return null;
            }
        }

        public static Model.Item Create(string id, int count = 1)
        {
            var itemCase = MasterTable.From<TableItem>()[id];
            if (itemCase == null)
                return null;

            switch (itemCase.Type)
            {
                case NetworkShared.ItemType.Equipment:
                    return Create(MasterTable.From<TableEquipmentOption>()[id]);

                case NetworkShared.ItemType.Consume:
                    return new Model.Consume(Sequence, itemCase, count);

                default:
                    return new Model.Item(Sequence, itemCase, count);
            }
        }
    }
}
