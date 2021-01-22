using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum WeaponType
    {
        [Description("검")]
        Sword,

        [Description("활")]
        Arrow,

        [Description("지팡이")]
        Staff,

    }
}
