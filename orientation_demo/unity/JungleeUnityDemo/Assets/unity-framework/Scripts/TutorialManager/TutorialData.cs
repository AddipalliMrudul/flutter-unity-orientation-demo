using System;
using XcelerateGames.UI;
using UnityEngine;
using XcelerateGames.Audio;

namespace XcelerateGames.Tutorials
{
    public enum ClickAreaType
    {
        None,
        Square,
        Circle,
        MaskedSquare,
    }

    public enum ArrowType
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    public enum StepType
    {
        None,
        Interactive,
        DialogBox,
        Video,
        LoadAsset,
        Highlight,      //Used just to show the UI item
        Mask,           //Used to mask the entire UI, So user cant interact
        PlayTutorial,   //Must be last step of the tutorial
        InstantiatePrefab,
    }

    [Serializable]
    public class GameObjectData
    {
        public string _ObjectPath = null;
        public bool _Enable = false;
    }

    [CreateAssetMenu(fileName = "TutorialData", menuName = Utilities.MenuName + "TutorialData")]
    public class TutorialData : ScriptableObject
    {
        public TutorialStep[] _Steps = null;
        //Path of all GameObjects, that need to be changed Interactive state while entering the tutorial
        public InteractiveState[] OnStartInteractiveState = null;
        //Path of all GameObjects, that need to be changed Interactive state while exiting the tutorial
        public InteractiveState[] OnEndInteractiveState = null;

        //GameObjects to be enabled/disabled on entering tutorial
        public GameObjectData[] _GameObjectsOnEnter = null;
        //GameObjects to be enabled/disabled on exitng tutorial
        public GameObjectData[] _GameObjectsOnExit = null;

        //If set to true, tutorial complete should be marked explicitly.
        public bool _DoNotMarkTutorialComplete = false;
        //If set to true, a Skip button will be shown on the lower right corner of text box
        public bool _CanBeSkipped = false;

        //If user selects to skip the tutorial, the current tutorial & all the tutorials mentioned in the list below will be skipped 
        public string[] _TutorialsToSkip = null;

        public string[] _TutorialsToMarkComplete = null;
    }

    [Serializable]
    public class TutorialStep
    {
        //Name of the tutorial step
        public string _StepName = null;

        public StepType _StepType = StepType.None;
        public ClickAreaType _ClickAreaType = ClickAreaType.Square;
        public Vector3 _ClickAreaOffset = Vector3.zero;
        public Color _MaskColor = new Color(0f, 0f, 0f, 0.4f);
        public bool _ChangeMaskColor = false;
        //By default mask blocks all clicks, this flag is used to control that
        public bool _AllowMaskClickThrough = false;
        public ArrowType _ArrowType = ArrowType.None;
        public Vector3 _ArrowOffset = Vector3.zero;
        public bool _ShowHandAnim = false;
        public Vector3 _HandAnimOffset = Vector3.zero;

        //If this flag is true, then tutorial will be marked as complete on this step.
        public bool _MarkTutorialComplete = false;
        //To be used by Canvas that has Camera Space set
        public bool _UseCamaraSpace = false;

        public Vector2 _BoxScale = Vector2.one;
        public float _CircleSize = 200f;
        public Vector3 _MessagePositionOffset = Vector3.zero;

        public Sprite _Mask = null;

        //in seconds, Applicable for only Highlight type
        public float _HoldTime = 0f;
        //In seconds, delay before the step is executed.
        public float _Delay = 0f;

        #region Anim Data
        //Name of anim to be played on step start 
        public string _StartAnim = null;
        //Name of anim to be played on step exit 
        public string _StopAnim = null;
        //Should we reset anim at end of step
        public bool _ResetAnim = false;
        #endregion
        //In case, if after adding the step, we decide to skip a step for testing/production, set this to true
        public bool _Skip = false;

        //Should this step pause the tutorial after executing this step?
        public bool _PauseTutorial = false;

        #region Interactive step data
        //Path of the object in hierarchy. Ex : PfUiResultScreen/Panel/Button1
        public string _TargetPath = null;
        public bool _ShowLoadingGear = false;
        public bool _FollowTarget = false;
        #endregion

        #region DialogBox step data
        public DialogBoxType _DialogBoxType = DialogBoxType.YES_NO;
        public string _HeaderKey = null;
        public string _MessageKey = null;
        public float _MessageRotation = 0f;
        public bool _FlipMessagePointer = false;
        #endregion

        #region LoadUI step related
        public GameObject _Prefab;
        //This path must have UiItem, we will wait for the click on this item before we move to the next step
        public string _InteractiveAssetPath;
        public string _ParentPath;
        public Vector3 _Position = Vector3.zero;
        public Vector3 _Scale = Vector3.one;
        #endregion

        //GameObjects to be enabled/disabled on entering step
        public GameObjectData[] _GameObjectsOnEnter = null;
        //GameObjects to be enabled/disabled on exitng step
        public GameObjectData[] _GameObjectsOnExit = null;
        public AudioVars _Audio = null;

        //These are the list of objects that wil be added with a Canvas component to make them highlight
        public string[] _HighlightObjects = null;

        //Path of all GameObjects, that need to be changed Interactive state while entering the step
        public InteractiveState[] OnEnterInteractiveState = null;
        //Path of all GameObjects, that need to be changed Interactive state while exiting the step
        public InteractiveState[] OnExitInteractiveState = null;
    }

    [Serializable]
    public class InteractiveState
    {
        public string _Path = null;
        public bool _IsInteractive = true;
    }
}
