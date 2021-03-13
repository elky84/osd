using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum WeaponType
    {
        [Description("")]
        Sword,

        [Description("")]
        Bow,

        [Description("")]
        Staff,

    }
}
