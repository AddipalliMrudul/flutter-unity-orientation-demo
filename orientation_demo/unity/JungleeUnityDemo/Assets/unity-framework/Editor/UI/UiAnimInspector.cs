using XcelerateGames.UI.Animations;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System;

namespace XcelerateGames.Editor.Inspectors
{
    [CustomEditor(typeof(UiAnim))]
    public class UiAnimInspector : UnityEditor.Editor
    {
        //Do not use this out side this class
        private enum AnimDataType
        {
            Position,
            Rotation,
            Scale,
            Alpha,
            Color
        }

        private UiAnim mTarget = null;

        //Controls whether we should draw controls on the selected object.
        //We do not draw controls for the AnimLib object
        private bool mDrawControls = true;

        //If set to true, we will show a drop down to pick an animation from the library
        private bool mLoadingAnim = false;

        //Will hold the index of the animation selected from the library
        private int mSelectedAnim = 0;

        //If set to true time line will be shown to see how animation will play
        private bool mShowTimeline = false;

        private bool? mTimelineUsed = false;

        //This time will be used to show how animation will look right in the editor, no need to play/run the game
        private float mTimeLineTimer = 0f;

        //Will hold the clone of copied animation
        private static UiAnimBase mBuffer = null;

        //To cache the transform properties
        private Vector3 mPosition = Vector3.zero;

        private Vector3 mRotation = Vector3.zero;
        private Vector3 mScale = Vector3.one;
        private Color mColor = Color.white;

        private bool? mCustomInspectorEnabled;

        #region Button Textures

        private static GUILayoutOption[] mButtonOptions, mButtonOptionsSmall = null;
        private static Texture mSaveIcon, mDuplicateIcon, mDeleteIcon, mLoadIcon, mCopyIcon, mPasteIcon, mPlayIcon, mPauseIcon;

        #endregion Button Textures

        private void OnEnable()
        {
            if (target == null)
                return;
            mCustomInspectorEnabled = EditorPrefs.GetBool("EnableUiAnimCustomEditor", true);

            if (!mCustomInspectorEnabled.Value)
                return;
            mTarget = (UiAnim)target;
            if (mTarget == null)
                return;
            CacheButtonIcons();
            //We are calling this to initialize & cache all necessary components
            mTarget.OnValidate();
            //Check if we are currently editing Animlib object
            mDrawControls = !(AssetDatabase.GetAssetPath(target).EndsWith(UiAnimLibrary.mAssetName, StringComparison.OrdinalIgnoreCase));
            UiAnimLibrary.Init();
            if (mDrawControls)
            {
                if (mTarget._Anims == null)
                    mTarget._Anims = new List<UiAnimBase>();

                for (int i = 0; i < mTarget._Anims.Count; ++i)
                {
                    //If the anim is of Reference type, then load anim data
                    if (mTarget._Anims[i]._Reference)
                        mTarget._Anims[i] = UiAnimLibrary.GetAnim(mTarget._Anims[i]._Name, mTarget._Anims[i]);
                    else
                    {
                        if (mTarget._Anims[i]._PositionData._Keys == null)
                            mTarget._Anims[i]._PositionData._Keys = new Vector3[1];
                    }
                }
            }
        }

        private void CacheTransformProperties()
        {
            mTimelineUsed = true;
            //Cache the values, we need these to reset
            mPosition = mTarget.transform.localPosition;
            mRotation = mTarget.transform.eulerAngles;
            mScale = mTarget.transform.localScale;
        }

        private void ResetTransformProperties()
        {
            if (mTarget != null)
            {
                if (mTimelineUsed.HasValue && mTimelineUsed.Value)
                {
                    //Reset to the cached values
                    mTarget.transform.localPosition = mPosition;
                    mTarget.transform.rotation = Quaternion.Euler(mRotation);
                    mTarget.transform.localScale = mScale;
                }
            }
        }

        private void CacheButtonIcons()
        {
            if (mButtonOptions == null) mButtonOptions = new GUILayoutOption[] { GUILayout.Width(32), GUILayout.Height(32) };
            if (mButtonOptionsSmall == null) mButtonOptionsSmall = new GUILayoutOption[] { GUILayout.Width(16), GUILayout.Height(16) };
            if (mSaveIcon == null) mSaveIcon = (Texture)EditorGUIUtility.Load("UiAnimIcons/Save.png");
            if (mDuplicateIcon == null) mDuplicateIcon = (Texture)EditorGUIUtility.Load("UiAnimIcons/Duplicate.png");
            if (mDeleteIcon == null) mDeleteIcon = (Texture)EditorGUIUtility.Load("UiAnimIcons/Delete.png");
            if (mLoadIcon == null) mLoadIcon = (Texture)EditorGUIUtility.Load("UiAnimIcons/Load.png");
            if (mCopyIcon == null) mCopyIcon = (Texture)EditorGUIUtility.Load("UiAnimIcons/Copy.png");
            if (mPasteIcon == null) mPasteIcon = (Texture)EditorGUIUtility.Load("UiAnimIcons/Paste.png");
            if (mPlayIcon == null) mPlayIcon = (Texture)EditorGUIUtility.Load("UiAnimIcons/Play.png");
            if (mPauseIcon == null) mPauseIcon = (Texture)EditorGUIUtility.Load("UiAnimIcons/Pause.png");
        }

        private void OnDisable()
        {
            SetObjectDirty();
            //If Custom inspector was disabled, dont do anything
            if (!mCustomInspectorEnabled.HasValue || !mCustomInspectorEnabled.Value)
                return;
            //If game is running, dont do anything, else if the anim is set as "Reference" then all anim data will be set to null.
            if (Application.isPlaying)
                return;
            ResetTransformProperties();
            if (mTarget != null && mTarget._Anims != null)
            {
                if (mDrawControls)
                {
                    for (int i = 0; i < mTarget._Anims.Count; ++i)
                    {
                        //If the anim is of Reference type, then reset all anim data
                        if (mTarget._Anims[i]._Reference)
                        {
                            mTarget._Anims[i]._PositionData = null;
                            mTarget._Anims[i]._RotationData = null;
                            mTarget._Anims[i]._ScaleData = null;
                            mTarget._Anims[i]._ColorData = null;
                        }
                    }
                }
            }
        }

        private void SetObjectDirty()
        {
            if (Application.isPlaying || mTarget == null)
                return;
            Transform transform = mTarget.transform;
            EditorUtility.SetDirty(mTarget);
            EditorSceneManager.MarkSceneDirty(transform.gameObject.scene);
        }

        public override void OnInspectorGUI()
        {
            if (!mCustomInspectorEnabled.Value)
            {
                base.OnInspectorGUI();
                return;
            }

            if (mDrawControls)
            {
                if (!string.IsNullOrEmpty(mTarget._DefaultAnim) && !IsAnimNameValid(mTarget._DefaultAnim))
                    EditorGUILayout.HelpBox("Anim not found in anim list", MessageType.Warning);
                mTarget._DefaultAnim = EditorGUITools.DrawTextFieldDelayed("Default Anim", mTarget._DefaultAnim);

                if (mTarget._PlayOnAwake)
                {
                    if (string.IsNullOrEmpty(mTarget._DefaultAnim))
                        EditorGUILayout.HelpBox("Play on Awake is enabled but default anim name is not specified", MessageType.Warning);
                    if (mTarget._Anims == null || mTarget._Anims.Count == 0)
                        EditorGUILayout.HelpBox("Play on Awake is enabled but no animations added", MessageType.Warning);
                }

                mTarget._PlayOnAwake = EditorGUITools.DrawToggle("Play On Awake", mTarget._PlayOnAwake);
            }

            int animCount = GetAnimCount();
            int count = EditorGUITools.DrawIntDelayed("Animations", animCount);
            if (count != animCount)
            {
                if (count < animCount)
                {
                    int diff = animCount - count;
                    mTarget._Anims.RemoveRange(animCount - diff, diff);
                }
                else
                {
                    while (GetAnimCount() < count)
                    {
                        UiAnimBase clone = null;
                        if (animCount > 0)
                            clone = mTarget._Anims[animCount - 1].Clone();
                        else
                            clone = UiAnimBase.Create();
                        if (mTarget._Anims == null)
                            mTarget._Anims = new List<UiAnimBase>();
                        mTarget._Anims.Add(clone);
                    }
                }
            }

            if (mTarget._Anims != null)
            {
                for (int i = 0; i < mTarget._Anims.Count; ++i)
                {
                    mTarget._Anims[i]._Draw = EditorGUITools.DrawDropDownHeader(mTarget._Anims[i]._Name, mTarget._Anims[i]._Draw);
                    if (mTarget._Anims[i]._Draw)
                    {
                        GUILayout.BeginHorizontal();
                        GUIContent content = new GUIContent(string.Empty, mDuplicateIcon, "Duplicate");
                        if (GUILayout.Button(content, mButtonOptions))
                        {
                            int index = i + 1;
                            mTarget._Anims.Insert(index, mTarget._Anims[i].Clone());
                            mTarget._Anims[index]._Name += "(Clone)";
                        }
                        content = new GUIContent(string.Empty, mDeleteIcon, "Delete");
                        if (GUILayout.Button(content, mButtonOptions))
                        {
                            if (EditorUtility.DisplayDialog("Warning", "Are you sure you want to delete \"" + mTarget._Anims[i]._Name + "\"", "Yes", "No"))
                                mTarget._Anims.RemoveAt(i);
                        }
                        content = new GUIContent(string.Empty, mSaveIcon, "Save");
                        if (GUILayout.Button(content, mButtonOptions))
                            UiAnimLibrary.Save(mTarget._Anims[i]);
                        if (mLoadingAnim)
                        {
                            List<string> anims = UiAnimLibrary.pAnimNames;
                            mSelectedAnim = EditorGUILayout.Popup(mSelectedAnim, anims.ToArray(), GUILayout.Width(128));
                            if (mSelectedAnim > 0)
                            {
                                mLoadingAnim = false;
                                //Keep the animation window open after loading.
                                mTarget._Anims[i]._Draw = true;
                                if (mSelectedAnim > 1)
                                    mTarget._Anims[i] = UiAnimLibrary.GetAnim(anims[mSelectedAnim], mTarget._Anims[i]);
                                mSelectedAnim = 0;
                            }
                        }
                        else
                        {
                            content = new GUIContent(string.Empty, mLoadIcon, "Load");
                            if (mDrawControls && GUILayout.Button(content, mButtonOptions))
                                mLoadingAnim = true;
                        }
                        content = new GUIContent(string.Empty, mCopyIcon, "Copy");
                        if (GUILayout.Button(content, mButtonOptions))
                            mBuffer = mTarget._Anims[i].Clone();
                        content = new GUIContent(string.Empty, mPasteIcon, "Paste");
                        if (mBuffer != null && GUILayout.Button(content, mButtonOptions))
                            mTarget._Anims[i] = mBuffer.Clone();
                        GUILayout.EndHorizontal();

                        if (mTarget._Anims.Count > 0)
                            DrawAnimInfo(mTarget._Anims[i], i);
                    }
                }
                if (mDrawControls)
                {
                    //Draw _OnAnimationDone event of Anim3D class
                    EditorGUITools.DrawHeader("Triggers Animation end event for any of the animation in the list");
                    EditorGUITools.DrawProperty("_OnAnimationDone", serializedObject);
                }
            }
        }

        private void DrawAnimInfo(UiAnimBase anim, int index)
        {
            #region Properties

            if (IsAnimNameDuplicate(anim._Name))
                EditorGUILayout.HelpBox("Multiple anims found with same name " + anim._Name, MessageType.Warning);
            anim._Name = EditorGUITools.DrawTextFieldDelayed("Name", anim._Name);
            anim._Mode = (Mode)EditorGUITools.DrawEnum("Mode", anim._Mode);
            if (anim._Mode == Mode.FORWARD_INFINITE || anim._Mode == Mode.REVERSE_INFINITE || anim._Mode == Mode.PINGPONG)
            {
                anim._LoopCount = EditorGUITools.DrawIntDelayed("Loop Count", anim._LoopCount);
            }
            EditorGUILayout.HelpBox(GetAnimCategoryHelpInfo(anim._Category), MessageType.Info);
            anim._Category = (UiAnimBase.Category)EditorGUITools.DrawEnum("Category", anim._Category);
            anim._IgnoreTimeScale = EditorGUITools.DrawToggle("Ignore Time Scale", anim._IgnoreTimeScale);
            //Reference option will not be shown for Animlib object.
            if (mDrawControls)
            {
                if (anim._Reference)
                    EditorGUILayout.HelpBox("Irrespective of setting here, \nthis anim will always be loaded from Library", MessageType.Info);
                anim._Reference = EditorGUITools.DrawToggle("Reference", anim._Reference);
            }
            string result = CheckForSnapToStartError();
            if (!string.IsNullOrEmpty(result))
                EditorGUILayout.HelpBox(result + " have \"Snap To Start\" enabled, this could result in unwanted behaviour", MessageType.Warning);
            anim._SnapToStart = EditorGUITools.DrawToggle("Snap To Start", anim._SnapToStart);
            //anim._StartZeroScale = EditorGUITools.DrawToggle("Start with Zero scale", anim._StartZeroScale);

            if (anim._StartInActive) EditorGUILayout.HelpBox("GameObject will be Set to Inactive on Init,\nIt will be enabled when Play is called", MessageType.Info);
            anim._StartInActive = EditorGUITools.DrawToggle("Start Inactive", anim._StartInActive);
            anim._DestroyOnComplete = EditorGUITools.DrawToggle("Destroy On Complete", anim._DestroyOnComplete);
            anim._TriggerEventOverride = EditorGUITools.DrawToggle("Trigger Event Override", anim._TriggerEventOverride);
            //Time multiplier must be a +ve value
            anim._TimeMultiplier = EditorGUITools.DrawFloatRange("Time Multiplier", anim._TimeMultiplier, -5f, 100f/*Just some higher num*/);
            anim._Delay = EditorGUITools.DrawFloat("Delay", anim._Delay);

            //if (anim._AudioClipRef != null)
            //{
            //    string bundleName;
            //    if (EditorUtilities.IsInAssetBundle(anim._AudioClipRef, out bundleName) && bundleName == "sounds")
            //        EditorGUILayout.HelpBox("This AudioClip is under sounds bundle.\n Ideally this AudioClip should not be under sounds bundle", MessageType.Warning);
            //}
            //anim._AudioClipRef = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", anim._AudioClipRef, typeof(AudioClip), false);
            //if (!string.IsNullOrEmpty(anim._AudioClip) && anim._AudioClipRef != null)
            //    EditorGUILayout.HelpBox("AudioClip is set, Value of this variabl will be ignored", MessageType.Warning);
            //anim._AudioClip = EditorGUITools.DrawTextField("Audio Clip Name", anim._AudioClip);
            //anim._AudioCategory = EditorGUITools.DrawTextField("Audio Category", anim._AudioCategory);
            //anim._Volume = EditorGUITools.DrawFloat("Volume", anim._Volume);
            //anim._AudioDelay = EditorGUITools.DrawFloat("Audio Delay", anim._AudioDelay);
            //Draw Audio vars
            if(!mTarget._Anims.IsNullOrEmpty())
            {
                //string propertyName = $"{nameof(mTarget._Anims)}.Array.data[{index}].{nameof(anim._AudioVars)}";
                //SerializedProperty serializedProperty = serializedObject.FindProperty(propertyName);
                SerializedProperty serializedProperty = serializedObject.FindProperty(nameof(mTarget._Anims)).GetArrayElementAtIndex(index).FindPropertyRelative(nameof(anim._AudioVars));
                if (serializedProperty != null)
                {
                    EditorGUILayout.PropertyField(serializedProperty);
                    serializedObject.ApplyModifiedProperties();
                }
                //else
                //    Debug.LogError($"Failed to find property : {propertyName}");
            }

            #endregion Properties

            #region Timeline

            bool newState = EditorGUITools.DrawToggle("Show Timeline", mShowTimeline);
            if (mShowTimeline != newState)
            {
                if (newState)
                {
                    CacheTransformProperties();
                    mColor = anim.GetColor(mTarget.transform);
                }
                else
                {
                    ResetTransformProperties();
                    anim.SetColor(mColor);
                }
            }

            mShowTimeline = newState;
            if (mShowTimeline)
            {
                GUILayout.BeginHorizontal();
                float prev = mTimeLineTimer;
                mTimeLineTimer = EditorGUITools.DrawSlider("Timeline", mTimeLineTimer, 0f, GetLength(anim));
                if (mTimeLineTimer != prev)
                    ApplyTimeline(anim);
                //if(mPlayingTimeLineAnim)
                //{
                //    if (GUILayout.Button(mPauseIcon, mButtonOptionsSmall))
                //        mPlayingTimeLineAnim = false;
                //    EditorUtility.SetDirty(target);
                //}
                //else
                //{
                //    if (GUILayout.Button(mPlayIcon, mButtonOptionsSmall))
                //    {
                //        mTimeLineTimer = 0f;
                //        mStartTime = EditorApplication.timeSinceStartup;
                //        mPlayingTimeLineAnim = true;
                //    }
                //}
                GUILayout.EndHorizontal();
            }

            #endregion Timeline

            anim._Debug = EditorGUITools.DrawToggle("Debug", anim._Debug);

            #region Draw Animation Data

            if (EditorGUITools.DrawDropDownHeader("Position"))
                anim._PositionData = (PositionAnimData)DrawAnimData(anim._PositionData, AnimDataType.Position);

            if (EditorGUITools.DrawDropDownHeader("Rotation"))
                anim._RotationData = DrawAnimData(anim._RotationData, AnimDataType.Rotation);

            if (EditorGUITools.DrawDropDownHeader("Scale"))
                anim._ScaleData = DrawAnimData(anim._ScaleData, AnimDataType.Scale);

            if (EditorGUITools.DrawDropDownHeader("Color"))
                anim._ColorData = DrawColorData(anim._ColorData);

            #endregion Draw Animation Data

            #region Draw Events

            if (mDrawControls)
            {
                SerializedProperty property = serializedObject.FindProperty("_Anims");
                serializedObject.Update();

                if (property != null)
                {
                    if (index < property.arraySize && index >= 0)
                    {
                        SerializedProperty p = property.GetArrayElementAtIndex(index);
                        EditorGUITools.DrawHeader("Triggers Animation start event for this anim");
                        EditorGUITools.DrawPropertyRelative("_OnAnimationStart", p, serializedObject);
                        EditorGUITools.DrawHeader("Triggers Animation end event for this anim");
                        EditorGUITools.DrawPropertyRelative("_OnAnimationDone", p, serializedObject);
                    }
                }
                else
                    UnityEngine.Debug.LogError("Could not find property : _Anims");
            }

            #endregion Draw Events
        }

        private AnimData DrawAnimData(AnimData animData, AnimDataType animDataType)
        {
            if (animData != null)
            {
                EditorGUITools.BeginContents();
                if (GUILayout.Button("Clear"))
                {
                    if (animData is PositionAnimData)
                        animData = new PositionAnimData();
                    else
                        animData = new AnimData();
                    animData.Init();
                }
                if (animData is PositionAnimData)
                {
                    PositionAnimData positionAnimData = animData as PositionAnimData;
                    positionAnimData._UsePositionAsOffset = EditorGUITools.DrawToggle("Use Position as Offset", positionAnimData._UsePositionAsOffset);
                    positionAnimData._StartFromCurrentPos = EditorGUITools.DrawToggle("Start From Current Position", positionAnimData._StartFromCurrentPos);
                    positionAnimData._UseWorldPosition = EditorGUITools.DrawToggle("Use World Position", positionAnimData._UseWorldPosition);
                }
                animData._Delay = EditorGUITools.DrawFloat("Delay", animData._Delay);
                if (animData != null && animData._Duration == 0 && (animData.NumKeys() > 0 || (animData._Curve != null && animData._Curve.length > 0)))
                    EditorGUILayout.HelpBox("Anim keys are set, duration cannot be 0", MessageType.Warning);
                animData._Duration = EditorGUITools.DrawFloatDelayed("Duration", animData._Duration);

                int numKeys = EditorGUITools.DrawIntDelayed("Keys", animData.NumKeys());
                if (numKeys != animData.NumKeys())
                {
                    //Resize the array
                    System.Array.Resize<Vector3>(ref animData._Keys, numKeys);
                }
                if (animData._Keys != null)
                {
                    if (animData._Keys.Length < 2 && animData._Duration > 0)
                        EditorGUILayout.HelpBox("You must set atleast 2 keys", MessageType.Warning);
                    for (int i = 0; i < animData._Keys.Length; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        //Apply values set in keys to transform
                        if (GUILayout.Button("<", GUILayout.Width(24)))
                        {
                            if (animDataType == AnimDataType.Position)
                            {
                                PositionAnimData positionAnimData = animData as PositionAnimData;
                                if (positionAnimData._UsePositionAsOffset)
                                {
                                    mTarget.transform.localPosition += animData._Keys[i];
                                }
                                else
                                    mTarget.transform.localPosition = animData._Keys[i];
                            }
                            else if (animDataType == AnimDataType.Rotation)
                                mTarget.transform.localRotation = Quaternion.Euler(animData._Keys[i]);
                            else if (animDataType == AnimDataType.Scale)
                                mTarget.transform.localScale = animData._Keys[i];
                        }
                        //Apply values set in transform to keys
                        if (GUILayout.Button(">", GUILayout.Width(24)))
                        {
                            if (animDataType == AnimDataType.Position)
                                animData._Keys[i] = mTarget.transform.localPosition;
                            else if (animDataType == AnimDataType.Rotation)
                                animData._Keys[i] = mTarget.transform.localRotation.eulerAngles;
                            else if (animDataType == AnimDataType.Scale)
                                animData._Keys[i] = mTarget.transform.localScale;
                        }
                        animData._Keys[i] = EditorGUITools.DrawVector3(string.Empty, animData._Keys[i]);
                        EditorGUILayout.EndHorizontal();
                    }

                    if (animData._Keys.Length == 0 && animData._Duration > 0)
                        EditorGUILayout.HelpBox("Animation duration is set, but no keys are added", MessageType.Warning);
                    if (animData._Curve == null)
                        animData._Curve = new AnimationCurve();
                    if (animData._Keys.Length > 0 && animData._Duration > 0 && animData._Curve.length <= 1)
                        EditorGUILayout.HelpBox("Animation duration is set, but curve is`nt specified", MessageType.Warning);
                    animData._Curve = EditorGUITools.DrawAnimationCurve("Curve", animData._Curve);
                }
                EditorGUITools.EndContents();
            }
            return animData;
        }

        private ColorData DrawColorData(ColorData animData)
        {
            if (animData != null)
            {
                EditorGUITools.BeginContents();
                if (GUILayout.Button("Clear"))
                {
                    animData = new ColorData();
                    animData.Init();
                }
                animData._Delay = EditorGUITools.DrawFloatDelayed("Delay", animData._Delay);
                if (animData != null && animData._Duration == 0 && (animData.NumKeys() > 0 || (animData._Curve != null && animData._Curve.length > 0)))
                    EditorGUILayout.HelpBox("Anim keys are set, duration cannot be 0", MessageType.Warning);
                animData._Duration = EditorGUITools.DrawFloatDelayed("Duration", animData._Duration);

                int numKeys = EditorGUITools.DrawIntDelayed("Keys", animData.NumKeys());
                if (numKeys != animData.NumKeys())
                {
                    //Resize the array
                    System.Array.Resize<Color>(ref animData._Keys, numKeys);
                }
                if (animData._Keys != null)
                {
                    if (animData._Keys.Length < 2 && animData._Duration > 0)
                        EditorGUILayout.HelpBox("You must set atleast 2 keys", MessageType.Warning);
                    for (int i = 0; i < animData._Keys.Length; ++i)
                        animData._Keys[i] = EditorGUITools.DrawColor((i + 1).ToString(), animData._Keys[i]);

                    if (animData._Keys.Length == 0 && animData._Duration > 0)
                        EditorGUILayout.HelpBox("Animation duration is set, but no keys are added", MessageType.Warning);
                    if (animData._Curve == null)
                        animData._Curve = new AnimationCurve();
                    if (animData._Keys.Length > 0 && animData._Duration > 0 && animData._Curve.length <= 1)
                        EditorGUILayout.HelpBox("Animation duration is set, but curve is`nt specified", MessageType.Warning);
                    animData._Curve = EditorGUITools.DrawAnimationCurve("Curve", animData._Curve);
                }
                EditorGUITools.EndContents();
            }
            return animData;
        }

        private void ApplyTimeline(UiAnimBase anim)
        {
            anim.SetTime(mTimeLineTimer);
            if (anim._PositionData != null && anim._PositionData.NumKeys() > 0 && anim._PositionData._Curve.length > 1)
                mTarget.transform.localPosition = anim.GetPos();
            if (anim._RotationData != null && anim._RotationData.NumKeys() > 0 && anim._RotationData._Curve.length > 1)
                mTarget.transform.rotation = anim.GetRotation();
            if (anim._ScaleData != null && anim._ScaleData.NumKeys() > 0 && anim._ScaleData._Curve.length > 1 && mTimeLineTimer > anim._ScaleData._Delay)
                mTarget.transform.localScale = anim.GetScale();
            if (anim._ColorData != null && anim._ColorData.NumKeys() > 0 && anim._ColorData._Curve.length > 1)
                anim.SetColor(mTimeLineTimer);
        }

        //Returns the list of animation names that have _SnapToStart enabled
        private string CheckForSnapToStartError()
        {
            string retVal = null;
            if (mTarget._Anims != null && mTarget._Anims.Count > 1)
            {
                List<UiAnimBase> anims = mTarget._Anims.FindAll(e => e._SnapToStart);
                if (anims.Count > 1)
                {
                    foreach (UiAnimBase anim in anims)
                        retVal += anim._Name + ", ";
                }
            }
            return retVal;
        }

        //Returns true if the given anim name is unique in the list else returns false
        private bool IsAnimNameDuplicate(string animName)
        {
            if (mTarget._Anims != null && mTarget._Anims.Count > 1)
                return mTarget._Anims.FindAll(e => Utilities.Equals(e._Name, animName)).Count > 1;
            return false;
        }

        //Returns true if the given anim name is avilable in the list else returns false
        private bool IsAnimNameValid(string animName)
        {
            if (mTarget._Anims != null)
                return mTarget._Anims.FindAll(e => Utilities.Equals(e._Name, animName)).Count > 0;
            return true;
        }

        private int GetAnimCount()
        {
            if (mTarget._Anims == null)
                return 0;
            return mTarget._Anims.Count;
        }

        private string GetAnimCategoryHelpInfo(UiAnimBase.Category category)
        {
            if (category == UiAnimBase.Category.In)
                return "Will be played automatically when Show is called";
            if (category == UiAnimBase.Category.Out)
                return "Will be played automatically when Hide is called";
            if (category == UiAnimBase.Category.General || category == UiAnimBase.Category.OnClick)
                return "Should be triggered manually.";
            return "Error! You should set a category.";
        }

        public virtual float GetLength(UiAnimBase anim)
        {
            float length = 0f;
            if (anim._PositionData != null)
                length += anim._PositionData._Duration + anim._PositionData._Delay;

            if (anim._RotationData != null)
                length += anim._RotationData._Duration + anim._RotationData._Delay;

            if (anim._ScaleData != null)
                length += anim._ScaleData._Duration + anim._ScaleData._Delay;

            if (anim._ColorData != null)
                length += anim._ColorData._Duration;
            return length;
        }

        #region Menu Commands

        /// <summary>
        /// We load the AnimLib only once per session, if someone updates & commits AnimLib prefab, we need to re-load it,
        /// Here we just mark the mInitialized flag to false, it will be re-Initialized once we click on any Anim3D component
        /// </summary>
        [MenuItem(Utilities.MenuName + "UI/Anim/Refresh Library")]
        private static void RefreshAnim3dLib()
        {
            UiAnimLibrary.mInitialized = false;
        }

        private const string mAnimInspectorCommand = Utilities.MenuName + "UI/Anim/Enable UiAnim Inspector";

        [MenuItem(mAnimInspectorCommand, false)]
        private static void EnableUiAnimCustomInspector()
        {
            EditorPrefs.SetBool("EnableUiAnimCustomEditor", !EditorPrefs.GetBool("EnableUiAnimCustomEditor", true));
        }

        [MenuItem(mAnimInspectorCommand, true, 1)]
        private static bool EnableUiAnimCustomInspectorValidate()
        {
            Menu.SetChecked(mAnimInspectorCommand, EditorPrefs.GetBool("EnableUiAnimCustomEditor", true));
            return true;
        }

        #endregion Menu Commands
    }
}