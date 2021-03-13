using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum ActionPattern
    {
        [Description("")]
        LeftMove,

        [Description("")]
        RightMove,

    }
}
