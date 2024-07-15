using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.EditorTools
{
    public partial class UnityShellEditorWindow : EditorWindow
    {
        #region Commands
        public const string CLEAR = "clear";
        public const string TERMINAL = "terminal";
        #endregion//============================================================[ Commands ]

        #region Public
        public object CustomCommand(object result)
        {
            if (input == TERMINAL)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    System.Diagnostics.Process.Start("CMD.exe");
                    result = "launching cmd";
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    Process.Start(@"/System/Applications/Utilities/Terminal.app");
                    result = "launching terminal";
                }
            }
            return result;
        }
        #endregion//============================================================[ Public ]
    }
}