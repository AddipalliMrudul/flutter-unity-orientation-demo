using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

/*  
 * This script comes handy when you want to rename children sequentially. 
 * This script is specially written to rename Spawn Point nodes.
*/

namespace XcelerateGames.Editor
{
    class RenameChildrenSequenciallyX : ScriptableWizard
    {
        public int _DigitsToPad = 1;
        public int _StartingNumber = 1;
        public bool _IncludeInactive = true;
        public string _Text = "";

        [MenuItem(Utilities.MenuName + "Level Designing/Rename Children Sequencially")]
        static void CreateWizard()
        {
            RenameChildrenSequenciallyX obj = ScriptableWizard.DisplayWizard("Rename Children Sequencially", typeof(RenameChildrenSequenciallyX), "Rename", "") as RenameChildrenSequenciallyX;

            if (EditorPrefs.HasKey("RCSX-DigitsToPad"))
                obj._DigitsToPad = EditorPrefs.GetInt("RCSX-DigitsToPad");

            if (EditorPrefs.HasKey("RCSX-StartingNumber"))
                obj._StartingNumber = EditorPrefs.GetInt("RCSX-StartingNumber");

            if (EditorPrefs.HasKey("RCSX-Text"))
                obj._Text = EditorPrefs.GetString("RCSX-Text");

            if (EditorPrefs.HasKey("RCSX-IncludeInactive"))
                obj._IncludeInactive = EditorPrefs.GetBool("RCSX-IncludeInactive");
}

        void OnDestroy()
        {
            EditorPrefs.SetInt("RCSX-DigitsToPad", _DigitsToPad);
            EditorPrefs.SetInt("RCSX-StartingNumber", _StartingNumber);
            EditorPrefs.SetString("RCSX-Text", _Text);
            EditorPrefs.SetBool("RCSX-IncludeInactive", _IncludeInactive);
        }

        void OnWizardUpdate()
        {
            helpString = "Sample : " + _Text + GetString(_StartingNumber);
        }

        private string GetString(int number)
        {
            string text = number.ToString();
            int digitsToPad = _DigitsToPad - text.Length;
            for (int i = 0; i < digitsToPad; ++i)
            {
                text = "0" + text;
            }
            return text;
        }

        void OnWizardCreate()
        {
            if (Selection.activeTransform == null)
                return;
            else
            {
                Transform parentTran = Selection.activeTransform;
                Component[] potentialWaypoints = parentTran.GetComponentsInChildren<Transform>(_IncludeInactive);

                //Now look for immediate child only, else all childrten in heirarchy will be renamed. which is not we want
                List<Transform> transforms = new List<Transform>();
                foreach (Component comp in potentialWaypoints)
                {
                    if (comp.transform.parent == parentTran)
                        transforms.Add(comp.transform);
                }

                //			GameUtilities.SortByName (ref potentialWaypoints);
                // do simple sorting by name
                int c = transforms.Count;
                int i = 0;
                int j = 0;
                Transform temp = null;
                for (i = 0; i < c; i++)
                {
                    for (j = i + 1; j < c; j++)
                    {
                        if (string.Compare(transforms[j].name, transforms[i].name) < 0)
                        {
                            // swap
                            temp = transforms[i];
                            transforms[i] = transforms[j];
                            transforms[j] = temp;
                        }
                    }
                }

                foreach (Transform node in transforms)
                {
                    if (node != parentTran)
                    {
                        node.name = _Text + GetString(_StartingNumber);
                        _StartingNumber++;
                    }
                }
            }
        }
    }
}
