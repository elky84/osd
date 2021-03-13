using Newtonsoft.Json;

namespace NetworkShared
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum ObjectType
    {
        [Description("")]
        Character,

        [Description("")]
        Mob,

        [Description("")]
        NPC,

        [Description("")]
        Item,

    }
}
