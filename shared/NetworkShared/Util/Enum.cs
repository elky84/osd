using System;

namespace NetworkShared.Util
{
    public static partial class EnumUtil
    {
        /// <summary>Returns the description of the specified enum.</summary>
        /// <param name="value">The value of the enum for which to return the description.</param>
        /// <returns>A description of the enum, or the enum name if no description exists.</returns>
        public static string GetDescription(this Enum value)
        {
            return GetEnumDescription(value);
        }

        /// <summary>Returns the description of the specified enum.</summary>
        /// <param name="value">The value of the enum for which to return the description.</param>
        /// <returns>A description of the enum, or the enum name if no description exists.</returns>
        public static string GetDescription<T>(object value)
        {
            return GetEnumDescription(value);
        }

        /// <summary>Returns the description of the specified enum.</summary>
        /// <param name="value">The value of the enum for which to return the description.</param>
        /// <returns>A description of the enum, or the enum name if no description exists.</returns>
        public static string GetEnumDescription(object value)
        {
            if (value == null)
                return null;

            Type type = value.GetType();

            //Make sure the object is an enum.
            if (!type.IsEnum)
                throw new ApplicationException("Value parameter must be an enum.");

            var fieldInfo = type.GetField(value.ToString());
            object[] descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            //If no DescriptionAttribute exists for this enum value, check the DescriptiveEnumEnforcementAttribute and decide how to proceed.
            if (descriptionAttributes == null || descriptionAttributes.Length == 0)
            {
                object[] enforcementAttributes = fieldInfo.GetCustomAttributes(typeof(DescriptiveEnumEnforcementAttribute), false);

                //If a DescriptiveEnumEnforcementAttribute exists, either throw an exception or return the name of the enum instead.
                if (enforcementAttributes != null && enforcementAttributes.Length == 1)
                {
                    DescriptiveEnumEnforcementAttribute enforcementAttribute = (DescriptiveEnumEnforcementAttribute)enforcementAttributes[0];

                    if (enforcementAttribute.EnforcementType == DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)
                        throw new ApplicationException("No Description attributes exist in enforced enum of type '" + type.Name + "', value '" + value.ToString() + "'.");

                    return value.ToString();
                }
                else //Just return the name of the enum.
                    return value.ToString();
            }
            else if (descriptionAttributes.Length > 1)
                throw new ApplicationException("Too many Description attributes exist in enum of type '" + type.Name + "', value '" + value.ToString() + "'.");

            //Return the value of the DescriptionAttribute.
            return descriptionAttributes[0].ToString();
        }

        public static T ToEnumString<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static T GetValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
            // Or return default(T);
        }
    }
}
