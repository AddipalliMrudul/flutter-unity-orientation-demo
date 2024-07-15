using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json.Converters;

namespace XcelerateGames
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Global settings to be used for serialization & deserialization
        /// </summary>
        private static JsonSerializerSettings jsonSerializerSettings = null;

        /// <summary>
        /// Static constructor to set global settings for serialization
        /// </summary>
        static ExtensionMethods()
        {
            jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Error = delegate (object sender, ErrorEventArgs args)
                {
                    string exceptionData = "Message : " + args.ErrorContext.Error.Message;
                    exceptionData += "\n Source : " + args.ErrorContext.Error.Source;
                //exceptionData += "\n InnerException : " + args.ErrorContext.Error.InnerException.ToString();
                exceptionData += "\n Path : " + args.ErrorContext.Path;
                    exceptionData += "\n Member : " + args.ErrorContext.Member;
                    exceptionData += "\n OriginalObject : " + args.ErrorContext.OriginalObject;
                    XDebug.LogException("JSON Deserialize Exception : " + exceptionData);
                    args.ErrorContext.Handled = false;
                }
            };
        }

        /// <summary>
        /// Serialize given object to its json representation
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <returns>json string of serialized object</returns>
        /// @see FromJson
        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
        }

        /// <summary>
        /// Deserialize json string to the given object type.
        /// </summary>
        /// <typeparam name="T">Type of object to which we need to deserialize</typeparam>
        /// <param name="obj">json string</param>
        /// <returns>Object of type T</returns>
        public static T FromJson<T>(this string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj, jsonSerializerSettings);
        }

        /// <summary>
        /// Get a particular node by name from a json string
        /// </summary>
        /// <param name="str">json string</param>
        /// <param name="nodeName">name of the node</param>
        /// <returns>json string for the given node</returns>
        public static string GetJsonNode(this string str, string nodeName, bool asJson = true)
        {
            JObject responseData = JObject.Parse(str);
            if (responseData != null)
            {
                JToken token = responseData[nodeName];
                if (token != null)
                    return asJson ? token.ToJson(): token.ToString();
                else
                    XDebug.LogException($"Could not find token {nodeName} in given string");
            }
            else
                XDebug.LogException("Could not parse to Json object");
            return null;
        }

        /// <summary>
        /// Activate/Deactivate GameObject. Its a helper function
        /// </summary>
        /// <param name="obj">instance of MonoBehaviour</param>
        /// <param name="value">activate/deactivate</param>
        /// <returns>returns the state of game object</returns>
        public static bool SetActive(this MonoBehaviour obj, bool value)
        {
            obj.gameObject.SetActive(value);
            return value;
        }

        /// <summary>
		/// Enable/Disable Animator component
        /// </summary>
        /// <param name="gameObject">GameObject that has Animator component</param>
        /// <param name="enable">Enable/Disable</param>
        /// <param name="recursive">Apply recursively or just the given object</param>
        /// <param name="includeInactive">Should we include inactive GameObjects?</param>
        public static void EnableAnimator(this GameObject gameObject, bool enable, bool recursive, bool includeInactive)
        {
            List<Animator> animators = new List<Animator>();
            if (recursive)
                animators.AddRange(gameObject.GetComponentsInChildren<Animator>(includeInactive));
            else
                animators.Add(gameObject.GetComponent<Animator>());
            foreach (Animator anim in animators)
                anim.enabled = enable;
        }
    }
}