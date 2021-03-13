using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum Target
    {
        [Description("")]
        Ally,

        [Description("")]
        Enemy,

        [Description("")]
        Self,

    }
}
