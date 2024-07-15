#if FB_ENABLED
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/*
 * Author : Altaf
 * Date : March 23, 2017
 * Purpose : Helper script to apply FB access tokens.
 * Usage : In the text file FBAccessTokens.txt, put all user names & token separated by colon(:), ex : Altaf:ghjdfsghjhgshghjfghfdgh.
 * The token expire every 4 hours, Its better to use extended tokens, These token stay alive for a month.
*/

namespace XcelerateGames.Editor.FBHelper
{
    public class FBAccessTokenHelper : EditorWindow
    {
        private Vector2 mScroll = Vector2.zero;
        private List<DevFBUser> mUsers = new List<DevFBUser>();
        private const string mFileName = "FBAccessTokens.txt";

        [MenuItem(Utilities.MenuName + "Facebook/Clear Token")]
        static void ClearFBToekn()
        {
            string msg = "AccessToken not found";
            if (PlayerPrefs.HasKey(FBLogIn.FBDebugKey))
            {
                msg = "Deleted AccessToken";
                PlayerPrefs.DeleteKey(FBLogIn.FBDebugKey);
            }
            EditorUtility.DisplayDialog("Info", msg, "Ok", "");
        }

        [MenuItem(Utilities.MenuName + "Facebook/Access Tokens")]
        private static void DoFBAccessTokenHelper()
        {
            GetWindow<FBAccessTokenHelper>().titleContent.text = "FB Helper";
        }

        private void Awake()
        {
            Load();
        }

        private void Load()
        {
            mUsers.Clear();
            if(File.Exists(mFileName))
            {
                string data = File.ReadAllText(mFileName);
                mUsers = data.FromJson<List<DevFBUser>>();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();

            Color prev = GUI.color;
            GUI.color = Color.yellow;
            if (GUILayout.Button("Refresh"))
                Load();
            if (GUILayout.Button("Add"))
                AddFBAccessToken.InitAdd(this, new DevFBUser());

            GUI.color = prev;
            GUILayout.EndHorizontal();

            if (!EditorApplication.isPlaying)
                GUILayout.Label("Only available during Play mode.");

            EditorGUILayout.Space();

            mScroll = GUILayout.BeginScrollView(mScroll);
            GUILayout.BeginVertical();

            for (int i = 0; i < mUsers.Count; ++i)
            {
                DevFBUser user = mUsers[i];
                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
                if (GUILayout.Button(user.Name))
                {
                    FBLogIn.SetFBAccessToken(user.Token, true);
                    PlayerPrefs.SetString(FBLogIn.FBDebugKey, user.Token);
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Edit", GUILayout.Width(64)))
                    AddFBAccessToken.InitEdit(this, new DevFBUser(user));
                if (GUILayout.Button("X", GUILayout.Width(32)))
                {
                    mUsers.Remove(user);
                    i--;
                    Save();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        public void Add(DevFBUser user)
        {
            mUsers.Add(user);
            Save();
        }


        void Save()
        {
            File.WriteAllText(mFileName, mUsers.ToJson());
        }

        public bool NameExists(string inName)
        {
            return mUsers.Find(e => inName.Equals(e.Name, System.StringComparison.OrdinalIgnoreCase)) != null;
        }

        internal void UpdateUser(DevFBUser user)
        {
            int index = mUsers.FindIndex(e => e.Id == user.Id);
            mUsers.RemoveAll(e => e.Id == user.Id);
            mUsers.Insert(index, user);
            Save();
        }

        public bool TokenExists(string inToken)
        {
            return mUsers.Find(e => inToken.Equals(e.Token, System.StringComparison.OrdinalIgnoreCase)) != null;
        }
    }
}
#endif //FB_ENABLED