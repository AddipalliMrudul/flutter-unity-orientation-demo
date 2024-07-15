using System;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace XcelerateGames
{
    /// <summary>
    /// Extension class for enums
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Get the next element of enum
        /// </summary>
        /// <typeparam name="T">Type T of enum</typeparam>
        /// <param name="srcEnum">current enum value</param>
        /// <returns>Next element in enum after the current one</returns>
        /// @exception If the given Type T is not an enum, thows InvalidEnumArgumentException
        public static T Next<T>(this T srcEnum) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new InvalidEnumArgumentException($"Invalid argument type. Expected argument of type enum, got type \"{typeof(T)}\" instead");
            T[] values = (T[])Enum.GetValues(srcEnum.GetType());
            int nextIndex = Array.IndexOf(values, srcEnum) + 1;
            return nextIndex == values.Length ? values[0] : values[nextIndex];
        }

        /// <summary>
        /// Get the previous element of enum
        /// </summary>
        /// <typeparam name="T">Type T of enum</typeparam>
        /// <param name="srcEnum">current enum value</param>
        /// <returns>Previous element in enum before the current one</returns>
        /// @exception If the given Type T is not an enum, thows InvalidEnumArgumentException
        public static T Previous<T>(this T srcEnum) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new InvalidEnumArgumentException($"Invalid argument type. Expected argument of type enum, got type \"{typeof(T)}\" instead");
            T[] values = (T[])Enum.GetValues(srcEnum.GetType());
            int nextIndex = Array.IndexOf(values, srcEnum) - 1;
            return nextIndex == -1 ? values[values.Length - 1] : values[nextIndex];
        }

        /// <summary>
        /// Gt decription of the enum. Description can be added by adding attribute [Description] to enum element.
        /// </summary>
        /// <param name="en">enum element</param>
        /// <returns>Description of enum element</returns>
        public static string GetDescription(this Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        /// <summary>
        /// Returns a readable string representation of the given enum element. Ex if enum is *OnSuccess* returns **On Success**
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string Wodify(this Enum en)
        {
            return Regex.Replace(en.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
        }

        /// <summary>
        /// Get length of given enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcEnum"></param>
        /// <returns></returns>
        /// @exception If the given Type T is not an enum, thows InvalidEnumArgumentException
        public static int Length<T>(this T srcEnum) where T : struct
        {
            int count = -1;
            if (!typeof(T).IsEnum)
                throw new InvalidEnumArgumentException($"Invalid argument type. Expected argument of type enum, got type \"{typeof(T)}\" instead");
            count = Enum.GetValues(typeof(T)).Length;
            return count;
        }
    }
}
