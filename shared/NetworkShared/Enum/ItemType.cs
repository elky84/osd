using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum ItemType
    {
        [Description("장비")]
        Equipment,
        [Description("소비")]
        Consume,
        [Description("기타")]
        Other,
    }
}