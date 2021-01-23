using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum Axis
    {
        [Description("X축")]
        X,

        [Description("Y축")]
        Y,

    }
}
