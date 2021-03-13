using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum ItemType
    {
        [Description("")]
        Equipment,

        [Description("")]
        Consume,

        [Description("")]
        Other,

    }
}
