using System.Collections.Generic;
using UnityEditor;

namespace XcelerateGames.Editor
{
    /// <summary>
    /// This class manages addition and removal of compiler flags, used via command line
    /// </summary>
    public static class ScriptingDefinedSymbols
    {
        /// <summary>
        /// All loaded symbols
        /// </summary>
        private static List<string> mSymbols = null;

        private const char Separtor = ';';

        /// <summary>
        /// Returns all symbols presently available
        /// </summary>
        public static List<string> Symbols => mSymbols;

        /// <summary>
        /// Reads the existing compiler flags and saved in local vaiable mSymbols
        /// </summary>
        /// <returns></returns>
        public static bool Read()
        {
            mSymbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUtilities.CurrentBuildTargetGroup).Split(Separtor));
            return true;
        }

        /// <summary>
        /// Sets all compiler flags contained in mSymbols,this should be called once all compiler flags are read
        /// </summary>
        /// <returns></returns>
        public static bool Commit()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUtilities.CurrentBuildTargetGroup, string.Join(Separtor.ToString(), mSymbols));
            return true;
        }

        /// <summary>
        /// Sets the compiler flag passed as argument if not already present
        /// </summary>
        /// <param name="compilerFlag"></param>
        public static void Add(string compilerFlag)
        {
            if (!Contains(compilerFlag))
            {
                mSymbols.Add(compilerFlag);
            }
        }

        /// <summary>
        /// Removes the compiler flag passed as argument
        /// </summary>
        /// <param name="compilerFlag"></param>
        public static void Remove(string compilerFlag)
        {
            mSymbols.Remove(compilerFlag);
        }

        /// <summary>
        /// Checks if th given symbol is present
        /// </summary>
        /// <param name="compilerFlag">compiler flag to check</param>
        /// <returns>true if found, else false</returns>
        public static bool Contains(string compilerFlag)
        {
            if (mSymbols == null)
                return false;
            return mSymbols.Contains(compilerFlag);
        }

        public static void AddOrRemove(string symbol, bool add)
        {
            if (add)
                Add(symbol);
            else
                Remove(symbol);
        }
    }
}
