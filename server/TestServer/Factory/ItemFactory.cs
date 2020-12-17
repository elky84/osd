using MasterData;
using MasterData.Table;
using TestServer.Model;

namespace TestServer.Factory
{
    public static class ItemFactory
    {
        private static Model.Item Create(MasterData.Table.EquipmentOption equipmentOption)
        {
            var itemCase = MasterTable.From<TableItem>()[equipmentOption.Id];
            if (itemCase == null)
                return null;

            switch (equipmentOption.Type)
            {
                case NetworkShared.EquipmentType.Weapon:
                    return new Weapon(itemCase);

                case NetworkShared.EquipmentType.Shield:
                    return new Shield(itemCase);

                case NetworkShared.EquipmentType.Armor:
                    return new Armor(itemCase);

                case NetworkShared.EquipmentType.Shoes:
                    return new Shoes(itemCase);

                case NetworkShared.EquipmentType.Helmet:
                    return new Helmet(itemCase);

                default:
                    return null;
            }
        }

        public static Model.Item Create(string id)
        {
            var itemCase = MasterTable.From<TableItem>()[id];
            if (itemCase == null)
                return null;

            switch (itemCase.Type)
            {
                case NetworkShared.ItemType.Equipment:
                    return Create(MasterTable.From<TableEquipmentOption>()[id]);

                case NetworkShared.ItemType.Consume:
                    return new Consume(itemCase);

                default:
                    return new Model.Item(itemCase);
            }
        }
    }
}
