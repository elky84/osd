using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum StatType
    {
        [Description("체력")]
        HP,

        [Description("마력")]
        MP,

        [Description("방어력")]
        Defence,

        [Description("크리티컬")]
        Critical,

        [Description("크리티컬 데미지")]
        CriticalDamage,

        [Description("물리공격력")]
        PhysicalDamage,

        [Description("마법공격력")]
        MagicalDamage,

        [Description("공격 속도")]
        AttackSpeed,

    }
}
