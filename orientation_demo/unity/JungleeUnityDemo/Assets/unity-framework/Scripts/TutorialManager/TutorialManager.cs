using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;
using XcelerateGames.Locale;
using XcelerateGames.UI;
using XcelerateGames.UI.Animations;

namespace XcelerateGames.Tutorials
{
    public class TutorialManager : UiBase
    {
        #region Properties
        //This UI blocks clicks to all other items except the one specified as target
        public GameObject _MaskedUI = null;
        public UiItem _SquareClickableArea, _CircularClickableArea, _MaskedSquareClickableArea, _ArrowLeftRight, _ArrowUpDown, _Message, _SkipBtn;
        public Animator _HandAnimation = null;
        public Color _DefaultMaskColor = new Color(0f, 0f, 0f, 0.75f);
        //This will be used to highlight multiple UI elements
        public Canvas _TopCanvas = null;
        public string _PointAnimName = "Point";
        #endregion //Properties

        #region Signals
        [InjectSignal] private SigPlayTutorial mSigPlayTutorial = null;
        [InjectSignal] private SigSetTutorialStep mSigSetTutorialStep = null;
        [InjectSignal] private SigStopTutorial mSigStopTutorial = null;

        [InjectSignal] private SigTutorialStarted mSigTutorialStarted = null;
        [InjectSignal] private SigTutorialComplete mSigTutorialComplete = null;
        [InjectSignal] private SigOnTutorialStep mSigOnTutorialStep = null;
        [InjectSignal] private SigOnTutorialStepSetupDone mSigOnTutorialStepSetupDone = null;
        [InjectSignal] private SigPauseTutorial mSigPauseTutorial = null;
        [InjectSignal] private SigLoadAssetFromBundle mSigLoadAssetFromBundle = null;
        [InjectSignal] private SigGoToNextStep mSigGoToNextStep = null;
        [InjectSignal] private SigShowMask mSigShowMask = null;
        [InjectSignal] private SigDestroyTutorials mSigDestroyTutorials = null;
        //[InjectSignal] private SigInitVideoPlayer mSigInitVideoPlayer = null;
        //[InjectSignal] private SigVideoEnd mSigVideoEnd = null;
        [InjectSignal] private Timer.SigTimerRegister mSigTimerReg = null;
        //[InjectSignal] private SigShowDialogBox mSigShowDialogBox = null;
        [InjectSignal] private SigOnHandTap mSigOnHandTap = null;

        [InjectSignal] private TutorialModel mTutorialModel = null;
        #endregion //Signals

        [SerializeField] string _BundleName = "tutorials/";

        private TutorialData mCurrentTutorial = null;
        private UiItem mTargetItem = null;
        private int mCurrentStep = -1;
        private float mElapsedTime = 0f;
        private bool mPaused = false;
        private bool mSkipExitEvent = false;
        private List<string> mQueue = new List<string>();

        private TutorialStep pStep { get { return mCurrentTutorial._Steps[mCurrentStep]; } }
        private UiItem ClickableArea
        {
            get
            {
                if (mCurrentTutorial._Steps[mCurrentStep]._ClickAreaType == ClickAreaType.Circle)
                    return _CircularClickableArea;
                if (mCurrentTutorial._Steps[mCurrentStep]._ClickAreaType == ClickAreaType.Square)
                    return _SquareClickableArea;
                if (mCurrentTutorial._Steps[mCurrentStep]._ClickAreaType == ClickAreaType.MaskedSquare)
                    return _MaskedSquareClickableArea;
                return null;
            }
        }

        private Vector3 TargetPosition
        {
            get
            {
                if (pStep._UseCamaraSpace)
                {
                    return Camera.main.WorldToScreenPoint(mTargetItem.transform.position);
                }
                else
                {
                    return mTargetItem.transform.position;
                }
            }
        }

        #region Public Methods

        protected virtual void Initialize()
        {
            //Hide all UI items by default
            _MaskedUI.SetActive(false);
            _SquareClickableArea.SetActive(false);
            _CircularClickableArea.SetActive(false);
            _MaskedSquareClickableArea.SetActive(false);
            _ArrowLeftRight.SetActive(false);
            _ArrowUpDown.SetActive(false);
            _Message.SetActive(false);

            mSigSetTutorialStep.AddListener(SetStep);
            mSigPlayTutorial.AddListener(PlayTutorial);
            mSigPauseTutorial.AddListener(OnPauseTutorial);
            mSigStopTutorial.AddListener(StopTutorials);
            mSigGoToNextStep.AddListener(PlayStep);
            mSigShowMask.AddListener(ShowMask);
            mSigDestroyTutorials.AddListener(OnDestroyTutorials);
            if (XDebug.CanLog(XDebug.Mask.Tutorials))
                XDebug.Log("TutorialManager Initilaized", XDebug.Mask.Tutorials);
        }

        protected virtual void DeInitialize()
        {
            mSigSetTutorialStep.RemoveListener(SetStep);
            mSigPlayTutorial.RemoveListener(PlayTutorial);
            mSigPauseTutorial.RemoveListener(OnPauseTutorial);
            mSigStopTutorial.RemoveListener(StopTutorials);
            mSigGoToNextStep.RemoveListener(PlayStep);
            mSigShowMask.RemoveListener(ShowMask);
            mSigDestroyTutorials.RemoveListener(OnDestroyTutorials);
        }

        private void OnDestroyTutorials()
        {
            if (XDebug.CanLog(XDebug.Mask.Tutorials))
                XDebug.Log("TutorialManager being destroyed", XDebug.Mask.Tutorials);
            Destroy(gameObject);
        }

        private void ShowMask(bool show)
        {
            UiItem clickableArea = ClickableArea;
            if (clickableArea != null)
                clickableArea.SetActive(show);
        }

        public void OnAnimEvent(string animId)
        {
            mSigOnHandTap.Dispatch(pStep._StepName);
        }
        #endregion Public Methods

        #region UI callbacks
        public void OnSkip()
        {
            XDebug.Log("Stopping Tutorial " + mCurrentTutorial.name + " as user skipped.", XDebug.Mask.Tutorials);
            if (mCurrentTutorial._TutorialsToSkip != null)
            {
                Array.ForEach(mCurrentTutorial._TutorialsToSkip, mTutorialModel.MarkTutorialComplete);
            }
            StopTutorials();
        }
        #endregion UI callbacks

        #region Private Methods

        protected override void Start()
        {
            base.Start();
            Initialize();
            enabled = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DeInitialize();
        }

        private void LateUpdate()
        {
            if (mCurrentTutorial == null)
                return;
            if (pStep._FollowTarget && mTargetItem != null)
            {
                ClickableArea.transform.position = TargetPosition;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (mCurrentTutorial != null)
            {
                if (pStep._StepType == StepType.Highlight)
                {
                    mElapsedTime += Time.deltaTime;
                    if (mElapsedTime >= pStep._HoldTime)
                    {
                        PlayStep();
                    }
                }
            }
        }

        private void StopTutorials()
        {
            if (mCurrentTutorial == null)
                return;
            XDebug.Log("Stopping Tutorials : " + mCurrentTutorial.name, XDebug.Mask.Tutorials);
            ExitStep();
            OnTutorialEnd(true);
        }

        private void OnPauseTutorial(bool pause)
        {
            mPaused = pause;
            enabled = !mPaused;
            if (mPaused)
            {
                XDebug.Log("Pausing Tutorials : " + mCurrentTutorial.name, XDebug.Mask.Tutorials);
            }
            else
            {
                XDebug.Log("Resuming Tutorials : " + mCurrentTutorial.name, XDebug.Mask.Tutorials);
                //Skip the exit event when we resume, else pause state will be set again
                mSkipExitEvent = true;
                //We need to reduce one count, as we had increased the count before setting tutorial to pause state. If we dont do this, one step will be skipped
                mCurrentStep--;
                PlayStep();
            }
        }

        private void PlayTutorial(string tutorialAssetName, bool forcePlay)
        {
            if (mTutorialModel._Skip)
            {
                XDebug.Log("Tutorial skip cheat is enabled, returning.", XDebug.Mask.Tutorials);
                return;
            }
            if (mCurrentTutorial != null)
            {
                XDebug.Log("PlayTutorial called \"" + tutorialAssetName + "\" when a tutorial \"" + mCurrentTutorial.name + "\" is already playing.", XDebug.Mask.Tutorials);
                return;
            }
            XDebug.Log("PlayTutorial called : " + tutorialAssetName, XDebug.Mask.Tutorials);
            if (mTutorialModel.IsTutorialComplete(tutorialAssetName) && !forcePlay)
            {
                XDebug.Log("Tutorial \"" + tutorialAssetName + "\" already completed, returning", XDebug.Mask.Tutorials);
                return;
            }
            mCurrentTutorial = ResourceManager.LoadAssetFromBundle<TutorialData>(_BundleName + tutorialAssetName);
            if (mCurrentTutorial != null)
            {
                enabled = true;
                mTutorialModel._CurrentTutorialData = mCurrentTutorial;
                _SkipBtn.SetActive(mCurrentTutorial._CanBeSkipped);
                Show();
                mSigTutorialStarted.Dispatch(mCurrentTutorial.name, mCurrentTutorial);
                mCurrentStep = -1;
                XDebug.Log("Playing tutorial : " + mCurrentTutorial.name, XDebug.Mask.Tutorials);
                ApplyInteractiveStateData(mCurrentTutorial.OnStartInteractiveState);
                HandleGameObjects(mCurrentTutorial._GameObjectsOnEnter);
                PlayStep();
            }
            else
                XDebug.LogException("Could not find tutorial : " + tutorialAssetName);
        }

        //private void ChangeInteractiveState(string[] objectPaths, bool interactive)
        //{
        //    if (objectPaths == null)
        //        return;
        //    foreach (string objPath in objectPaths)
        //    {
        //        GameObject gameObj = GameObject.Find(objPath);
        //        if (gameObj != null)
        //        {
        //            UiItem item = gameObj.GetComponent<UiItem>();
        //            if (item != null)
        //            {
        //                if(XDebug.CanLog(XDebug.Mask.Tutorials))
        //                    Debug.Log($"{objPath} SetInteractive({interactive})");
        //                item.SetInteractive(interactive);
        //            }
        //            else
        //                XDebug.LogError($"ChangeInteractiveState<{interactive}>:Could not find UiItem component under {objPath}", XDebug.Mask.Tutorials);
        //        }
        //        else
        //            XDebug.LogError($"ChangeInteractiveState<{interactive}>:Could not find object at path : {objPath}");
        //    }
        //}

        private void ApplyInteractiveStateData(InteractiveState[] stateData)
        {
            if (stateData == null)
                return;
            foreach (InteractiveState state in stateData)
            {
                GameObject gameObj = GameObject.Find(state._Path);
                if (gameObj != null)
                {
                    UiItem item = gameObj.GetComponent<UiItem>();
                    if (item != null)
                    {
                        if (XDebug.CanLog(XDebug.Mask.Tutorials))
                            XDebug.Log($"{state} SetInteractive({state._IsInteractive})", XDebug.Mask.Tutorials);
                        item.SetInteractive(state._IsInteractive);
                    }
                    else
                        XDebug.LogError($"ApplyInteractiveStateData<{state._IsInteractive}>:Could not find UiItem component under {state._Path}", XDebug.Mask.Tutorials);
                }
                else
                {
                    string stepName = "NA";
                    if (mCurrentStep >= 0 && mCurrentStep < mCurrentTutorial._Steps.Length)
                        stepName = pStep._StepName;
                    XDebug.LogError($"ApplyInteractiveStateData<{state._IsInteractive}>:Could not find object at path : {state._Path} for {stepName}");
                }
            }
        }

        //For debugging purpose only
        private void SetStep(int stepIndex)
        {
            ExitStep();
            mCurrentStep = stepIndex;
            ExecuteStep(0f);
        }

        private void PlayStep()
        {
            if (PlayNextStep())
            {

            }
            else
            {
                OnTutorialEnd(false);
            }
        }

        private void OnTutorialEnd(bool skipped)
        {
            if (XDebug.CanLog(XDebug.Mask.Tutorials))
                XDebug.Log($"OnTutorialEnd {mCurrentTutorial.name}", XDebug.Mask.Tutorials);
            ApplyInteractiveStateData(mCurrentTutorial.OnEndInteractiveState);
            HandleGameObjects(mCurrentTutorial._GameObjectsOnExit);
            _HandAnimation.transform.SetParent(transform);

            if (!mCurrentTutorial._DoNotMarkTutorialComplete)
                mTutorialModel.MarkTutorialComplete(mCurrentTutorial.name);
            else
            {
                if (XDebug.CanLog(XDebug.Mask.Tutorials))
                    XDebug.Log("Not marking tutorial as complete based on settings, Must be marked as complete from code", XDebug.Mask.Tutorials);
            }
            mSigTutorialComplete.Dispatch(mCurrentTutorial.name, skipped, skipped ? pStep._StepName : null);
            Resources.UnloadAsset(mCurrentTutorial);
            mCurrentStep = -1;

            for (int i = 0; i < mCurrentTutorial._TutorialsToMarkComplete.Length; ++i)
            {
                mTutorialModel.MarkTutorialComplete(mCurrentTutorial._TutorialsToMarkComplete[i]);
            }
            mCurrentTutorial = null;
            mTutorialModel._CurrentTutorialData = null;

            Hide();
            if (mQueue.Count > 0)
            {
                string tutName = mQueue[0];
                mQueue.RemoveAt(0);
                if (XDebug.CanLog(XDebug.Mask.Tutorials))
                    XDebug.Log("Calling PlayTutorial \"" + tutName + "\" from queue. Remaining Queue length : " + mQueue.Count, XDebug.Mask.Tutorials);
                //mSigTimerUnReg.Dispatch(ExecuteStep);
                PlayTutorial(tutName, false);
                return;
            }
        }

        private bool PlayNextStep()
        {
            ExitStep();
            mCurrentStep++;
            if (mCurrentStep < mCurrentTutorial._Steps.Length)
            {
                if (pStep._Skip)
                {
                    if (XDebug.CanLog(XDebug.Mask.Tutorials))
                        XDebug.Log("Skipping step " + pStep._StepName + " as it is marked to skip", XDebug.Mask.Tutorials);
                    PlayStep();
                    return true;
                }
                if (!mPaused)
                {
                    mSigTimerReg.Dispatch(ExecuteStep, pStep._Delay, 0);
                }
                else
                {
                    if (XDebug.CanLog(XDebug.Mask.Tutorials))
                        XDebug.Log("Tutorial is paused.", XDebug.Mask.Tutorials);
                }

                return true;
            }
            if (XDebug.CanLog(XDebug.Mask.Tutorials))
                XDebug.Log("Tutorial steps complete for " + mCurrentTutorial.name, XDebug.Mask.Tutorials);
            return false;
        }

        private void ExecuteStep(float dt)
        {
            TutorialStep step = pStep;
            if (XDebug.CanLog(XDebug.Mask.Tutorials))
                XDebug.Log("Executing Tutorial Step " + step._StepName, XDebug.Mask.Tutorials);
            mSigOnTutorialStep.Dispatch(mCurrentTutorial.name, step._StepName, mCurrentStep, true);
            _SquareClickableArea.SetActive(step._ClickAreaType == ClickAreaType.Square);
            _CircularClickableArea.SetActive(step._ClickAreaType == ClickAreaType.Circle);
            _MaskedUI.SetActive(step._ClickAreaType == ClickAreaType.None || step._StepType == StepType.Mask);
            _MaskedSquareClickableArea.SetActive(step._ClickAreaType == ClickAreaType.MaskedSquare);
            _Message.SetActive(step._StepType != StepType.DialogBox && !string.IsNullOrEmpty(step._MessageKey));

            ApplyInteractiveStateData(pStep.OnEnterInteractiveState);
            HandleGameObjects(pStep._GameObjectsOnEnter);
            Highlight(true);

            if (step._StepType == StepType.Interactive || step._StepType == StepType.Highlight)
            {
                ShowTarget();
                if (mTargetItem == null)
                {
                    XDebug.LogError($"TargetItem {step._StepName} could not be found, Moving to next step", XDebug.Mask.Tutorials);
                    PlayStep();
                    return;
                }
            }
            else if (step._StepType == StepType.DialogBox)
            {
                OnDialogBoxCreated();
                //mSigShowDialogBox.Dispatch(new DialogBoxParams(step._MessageKey, step._HeaderKey, step._DialogBoxType, onCreate: OnDialogBoxCreated));
                ShowTarget();
            }
            else if (step._StepType == StepType.Video)
            {
                if (XDebug.CanLog(XDebug.Mask.Tutorials))
                    XDebug.Log("Playing video " + step._TargetPath, XDebug.Mask.Tutorials);
                //mSigVideoEnd.AddListener(OnVideoEnd);
                //mSigInitVideoPlayer.Dispatch(step._TargetPath, ScreenOrientation.AutoRotation, true, 0);
            }
            else if (step._StepType == StepType.LoadAsset)
            {
                mSigLoadAssetFromBundle.Dispatch(step._TargetPath, null, step._ShowLoadingGear, 0, OnAssetLoaded);
                ////We dispatched a signal to create UI, now move to next step, Ideally this should be the last step of tutorial
                //PlayStep();
            }
            else if (step._StepType == StepType.PlayTutorial)
            {
                mQueue.Add(step._TargetPath);
                if (XDebug.CanLog(XDebug.Mask.Tutorials))
                    XDebug.Log("Added \"" + step._TargetPath + "\" to queue. Queue length : " + mQueue.Count, XDebug.Mask.Tutorials);
                PlayStep();
            }
            else if (step._StepType == StepType.InstantiatePrefab)
            {
                if (step._Prefab != null)
                {
                    GameObject obj = GameObject.Instantiate(step._Prefab);
                    if (obj != null)
                    {
                        obj.name = step._Prefab.name;
                        if (!string.IsNullOrEmpty(step._ParentPath))
                        {
                            GameObject parent = GameObject.Find(step._ParentPath);
                            if (parent != null)
                            {
                                obj.transform.SetParent(parent.transform, step._Position, step._Scale, false);
                            }
                            else
                                XDebug.LogError("Could not find object at path : " + step._ParentPath);
                        }

                        Transform tran = obj.transform.Find(step._InteractiveAssetPath);
                        if (tran != null)
                        {
                            UiItem item = tran.GetComponent<UiItem>();
                            if (item != null)
                            {
                                item._OnClick.AddListener(OnClick);
                            }
                        }
                        else
                            XDebug.LogError("Could not find asset at path : " + step._InteractiveAssetPath);
                    }
                    else
                        XDebug.LogError("Could not instantiate prefab");
                }
                else
                    XDebug.LogError("Prefab is null for step : " + step._StepName);
            }
            else if (step._StepType == StepType.None)
            {
                XDebug.LogException("Invalid _StepType for step : " + step._StepName + ", Tutorial name : " + mCurrentTutorial.name);
            }

            if (mTargetItem != null && !string.IsNullOrEmpty(pStep._StartAnim))
                mTargetItem.PlayAnim(pStep._StartAnim);
            if (_Message.gameObject.activeInHierarchy)
            {
                _Message.SetText(Localization.Get(step._MessageKey));
                if (mTargetItem != null)
                {
                    _Message.transform.SetParent(ClickableArea != null ? ClickableArea.transform : mTargetItem.transform, false);
                    _Message.transform.localPosition = Vector3.zero;
                    _Message.transform.localScale = Vector3.one;
                }
                else
                    _Message.transform.localPosition = Vector3.zero;
                //Child 0 is Arrow pointer to left & child 1 is pointer to right
                //view._Message.transform.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, pStep._MessageRotation);
                _Message.transform.GetChild(0).SetActive(!pStep._FlipMessagePointer);
                _Message.transform.GetChild(1).SetActive(pStep._FlipMessagePointer);
                _Message.GetComponent<RectTransform>().anchoredPosition = step._MessagePositionOffset;
            }

            if (step._PauseTutorial)
            {
                mCurrentStep += 1;
                OnPauseTutorial(true);
            }
            mSigOnTutorialStepSetupDone.Dispatch(mCurrentTutorial.name, step._StepName, mCurrentStep, true);
        }

        private void HandleGameObjects(GameObjectData[] gameObjects)
        {
            if (gameObjects == null)
                return;
            foreach (GameObjectData data in gameObjects)
            {
                GameObject gameObj = GameObject.Find(data._ObjectPath);
                if (gameObj != null)
                {
                    if (XDebug.CanLog(XDebug.Mask.Tutorials)) XDebug.Log($"{data._ObjectPath} : SetActive({data._Enable})", XDebug.Mask.Tutorials);
                    gameObj.SetActive(data._Enable);
                }
                else
                    XDebug.LogError("Could not find GameObject  at path : " + data._ObjectPath);
            }
        }

        private void OnDialogBoxCreated()
        {
            TutorialStep step = pStep;
            UiDialogBox dialogBox = UiDialogBox.Show(step._MessageKey, step._HeaderKey, step._DialogBoxType);
            if (step._DialogBoxType == DialogBoxType.CLOSE)
                dialogBox.OnClose += OnDialogBoxClosed;
            else if (step._DialogBoxType == DialogBoxType.OK_CANCEL || step._DialogBoxType == DialogBoxType.YES_NO)
            {
                dialogBox.OnOk += OnDialogBoxOk;
                dialogBox.OnCancel += OnDialogBoxCancel;
            }
            else if (step._DialogBoxType == DialogBoxType.OKAY)
            {
                dialogBox.OnOkay += OnDialogBoxClosed;
            }
        }

        private void OnVideoEnd(string clipName, bool success)
        {
            if (XDebug.CanLog(XDebug.Mask.Tutorials))
                XDebug.Log("Video " + clipName + " completed, with success? " + success, XDebug.Mask.Tutorials);
            //mSigVideoEnd.RemoveListener(OnVideoEnd);
            PlayStep();
        }

        private void OnDialogBoxCancel()
        {
            PlayStep();
        }

        private void OnDialogBoxOk()
        {
            PlayStep();
        }

        private void OnDialogBoxClosed()
        {
            PlayStep();
        }

        private void OnClick(UiItem item)
        {
            if (XDebug.CanLog(XDebug.Mask.Tutorials))
                XDebug.Log("OnClick " + pStep._StepName + " : " + item.name, XDebug.Mask.Tutorials);
            PlayStep();
        }

        private void OnAssetLoaded(GameObject inUserData)
        {
            if (pStep._ShowLoadingGear)
                UiLoadingCursor.Show(false);

            //We loaded & instantiated the object, now move to next step, Ideally this should be the last step of tutorial
            PlayStep();
        }

        private void Highlight(bool highlight)
        {
            if (pStep._HighlightObjects == null || pStep._HighlightObjects.Length == 0)
                return;

            foreach (string objPath in pStep._HighlightObjects)
            {
                GameObject gameObj = GameObject.Find(objPath);
                if (gameObj != null)
                {
                    if (highlight)
                        Utilities.AddCanvas(gameObj, true, _TopCanvas.sortingOrder + 1);
                    else
                        Utilities.RemoveCanvas(gameObj);
                }
                else
                    XDebug.LogError($"Could not find object at path : \"{objPath}\" to highlight");
            }
        }

        private void ShowTarget()
        {
            if (string.IsNullOrEmpty(pStep._TargetPath))
            {
                if (XDebug.CanLog(XDebug.Mask.Tutorials)) XDebug.Log(string.Format("Target object path is empty for step {0}, Not doing anything : ", pStep._StepName), XDebug.Mask.Tutorials);
                return;
            }
            GameObject gameObj = GameObject.Find(pStep._TargetPath);
            if (gameObj != null)
            {
                mTargetItem = gameObj.GetComponent<UiItem>();
                if (mTargetItem != null)
                {
                    AdjustSize();
                    ApplyMaskColor();
                    ApplyClickThrough();
                    SetArrow();
                    SetHandAnim();
                    if (pStep._StepType == StepType.Interactive)
                        mTargetItem._OnClick.AddListener(OnClick);
                    if (pStep._ClickAreaType != ClickAreaType.None)
                    {
                        ClickableArea.transform.position = TargetPosition + pStep._ClickAreaOffset;

                        ClickableArea.GetComponent<RectTransform>().SetPivot(mTargetItem.transform.GetPivot());
                    }

                    if (pStep._Mask != null)
                        _MaskedSquareClickableArea.SetSprite(pStep._Mask);
                    if (XDebug.CanLog(XDebug.Mask.Tutorials))
                        XDebug.Log("Setting pos : " + gameObj.name + " : " + gameObj.GetComponent<RectTransform>().position, XDebug.Mask.Tutorials);
                    if (pStep._Audio != null)
                        pStep._Audio.Play();
                }
                else
                    XDebug.LogError("Could not find UiItem component under " + pStep._TargetPath, XDebug.Mask.Tutorials);
            }
            else
                XDebug.LogError("Could not find Target object at path : " + pStep._TargetPath);
        }

        private void ExitStep()
        {
            try
            {
                if (mCurrentStep < 0 || mCurrentStep >= mCurrentTutorial._Steps.Length)
                {
                    if (XDebug.CanLog(XDebug.Mask.Tutorials))
                        XDebug.Log($"Skipping Exit step as index is out of range for {mCurrentTutorial.name} Index : {mCurrentStep}, max : {mCurrentTutorial._Steps.Length}", XDebug.Mask.Tutorials);
                    return;
                }
                if (pStep._Skip)
                {
                    return;
                }
                if (mSkipExitEvent)
                {
                    mSkipExitEvent = false;
                    if (XDebug.CanLog(XDebug.Mask.Tutorials))
                        XDebug.Log("Skipping Exit step event as we are resuming tutorials" + pStep._StepName, XDebug.Mask.Tutorials);
                    return;
                }
                XDebug.Log("Exiting Tutorial Step " + pStep._StepName, XDebug.Mask.Tutorials);
                ApplyInteractiveStateData(pStep.OnExitInteractiveState);
                Highlight(false);
                HandleGameObjects(pStep._GameObjectsOnExit);

                if (mTargetItem != null)
                {
                    mTargetItem._OnClick.RemoveListener(OnClick);
                    if (pStep._ResetAnim && mTargetItem.pAnimator != null)
                    {
                        mTargetItem.pAnimator.StopAndResetToBeginning(pStep._StartAnim);
                    }

                    if (!string.IsNullOrEmpty(pStep._StopAnim))
                        mTargetItem.PlayAnim(pStep._StopAnim);
                }
                if (pStep._MarkTutorialComplete)
                    mTutorialModel.MarkTutorialComplete(mCurrentTutorial.name);
                mTargetItem = null;
                _ArrowLeftRight.SetActive(false);
                _ArrowUpDown.SetActive(false);
                _Message.transform.SetParent(_SquareClickableArea.transform.parent);
                _Message.SetActive(false);
                _HandAnimation.gameObject.SetActive(false);
                mElapsedTime = 0f;

                bool hasMask = NextStepHasMask();
                _SquareClickableArea.SetActive(hasMask);
                _CircularClickableArea.SetActive(hasMask);
                _MaskedSquareClickableArea.SetActive(hasMask);
                _Message.SetActive(false);
                _MaskedUI.SetActive(true);

                mSigOnTutorialStep.Dispatch(mCurrentTutorial.name, pStep._StepName, mCurrentStep, false);
            }
            catch (Exception e)
            {
                XDebug.LogException($"TutorialManager::ExitStep \n Message: {e.Message}, \nStack: {e.StackTrace}, \n Source: {e.Source}");
            }
        }

        private bool NextStepHasMask()
        {
            bool hasMask = false;
            int nextStepIndex = mCurrentStep + 1;
            if (nextStepIndex < mCurrentTutorial._Steps.Length)
            {
                TutorialStep nextStep = mCurrentTutorial._Steps[nextStepIndex];

                if (nextStep._ClickAreaType == pStep._ClickAreaType && nextStep._Delay == 0)
                    hasMask = true;
            }
            return hasMask;
        }

        private void AdjustSize()
        {
            if (pStep._ClickAreaType != ClickAreaType.None)
            {
                if (pStep._ClickAreaType == ClickAreaType.Square || pStep._ClickAreaType == ClickAreaType.MaskedSquare)
                    ClickableArea.Size = new Vector2(mTargetItem.Size.x * pStep._BoxScale.x, mTargetItem.Size.y * pStep._BoxScale.y);
                else if (pStep._ClickAreaType == ClickAreaType.Circle)
                {
                    ClickableArea.Size = Vector2.one * pStep._CircleSize;
                }
            }
        }

        private void ApplyMaskColor()
        {
            UiItem clickableArea = ClickableArea;
            if (clickableArea != null)
            {
                Image[] images = clickableArea.GetComponentsInChildren<Image>();
                foreach (Image image in images)
                    image.color = pStep._ChangeMaskColor ? pStep._MaskColor : _DefaultMaskColor;
            }
        }

        private void ApplyClickThrough()
        {
            UiItem clickableArea = ClickableArea;
            if (clickableArea != null)
            {
                Image[] images = clickableArea.GetComponentsInChildren<Image>();
                foreach (Image image in images)
                    image.raycastTarget = !pStep._AllowMaskClickThrough;
            }
        }

        private void SetArrow()
        {
            if (pStep._ArrowType == ArrowType.None)
                return;
            _ArrowLeftRight.SetActive(pStep._ArrowType == ArrowType.Left || pStep._ArrowType == ArrowType.Right);
            _ArrowUpDown.SetActive(pStep._ArrowType == ArrowType.Up || pStep._ArrowType == ArrowType.Down);
            Vector3 pos = TargetPosition;
            UiItem arrow = null;
            if (pStep._ArrowType == ArrowType.Left)
            {
                pos.x -= mTargetItem.Width;
                arrow = _ArrowLeftRight;
            }
            else if (pStep._ArrowType == ArrowType.Right)
            {
                pos.x += mTargetItem.Width;
                arrow = _ArrowLeftRight;
                arrow.transform.localEulerAngles = new Vector3(0, 0, 180);
            }
            else if (pStep._ArrowType == ArrowType.Up)
            {
                pos.y += mTargetItem.Height;
                arrow = _ArrowUpDown;
                arrow.transform.localEulerAngles = new Vector3(0, 0, 270);
            }
            else if (pStep._ArrowType == ArrowType.Down)
            {
                pos.y -= mTargetItem.Height;
                arrow = _ArrowUpDown;
                arrow.transform.localEulerAngles = new Vector3(0, 0, 90);
            }
            arrow.transform.position = pos + pStep._ArrowOffset;
            arrow.GetComponent<UiAnim>().ReInitDefaultPosition(_PointAnimName, true);
            arrow.PlayAnim(_PointAnimName);
        }

        private void SetHandAnim()
        {
            _HandAnimation.gameObject.SetActive(pStep._ShowHandAnim);
            if (!pStep._ShowHandAnim)
            {
                _HandAnimation.transform.SetParent(transform);
            }
            else
            {
                //if (pStep._ClickAreaType == ClickAreaType.MaskedSquare)
                // _HandAnimation.transform.position = TargetPosition;
                //else
                _HandAnimation.transform.SetParent(mTargetItem.transform, Vector3.zero, Vector3.one, false);
                _HandAnimation.GetComponent<RectTransform>().anchoredPosition = pStep._HandAnimOffset;
            }
        }

        #endregion
    }
}
