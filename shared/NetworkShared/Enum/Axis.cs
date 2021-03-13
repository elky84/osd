using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum Axis
    {
        [Description("")]
        X,

        [Description("")]
        Y,

    }
}
