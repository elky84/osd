using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum EquipmentType
    {
        [Description("")]
        Weapon,

        [Description("")]
        Shield,

        [Description("")]
        Armor,

        [Description("")]
        Shoes,

        [Description("")]
        Helmet,

        [Description("")]
        Accessory,

    }
}
