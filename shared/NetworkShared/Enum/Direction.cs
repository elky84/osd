using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum Direction
    {
        [Description("")]
        Left,

        [Description("")]
        Right,

        [Description("")]
        Top,

        [Description("")]
        Bottom,

    }
}
