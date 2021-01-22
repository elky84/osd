using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum AttackType
    {
        [Description("선공")]
        Preemptive,

        [Description("후공")]
        Reactive,

    }
}
