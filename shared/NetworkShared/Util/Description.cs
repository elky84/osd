using System;

namespace NetworkShared
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DescriptionAttribute : System.Attribute
    {
        /// <summary>The description for the enum value.</summary>
        public string Description { get; set; }
        public string LocalizeKey { get; set; }

        /// <summary>Constructs a new DescriptionAttribute.</summary>
        public DescriptionAttribute() { }

        /// <summary>Constructs a new DescriptionAttribute.</summary>
        /// <param name="description">The initial value of the Description property.</param>
        public DescriptionAttribute(string description)
        {
            this.Description = description;
        }

        /// <summary>Constructs a new DescriptionAttribute.</summary>
        /// <param name="description">The initial value of the Description property.</param>
        public DescriptionAttribute(string description, string localizeKey)
        {
            this.Description = description;
            this.LocalizeKey = localizeKey;
        }

        /// <summary>Returns the Description property.</summary>
        /// <returns>The Description property.</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(this.LocalizeKey) ? this.Description : this.LocalizeKey;
        }
    }

    [AttributeUsage(AttributeTargets.Enum)]
    public class DescriptiveEnumEnforcementAttribute : System.Attribute
    {
        /// <summary>Defines the different types of enforcement for DescriptiveEnums.</summary>
        public enum EnforcementTypeEnum
        {
            /// <summary>Indicates that the enum must have a NameAttribute and a DescriptionAttribute.</summary>
            ThrowException,

            /// <summary>Indicates that the enum does not have a NameAttribute and a DescriptionAttribute, the value will be used instead.</summary>
            DefaultToValue
        }

        /// <summary>The enforcement type for this DescriptiveEnumEnforcementAttribute.</summary>
        public EnforcementTypeEnum EnforcementType { get; set; }

        /// <summary>Constructs a new DescriptiveEnumEnforcementAttribute.</summary>
        public DescriptiveEnumEnforcementAttribute()
        {
            this.EnforcementType = EnforcementTypeEnum.DefaultToValue;
        }

        /// <summary>Constructs a new DescriptiveEnumEnforcementAttribute.</summary>
        /// <param name="enforcementType">The initial value of the EnforcementType property.</param>
        public DescriptiveEnumEnforcementAttribute(EnforcementTypeEnum enforcementType)
        {
            this.EnforcementType = enforcementType;
        }
    }
}
