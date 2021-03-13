using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum SkillType
    {
        [Description("")]
        Active,

        [Description("")]
        Buff,

        [Description("")]
        Passive,

    }
}
