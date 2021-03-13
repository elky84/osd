using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum StatType
    {
        [Description("")]
        HP,

        [Description("")]
        MP,

        [Description("")]
        Defence,

        [Description("")]
        Critical,

        [Description("")]
        CriticalDamage,

        [Description("")]
        PhysicalDamage,

        [Description("")]
        MagicalDamage,

        [Description("")]
        AttackSpeed,

    }
}
