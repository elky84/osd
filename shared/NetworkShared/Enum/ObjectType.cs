using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum ObjectType
    {
        [Description("캐릭터")]
        Character,
        [Description("몬스터")]
        Mob,
        [Description("엔피씨")]
        NPC,
        [Description("아이템")]
        Item,
    }
}