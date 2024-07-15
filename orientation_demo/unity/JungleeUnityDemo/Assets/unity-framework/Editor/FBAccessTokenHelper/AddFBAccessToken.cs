#if FB_ENABLED

using UnityEditor;
using UnityEngine;

/// <summary>
/// Author : Altaf
/// Date : March 23, 2017
/// Purpose : Helper script to apply FB access tokens.
/// Usage : In the text file FBAccessTokens.txt, put all user names & token separated by colon(:), ex : Altaf:ghjdfsghjhgshghjfghfdgh.
/// The token expire every 4 hours, Its better to use extended tokens, These token stay alive for a month.
/// </summary>

namespace XcelerateGames.Editor.FBHelper
{
    public class AddFBAccessToken : EditorWindow
    {
        DevFBUser mUser;

        bool mIsEditing = false;
        FBAccessTokenHelper mFBAccessTokenHelper;

        internal static void InitAdd(FBAccessTokenHelper accessTokenHelperInstance, DevFBUser user)
        {
            AddFBAccessToken instance = ScriptableObject.CreateInstance<AddFBAccessToken>();
            instance.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            instance.titleContent.text = "Add Token";
            instance.mUser = user;
            instance.mFBAccessTokenHelper = accessTokenHelperInstance;
            instance.mIsEditing = false;
            instance.ShowUtility();
        }

        internal static void InitEdit(FBAccessTokenHelper accessTokenHelperInstance, DevFBUser user)
        {
            AddFBAccessToken instance = ScriptableObject.CreateInstance<AddFBAccessToken>();
            instance.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            instance.titleContent.text = "Edit Token";
            instance.mFBAccessTokenHelper = accessTokenHelperInstance;
            instance.mUser = user;
            instance.mIsEditing = true;
            instance.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name :");
            mUser.Name = GUILayout.TextField(mUser.Name);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Token :");
            mUser.Token = GUILayout.TextField(mUser.Token);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (mIsEditing)
            {
                if (GUILayout.Button("Update"))
                    Add();
            }
            else
            {
                if (GUILayout.Button("Add"))
                    Add();
            }
            if (GUILayout.Button("Close"))
                Close();
            GUILayout.EndHorizontal();
        }

        void Add()
        {
            if (string.IsNullOrEmpty(mUser.Name))
                EditorUtility.DisplayDialog("Error", "Name cannot be null", "Ok");
            else if (string.IsNullOrEmpty(mUser.Token))
                EditorUtility.DisplayDialog("Error", "Token cannot be null", "Ok");
            else
            {
                if (mIsEditing)
                {
                    mFBAccessTokenHelper.UpdateUser(mUser);
                    Close();
                }
                else
                {
                    if (mFBAccessTokenHelper.NameExists(mUser.Name))
                        EditorUtility.DisplayDialog("Error", "Name already exists", "Ok");
                    else if (mFBAccessTokenHelper.NameExists(mUser.Token))
                        EditorUtility.DisplayDialog("Error", "Token already exists", "Ok");
                    else
                    {
                        if (mIsEditing)
                            mFBAccessTokenHelper.UpdateUser(mUser);
                        else
                            mFBAccessTokenHelper.Add(mUser);
                        Close();
                    }
                }
            }
        }
    }
}
#endif //FB_ENABLED
