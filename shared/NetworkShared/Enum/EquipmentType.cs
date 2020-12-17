using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum EquipmentType
    {
        [Description("무기")]
        Weapon,
        [Description("방패")]
        Shield,
        [Description("갑옷")]
        Armor,
        [Description("신발")]
        Shoes,
        [Description("모자")]
        Helmet,
    }
}