using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum SkillType
    {
        [Description("액티브")]
        Active,

        [Description("버프")]
        Buff,

        [Description("패시브")]
        Passive,

    }
}
