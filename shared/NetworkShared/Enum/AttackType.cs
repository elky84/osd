using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum AttackType
    {
        [Description("")]
        Preemptive,

        [Description("")]
        Reactive,

    }
}
