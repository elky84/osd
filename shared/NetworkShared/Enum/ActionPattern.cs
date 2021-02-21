using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum ActionPattern
    {
        [Description("왼쪽이동")]
        LeftMove,

        [Description("오른쪽이동")]
        RightMove,

    }
}
