using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum Target
    {
        [Description("아군")]
        Ally,

        [Description("적군")]
        Enemy,

        [Description("자신")]
        Self,

    }
}
