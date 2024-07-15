using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// This class has extension methods to help with reflection
    /// </summary>
    public static class ReflectionUtilities
    {
        /// <summary>
        /// Get the fields of the given objects
        /// </summary>
        /// <param name="type">Type of the object</param>
        /// <param name="bindingFlags">flags to consider while gettining fields</param>
        /// <returns>List of all fields in the given objects</returns>
        public static List<FieldInfo> GetFields(Type type, BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            List<FieldInfo> fieldInfo = new List<FieldInfo>();
            //Get all private variables
            fieldInfo.AddRange(type.GetFields(bindingFlags));
            //Get all public variables
            fieldInfo.AddRange(type.GetFields());

            return fieldInfo;
        }

        /// <summary>
        /// Returns a string with values of all member variables. To be used for logging exceptions
        /// </summary>
        /// <param name="obj">Object whos value needs to be extracted</param>
        /// <param name="recursive">Should we recurilvely go though all foulds? @note recursive extraction will be slow</param>
        /// <returns>string with field name & value</returns>
        public static string GetInstanceValues(object obj, bool recursive = false)
        {
            if (recursive)
                return WriteFields(obj, 0);
            else
            {
                ArrayList fields = new ArrayList();

                FieldInfo[] fieldInfos = obj.GetType().GetFields(
                    // Gets all public, private, protected and static fields

                    BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance |
                    // This tells it to get the fields from all base types as well

                    BindingFlags.FlattenHierarchy);

                // Go through the list and get name & values
                string data = null;
                foreach (FieldInfo fi in fieldInfos)
                {
                    data += ($"{fi.Name}:{fi.GetValue(obj)} \n");
                    fields.Add(fi);
                }

                return data;
            }
        }

        /// <summary>
        /// Get all constants in the given objects
        /// </summary>
        /// <param name="type">Type of the object</param>
        /// <returns>list of all constant values</returns>
        public static FieldInfo[] GetConstants(Type type)
        {
            ArrayList constants = new ArrayList();

            FieldInfo[] fieldInfos = type.GetFields(
                // Gets all public and static fields

                BindingFlags.Public | BindingFlags.Static |
                // This tells it to get the fields from all base types as well

                BindingFlags.FlattenHierarchy);

            // Go through the list and only pick out the constants
            foreach (FieldInfo fi in fieldInfos)
                // IsLiteral determines if its value is written at 
                //   compile time and not changeable
                // IsInitOnly determine if the field can be set 
                //   in the body of the constructor
                // for C# a field which is readonly keyword would have both true 
                //   but a const field would have only IsLiteral equal to true
                if (fi.IsLiteral && !fi.IsInitOnly)
                    constants.Add(fi);

            // Return an array of FieldInfos
            return (FieldInfo[])constants.ToArray(typeof(FieldInfo));
        }

        /// <summary>
        /// Get the values of the fields
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="depth">Depth to be used for extraction, @note higher depth, takes long time</param>
        /// <returns>string of fields and values</returns>
        public static string WriteFields(object obj, int depth)
        {
            string data = string.Empty;

            //Checking for depth to avoid freezing the game for a long time.
            if (obj == null || depth > 5)
                return data;
            //string[] nameSpacesToIgnore = new string[] { "System", "UnityEngine.U2D" };
            foreach (FieldInfo fieldInfo in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                //if (nameSpacesToIgnore.Contains(fieldInfo.FieldType.Namespace))
                //    continue;
                //Debug.LogWarning($"{fieldInfo.Name} : " + (fieldInfo.FieldType.IsClass));
                if (fieldInfo.FieldType.IsClass)
                {
                    object objVal = fieldInfo.GetValue(obj);
                    if (objVal != null)
                    {
                        if(objVal is IDictionary || objVal is IList)
                            data += Printable(objVal, fieldInfo.Name);
                        else if(objVal is string)
                            data += $"\n{fieldInfo.Name}:{fieldInfo.GetValue(obj)}";
                        else
                            data += WriteFields(fieldInfo.GetValue(obj), depth + 1);
                    }
                    else
                        data += $"{fieldInfo.Name} is null\n";
                }
                else
                    data += $"\n{fieldInfo.Name}:{fieldInfo.GetValue(obj)}";
            }
            return data;
        }

        /// <summary>
        /// Helper function to print the values under IEnumerable
        /// </summary>
        /// <param name="obj">Object of type IEnumerable</param>
        /// <param name="name">name of the variable to print the values under</param>
        /// <returns>Formatted values</returns>
        public static string Printable(object obj, string name)
        {
            string data = string.Empty;
            if (obj == null)
                return data;

            if (obj is IDictionary)
            {
                var dict = obj as IDictionary;
                if (dict != null)
                {
                    data += $"\n{name}\n";
                    foreach (var key in dict.Keys)
                    {
                        data += ($"\t{key}:{dict[key]}\n");
                    }
                }
            }
            else if (obj is IList)
            {
                var lst = obj as IList;
                if (lst != null)
                {
                    data += $"\n{name}\n\t";
                    foreach (var v in lst)
                    {
                        data += v + ", ";
                    }
                }
            }
            //else if(obj.GetType().IsArray)
            //{
            //    Type type = obj.GetType().GetElementType();
            //    data += $"\n{name}\n\t";
            //    foreach (var a in obj.GetType().GetInterface("IEnumerable`1").GetGenericArguments())
            //    {
            //        data += a + ", ";
            //    }
            //}
            //else
            //    data = obj.ToString();

            return data;
        }
    }
}
