/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Reflection;

namespace Kopernicus
{
    namespace UI
    {
        namespace Extensions
        {
            /// <summary>
            /// A class that adds some functions to MemberInfos
            /// </summary>
            public static class MemberInfoExtensions
            {
                /// <summary>
                /// Gets the value of a member info, by checking whether it is a Field or a Property
                /// </summary>
                public static Object GetValue(this MemberInfo member, Object reference)
                {
                    if (member.MemberType == MemberTypes.Field)
                        return (member as FieldInfo)?.GetValue(reference);
                    else if (member.MemberType == MemberTypes.Property && (member as PropertyInfo).CanRead)
                        return (member as PropertyInfo)?.GetValue(reference, null);
                    else
                        return null;
                }

                /// <summary>
                /// Gets the value of a member info, by checking whether it is a Field or a Property
                /// </summary>
                public static void SetValue<T>(this MemberInfo member, Object reference, T value)
                {
                    if (member.MemberType == MemberTypes.Field)
                        (member as FieldInfo)?.SetValue(reference, value);
                    else if (member.MemberType == MemberTypes.Property && (member as PropertyInfo).CanWrite)
                        (member as PropertyInfo)?.SetValue(reference, value, null);
                }

                /// <summary>
                /// Gets the value of a member info, by checking whether it is a Field or a Property
                /// </summary>
                public static void SetValue(this MemberInfo member, Object reference, Object value)
                {
                    if (member.MemberType == MemberTypes.Field)
                        (member as FieldInfo)?.SetValue(reference, value);
                    else if (member.MemberType == MemberTypes.Property && (member as PropertyInfo).CanWrite)
                        (member as PropertyInfo)?.SetValue(reference, value, null);
                }

                /// <summary>
                /// Gets the value of a member info, by checking whether it is a Field or a Property
                /// </summary>
                public static T GetValue<T>(this MemberInfo member, Object reference)
                {
                    if (member.MemberType == MemberTypes.Field)
                        return (T)(member as FieldInfo)?.GetValue(reference);
                    else if (member.MemberType == MemberTypes.Property && (member as PropertyInfo).CanRead)
                        return (T)(member as PropertyInfo)?.GetValue(reference, null);
                    else
                        return default(T);
                }

                /// <summary>
                /// Gets the type of the member info, by checking whether it is a Field or a Property
                /// </summary>
                public static Type GetMemberType(this MemberInfo member)
                {
                    if (member.MemberType == MemberTypes.Field)
                        return (member as FieldInfo)?.FieldType;
                    else if (member.MemberType == MemberTypes.Property)
                        return (member as PropertyInfo)?.PropertyType;
                    else
                        return null;
                }
            }
        }
    }
}