using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum Direction
    {
        [Description("왼쪽")]
        Left,
        [Description("오른쪽")]
        Right,
        [Description("위")]
        Top,
        [Description("아래")]
        Bottom,
    }
}