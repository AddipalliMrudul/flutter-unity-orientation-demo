using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XcelerateGames.AssetLoading;
using XcelerateGames.Audio;
using XcelerateGames.UI.Animations;
using TMPro;
using XcelerateGames.Locale;
using System;
using XcelerateGames.Cryptography;
using XcelerateGames.Testing;

namespace XcelerateGames.UI
{
    [DisallowMultipleComponent]
    public class UiItem : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler, /*IPointerClickHandler,*/ IPointerDownHandler, IPointerUpHandler, IExposeData
    {
        [System.Serializable]
        public class CGClickEvent : UnityEvent<UiItem> { }
        [Tooltip("Array of other obj which needed to be effected on the basis of this UiItem state")]
        public ObjectStateData[] _ObjStatesArray;   /**<Array of other obj which needed to be effected on the basis of this UiItem state */
        public bool _Visible = true;    /**<Is object visible */
        public bool _UpdateFont = true; /**<Should update font when language changes? @see UpdateFont()*/
        public bool _UnloadTexture = true;  /**<Unload texture when the object is destroyed. @note Not unloading textures will increase memory consumption */
        public TextMeshProUGUI _TextItem = null; /**<TextMeshProUGUI component for text */
        public Image _Image = null; /**<Image Component for Sprites*/
        public RawImage _RawImage = null;   /**<RawImage Component for Textures */
        public bool _IsTextureEncrypted = false;    /**<Is the Texture encrypted */
        //Applicable to RawImage only
        public bool _ResizeToFitImage = false;      /**<Resize to fit parent, Applicable to RawImage only. @see SetTexture(Texture)*/

        public float _InvokeDelay = 0.5f;   /**<Delay to invoke click event. Default is 0.5 sec */
        private RectTransform rectTransform = null;
        public RectTransform RectTransform { get { if (rectTransform == null) rectTransform = GetComponent<RectTransform>(); return rectTransform; } private set { } }
        public Transform Transform { get; private set; }

        public AudioVars _ClickSound = null;    /**<Click sound */
        public UiAnim _UiAnim = null;   /**<XcelerateGames.UI.Animations.UiAnim Animation component to pay animations */
        [SerializeField] protected Button _Button = null;/**<Button component */

        public CGClickEvent _OnClick = null;    /**<To add listeners of click event */
        public object _UserData = null; /**<Custtom data */

        /**< Unique ID to be used by automation system */
        public string ID
        {
            get
            {
                return _ID;
            }
            private set
            {
                _ID = value;
            }
        }

        [SerializeField]
        private string _ID = null;  /**<Unique ID of this object. To be used in automation testing */

        public bool _IsEllipsis = false;    /**<Should we use ellipsis if text is larger than its container */
        public int _MaxCharsAllowed = short.MaxValue;   /**<No of characters after which we should use ellipsis */

        public string mRawUrl { get; private set; }

        protected MimeTypeId mMimeTypeId = MimeTypeId.None; /**<MimeType of this object*/

        protected UiBase mParentUI = null;  /**<Parent UiBase class */

        protected UiItemData mUserData = null;  /**<User data */
        protected bool mUsingCachedAsset = false;   /**<True is the Texture on RawImage is loaded from cache. Will be used to free memory when this object is destroyed */

        public UiAnim pAnimator { get { return _UiAnim; } }

        public object ExposeData => _UserData;  /**<Data exposed by this object. To be sued by automation */

        /// <summary>
        /// Set & Get test of this element
        /// </summary>
        /// @see SetText(string)
        /// @see GetText()
        public string text
        {
            get { return _TextItem.text; }
            set { _TextItem.text = value; }
        }

        /// <summary>
        /// Set & Get the MimeTypeId
        /// </summary>
        public MimeTypeId MimeTypeId
        {
            get { return mMimeTypeId; }
            set { mMimeTypeId = value; }
        }

        /// <summary>
        /// Set & Get the width
        /// </summary>
        public float Width
        {
            get { return RectTransform.rect.width; }
            set
            {
                Vector2 size = RectTransform.sizeDelta;
                size.x = value;
                RectTransform.sizeDelta = size;
            }
        }

        /// <summary>
        /// Set & Get the height
        /// </summary>
        public float Height
        {
            get { return RectTransform.rect.height; }
            set
            {
                Vector2 size = RectTransform.sizeDelta;
                size.y = value;
                RectTransform.sizeDelta = size;
            }
        }

        /// <summary>
        /// Set & Get the size
        /// </summary>
        public Vector2 Size
        {
            get { return RectTransform.sizeDelta; }
            set { RectTransform.sizeDelta = value; }
        }

        /// <summary>
        /// Set & Get the rect
        /// </summary>
        public Rect Rect
        {
            get { return RectTransform.rect; }
            set
            {
                RectTransform.sizeDelta = new Vector2(value.width, value.height);
                RectTransform.localPosition = new Vector2(value.x, value.y);
            }
        }

        /// <summary>
        /// Callback by Unity when object is created.
        /// Caching of various components is done in this function
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            //RectTransform = GetComponent<RectTransform>();
            Transform = transform;
            GetReferences();
            if (_UpdateFont)
                UpdateFont();

            if (_Button != null)
                _Button.onClick.AddListener(OnClicked);

            mParentUI = GetComponentInParent<UiBase>();

            SetVisibility(_Visible);
        }
#if AUTOMATION_ENABLED
        /// <summary>
        /// Update unique object ID. To be used for Automation testing
        /// </summary>
        /// <param name="ID">Unique object ID</param>
        public void UpdateID(string ID)
        {
            if (!string.IsNullOrEmpty(ID))
                _ID = ID;

            RegisterID();
        }

        /// <summary>
        /// Register self with unique object ID. To be used for Automation testing
        /// </summary>
        private void RegisterID()
        {
            if (!string.IsNullOrEmpty(ID))
            {
                IDMapper.RegisterID(ID, gameObject.GetObjectPath());
            }
        }
#endif
        /// <summary>
        /// Update font. Triggered by XcelerateGames.Locale.UILocalizeTMP whenever Language/Font changes
        /// </summary>
        /// @see _UpdateFont
        private void UpdateFont()
        {
            if (_TextItem != null)
            {
                TMP_FontAsset fontAsset = FontManager.GetFont(Localization.LanguageCode);
                if (fontAsset != null)
                    _TextItem.font = fontAsset;
            }
        }

        /// <summary>
        /// Called when this element is clicked
        /// </summary>
        public virtual void OnClicked()
        {
            if (!GuiManager.pBlockClickEvents)
            {
                if (_UiAnim != null)
                {
                    UiAnimBase anim = _UiAnim.GetAnim(UiAnimBase.Category.OnClick);
                    if (anim != null)
                        _UiAnim.Play(anim._Name);
                }
                GuiManager.pInstance.StartCoroutine(GuiManager.pInstance.WaitAndEnable(_InvokeDelay+0.1f));
                Invoke("SendClickEvent", _InvokeDelay);
                PlaySound(_ClickSound);
            }
        }

        /// <summary>
        /// Play sound 
        /// </summary>
        /// <param name="audioVars">XcelerateGames.Audio.AudioVars</param>
        protected virtual void PlaySound(AudioVars audioVars)
        {
            if (audioVars != null)
                audioVars.Play();
        }

        /// <summary>
        /// Send click event
        /// </summary>
        protected virtual void SendClickEvent()
        {
            if (_OnClick != null)
                _OnClick.Invoke(this);
        }

        /// <summary>
        /// Called by unity when finger/mouse enters this element
        /// </summary>
        /// <param name="eventData">Event Data</param>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            UiBase.pMouseOverItem = this;
        }

        /// <summary>
        /// Called by unity when finger/mouse exits this element
        /// </summary>
        /// <param name="eventData">Event Data</param>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            UiBase.pMouseOverItem = null;
        }

        /// <summary>
        /// Called by unity when finger/mouse down on this element
        /// </summary>
        /// <param name="eventData">Event Data</param>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
        }

        /// <summary>
        /// Called by unity when finger/mouse up on this element
        /// </summary>
        /// <param name="eventData">Event Data</param>
        public virtual void OnPointerUp(PointerEventData eventData)
        {

        }

        /// <summary>
        /// Called by Unity after Awake() is called
        /// </summary>
        protected virtual void Start()
        {
#if AUTOMATION_ENABLED
            RegisterID();
#endif
        }

        /// <summary>
        /// Update. Called everyframe  by unity
        /// </summary>
        protected virtual void Update()
        {
        }

        /// <summary>
        /// Set visibility true/false
        /// </summary>
        /// <param name="isVisible">show?</param>
        public virtual void SetVisibility(bool isVisible)
        {
            _Visible = isVisible;
            gameObject.SetActive(isVisible);
        }

        /// <summary>
        /// Is element ineractable?
        /// </summary>
        /// <param name="isInteractive">is interactive?</param>
        public virtual void SetInteractive(bool isInteractive)
        {
            if (_Button != null)
                _Button.interactable = isInteractive;
            TriggerStateType triggerStateType = isInteractive ? TriggerStateType.OnSetInteracive : TriggerStateType.OnSetNonInteractive;
            ApplyState(triggerStateType);
        }

        /// <summary>
        /// Is this element interactive
        /// </summary>
        /// <returns>true if interactive, else false</returns>
        public virtual bool IsInteractive()
        {
            if (_Button != null)
                return _Button.interactable;
            return false;
        }

        /// <summary>
        /// Is this element visible
        /// </summary>
        /// <returns></returns>
        public virtual bool IsVisible()
        {
            if (_Image != null)
                return _Image.enabled;
            if (_TextItem != null)
                return _TextItem.enabled;
            return false;
        }

        /// <summary>
        /// Set Text if nullable has value. If it has no value then gameObject will be disabled.
        /// If <b>showZero</b> is true, gameObject will be shown with zero value, else it will be disabled
        /// </summary>
        /// <param name="inNum">Nullable value</param>
        /// <param name="showZero">Should we enable gameObject even if value is zero?</param>
        public virtual void SetText(int? inNum, bool showZero = false)
        {
            bool show = inNum.HasValue;
            if (show && inNum.Value == 0 && !showZero)
                show = false;

            gameObject.SetActive(show);
            if (show)
                SetText(inNum.ToString());
        }

        /// <summary>
        /// Set text. If Ellipsis is enabled, handles it internally
        /// </summary>
        /// <param name="inString">Text to set</param>
        public virtual void SetText(string inString)
        {
            if (_TextItem != null)
                _TextItem.text = _IsEllipsis ? inString.Ellipsis(_MaxCharsAllowed) : inString;
            else
                XDebug.LogWarning("_TextItem is null on : " + gameObject.GetObjectPath());
        }

        /// <summary>
        /// Set Text only if text is not null or empty
        /// </summary>
        /// <param name="inString">Text to set</param>
        public virtual void SetTextIfNotNull(string inString)
        {
            if (string.IsNullOrEmpty(inString))
            {
                if (_TextItem != null)
                    _TextItem.SetActive(false);
            }
            else
            {
                if (_TextItem != null)
                {
                    _TextItem.SetActive(true);
                    _TextItem.text = inString;
                }
            }
        }

        /// <summary>
        /// Set text, if text is null or empty, game object is disabled. This will disable object that has UiItem component attached
        /// </summary>
        /// <param name="inString">Text to set</param>
        /// <param name="disableIfNull">Disable gameObject if set is null or empty</param>
        public virtual void SetText(string inString, bool disableIfNull)
        {
            if (string.IsNullOrEmpty(inString))
            {
                if (disableIfNull)
                    gameObject.SetActive(false);
            }
            else
            {
                if (_TextItem != null)
                {
                    gameObject.SetActive(true);
                    _TextItem.text = inString;
                }
            }
        }

        /// <summary>
        /// Set text by passing Localization key.
        /// </summary>
        /// <param name="inLocaleKey">key from Localization doc</param>
        /// @see TextFormat(string, params object[])
        public virtual void SetLocaleText(string inLocaleKey)
        {
            SetText(Localization.Get(inLocaleKey));
        }

        /// <summary>
        /// Set text by passing a localised key with format options.
        /// </summary>
        /// <param name="key">Key in localisation doc</param>
        /// <param name="args">parameters to use in formatting text</param>
        /// @see SetLocaleText(string)
        public virtual void TextFormat(string key, params object[] args)
        {
            SetText(Localization.Format(key, args));
        }

        /// <summary>
        /// Get text on the text component
        /// </summary>
        /// <returns>String</returns>
        public virtual string GetText()
        {
            if (_TextItem != null)
                return _TextItem.text;
            else
            {
                UnityEngine.Debug.LogError("_TextItem is null on : " + gameObject.GetObjectPath());
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns true if text component exists & has text, else false
        /// </summary>
        public virtual bool HasText
        {
            get
            {
                if (_TextItem != null)
                    return !string.IsNullOrEmpty(_TextItem.text);
                return false;
            }
        }

        /// <summary>
        /// Update the sprite, If <b>disableImageIfNull</b> is true and sprite is null, then Image component will be disabled
        /// </summary>
        /// <param name="inSprite">Sprite to set</param>
        /// <param name="disableImageIfNull">If true and sprite is null, then Image component will be disabled</param>
        /// @see SetSprite(string, Action<bool>)
        public virtual void SetSprite(Sprite inSprite, bool disableImageIfNull = true)
        {
            if (_Image != null)
            {
                _Image.sprite = inSprite;
                if (disableImageIfNull)
                    _Image.enabled = (inSprite != null);
            }
        }

        /// <summary>
        /// Set Sprite by loading an asset from AssetBundle. 
        /// </summary>
        /// <param name="url">name of AssetBundle & asset inside it. Ex: avatar/player1.png</param>
        /// <param name="callback">Callback to be triggered when sprite is loaded & set</param>
        /// @see SetSprite(string, bool)
        public virtual void SetSprite(string url, Action<bool> callback = null)
        {
            if (_Image != null)
            {
                mRawUrl = url;
                ResourceManager.Load(url, OnSpriteLoaded, ResourceManager.ResourceType.Object, callback);
            }
            else
                XDebug.LogWarning("Trying to attach a sprite to a null object." + name);
        }

        /// <summary>
        /// Callback from ResourceManager when sprite is loaded via @see SetSprite(string, Action<bool>)
        /// </summary>
        /// <param name="inEvent">XcelerateGames.AssetLoading.ResourceEvent</param>
        /// <param name="inURL">Loaded asset path</param>
        /// <param name="inObject">Loaded object</param>
        /// <param name="inUserData">User data passed while loading</param>
        private void OnSpriteLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;
            bool loaded = false;
            mRawUrl = inURL;
            if (inEvent == ResourceEvent.COMPLETE)
            {
                Texture2D tex = inObject as Texture2D;
                if (tex != null)
                {
                    loaded = true;
                    SetSprite(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f));
                }
            }

            if (inUserData != null)
            {
                Action<bool> callback = inUserData as Action<bool>;
                callback?.Invoke(loaded);
            }
        }


        /// <summary>
        /// Returns the reference to the current sprite
        /// </summary>
        /// <returns>Sprite</returns>
        /// @see SetSprite(string, Action<bool>)
        /// @see SetSprite(Sprite, bool disableImageIfNull)
        public virtual Sprite GetSprite()
        {
            if (_Image != null)
                return _Image.sprite;
            return null;
        }

        /// <summary>
        /// Set texture by loading from asset bundle
        /// </summary>
        /// <param name="url">Name of asset bundle followed by asset path</param>
        /// <param name="callback">Callback to be triggered when asset is loaded</param>
        /// <param name="useCache">load the texture rom cache if exists. url will be used as cache key</param>
        public virtual void SetTexture(string url, Action<bool, string, UiItem> callback, bool useCache = false)
        {
            if (url.IsNullOrEmpty())
                return;
            url = url.Trim();
            if (_RawImage != null)
            {
                mRawUrl = url;
                string hash = FileUtilities.GetMD5OfText(url);

                if (useCache && FileUtilities.FileExists(hash))
                {
                    MimeTypeId mimeTypeId = Utilities.GetMimeTypeId(url);
                    if (mimeTypeId == MimeTypeId.Images)
                    {
                        mUsingCachedAsset = true;
                        if (_IsTextureEncrypted)
                        {
                            Texture2D tex = Crypto.Decrypt(FileUtilities.ReadBytes(hash)).CreateTexture2D(false);
                            SetTexture(tex);
                        }
                        else
                        {

                            Texture2D tex = new Texture2D(2, 2);
                            if (tex.LoadImage(FileUtilities.ReadBytes(hash), true))
                                SetTexture(tex);
                            else
                                XDebug.LogError($"Failed to load cached file of {url}");
                        }
                    }
                }
                else
                {
                    mUsingCachedAsset = false;
                    var tuple = Tuple.Create(callback, useCache);
                    if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        ResourceManager.LoadURL(url, OnImageLoaded, ResourceManager.ResourceType.Texture, tuple);
                    else
                        ResourceManager.Load(url, OnImageLoaded, ResourceManager.ResourceType.Object, tuple);
                }
            }
            else
                XDebug.LogWarning("Trying to attach a texture to a null object." + name);
        }

        /// <summary>
        /// Set Texture to RawImage component
        /// </summary>
        /// <param name="texture"></param>
        /// @see _ResizeToFitImage
        public virtual void SetTexture(Texture texture)
        {
            _RawImage.texture = texture;
            if (_ResizeToFitImage)
            {
                _RawImage.ScaleToDimension(texture.Size(), true);
            }
        }

        /// <summary>
        /// Callback from XcelerateGames.AssetLoading.ResourceManager when image is loaded from given path
        /// </summary>
        /// <param name="inEvent">XcelerateGames.AssetLoading.ResourceEvent</param>
        /// <param name="inURL">Loaded asset path</param>
        /// <param name="inObject">Loaded object</param>
        /// <param name="inUserData">User data passed while loading</param>
        /// @see SetTexture(string, Action<bool, string, UiItem>, bool useCache)
        private void OnImageLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;
            bool loaded = false;
            mRawUrl = inURL;
            Tuple<Action<bool, string, UiItem>, bool> varsd = inUserData as Tuple<Action<bool, string, UiItem>, bool>;
            if (inEvent == ResourceEvent.COMPLETE)
            {
                Texture2D tex = null;
                if (mMimeTypeId == MimeTypeId.Images && _IsTextureEncrypted)
                {
                    byte[] data = ResourceManager.GetBytes(inURL, true);
                    tex = Crypto.Decrypt(data).CreateTexture2D(false);
                }
                else
                    tex = inObject as Texture2D;

                if (tex != null)
                {
                    loaded = true;
                    SetTexture(tex);

                    if (varsd.Item2 && mMimeTypeId == MimeTypeId.Images)
                    {
                        string hash = FileUtilities.GetMD5OfText(inURL);
                        FileUtilities.WriteToFile(hash, ResourceManager.GetBytes(inURL));
                    }
                }
                else
                    XDebug.LogException($"Failed to apply texture: {inURL}");
            }

            if (varsd.Item1 != null)
            {
                varsd.Item1?.Invoke(loaded, inURL, this);
            }
        }

        /// <summary>
        /// Simulate perfom click
        /// </summary>
        public void PerformClick()
        {
            SendClickEvent();
        }

        /// <summary>
        /// Play animation of XcelerateGames.UI.Animations.UiAnim component
        /// </summary>
        /// <param name="animName">Name of animation to play</param>
        /// <param name="playIfNoAnimIsPlaying">Play this animation only if no animation playing already</param>
        /// <param name="delay">Time delay in seconds</param>
        /// @see XcelerateGames.UI.Animations.UiAnim
        public virtual void PlayAnim(string animName, bool playIfNoAnimIsPlaying = false, float delay = 0f)
        {
            if (_UiAnim != null)
            {
                bool canPlay = !playIfNoAnimIsPlaying || !_UiAnim.IsAnyAnimPlaying;
                if (canPlay)
                    _UiAnim.Play(animName, delay);
            }
        }

        /// <summary>
        /// Play given animation of XcelerateGames.UI.Animations.UiAnim animation in reverse
        /// </summary>
        /// <param name="animName"></param>
        /// @see XcelerateGames.UI.Animations.UiAnim
        public virtual void PlayAnimReverse(string animName)
        {
            if (_UiAnim != null)
                _UiAnim.PlayReverse(animName);
        }

        /// <summary>
        /// Stop animation of XcelerateGames.UI.Animations.UiAnim animation
        /// </summary>
        /// <param name="animName"></param>
        /// @see XcelerateGames.UI.Animations.UiAnim
        public virtual void StopAnim(string animName)
        {
            if (_UiAnim != null)
            {
                //Stop the animation
                _UiAnim.Play(animName);
                //Play Idel anim
                _UiAnim.Play("Idle");
            }
        }

        /// <summary>
        /// This is called as an AnimationEvent
        /// </summary>
        /// <param name="animName">Animation name.</param>
        public virtual void OnAnimDone(string animName)
        {
        }

        /// <summary>
        /// Disable game object
        /// Specially writen to be used by animator
        /// </summary>
        public virtual void DisableGameObject()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Enable game object
        /// Specially writen to be used by animator
        /// </summary>
        public virtual void EnableGameObject()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Callback from Unity when this object is being disabled
        /// </summary>
        protected virtual void OnDisable()
        {
            if (UiBase.pMouseOverItem == this && !gameObject.activeInHierarchy)
                UiBase.pMouseOverItem = null;
            ApplyState(TriggerStateType.OnDisable);
        }

        /// <summary>
        /// Callback from Unity when the parent of this object changed
        /// </summary>
        void OnTransformParentChanged()
        {
#if AUTOMATION_ENABLED
            if (!string.IsNullOrEmpty(ID))
                IDMapper.ChangeIDPath(ID, gameObject.GetObjectPath());
#endif
        }

        /// <summary>
        /// Get color of this item.
        /// If Image componennt is present, its color is returned
        /// else if RawImage componennt is present, its color is returned
        /// </summary>
        /// <returns>Color</returns>
        /// @see SetColor(Color, Color)
        /// @see SetColor(Color)
        public virtual Color GetColor()
        {
            if (_Image != null)
                return _Image.color;
            if (_RawImage != null)
                return _RawImage.color;
            XDebug.LogException($"Error! GetColor: _Image & _RawImage are null");
            return Color.black;
        }

        /// <summary>
        /// Set Color of this item
        /// If Image componennt is present, its color is set
        /// else if RawImage componennt is present, its color is set
        /// </summary>
        /// <param name="inColor">Color to set</param>
        /// @see SetColor(Color, Color)
        public virtual void SetColor(Color inColor)
        {
            if (_Image != null)
                _Image.color = inColor;
            if (_RawImage != null)
                _RawImage.color = inColor;
        }

        /// <summary>
        /// Set color of Image & text component
        /// </summary>
        /// <param name="imageColor">Color to be applied to Image component</param>
        /// <param name="textColor">Color to be applied to Text component</param>
        /// @see SetColor(Color)
        public virtual void SetColor(Color imageColor, Color textColor)
        {
            if (_Image != null)
                _Image.color = imageColor;
            if (_RawImage != null)
                _RawImage.color = imageColor;
            if (_TextItem != null)
                _TextItem.color = textColor;
        }

        /// <summary>
        /// Set custom data to this widget. Helpful in storing data for processing 
        /// </summary>
        /// <param name="userData">Custom data to set</param>
        /// @see GetUserData
        public void SetUserData(UiItemData userData)
        {
            mUserData = userData;
        }

        /// <summary>
        /// Return the custom data set via SetUserData(UiItemData)
        /// </summary>
        /// <returns>Custom data</returns>
        public UiItemData GetUserData()
        {
            return mUserData;
        }

        /// <summary>
        /// Called by unity when the this object is being destroyed.
        /// Unloads texture if _UnloadTexture is set to true
        /// </summary>
        protected override void OnDestroy()
        {
            if (_UnloadTexture && !string.IsNullOrEmpty(mRawUrl))
            {
                ResourceManager.Kill(mRawUrl);
                if (mUsingCachedAsset)
                    DestroyImmediate(_RawImage.texture, true);
                else
                    ResourceManager.Unload(mRawUrl);
            }
#if AUTOMATION_ENABLED
            if (!string.IsNullOrEmpty(ID))
                IDMapper.UnRegisterID(ID);
#endif
            base.OnDestroy();
        }

        /// <summary>
        /// Reset the state of this object
        /// </summary>
        void Reset()
        {
#if AUTOMATION_ENABLED
            Debug.Log("Reset called");
#endif
        }

        /// <summary>
        /// Context menu that works oly in Unity to help get refernces of components attached. Ex: Image, RawImage, Button & Text
        /// </summary>
        [ContextMenu("Get References")]
        private void GetReferences()
        {
            if (_Image == null)
                _Image = GetComponentInChildren<Image>();
            if (_RawImage == null)
                _RawImage = GetComponentInChildren<RawImage>();
            if (_TextItem == null)
                _TextItem = GetComponentInChildren<TextMeshProUGUI>(true);
            if (_UiAnim == null)
                _UiAnim = GetComponent<UiAnim>();
            if (_Button == null)
                _Button = GetComponent<Button>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity in IDE only to validate values
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            GetReferences();
            if (_OnClick != null && _OnClick.GetPersistentEventCount() > 0 && _OnClick.GetPersistentTarget(0) != null && _Button == null && GetComponent<Toggle>() == null)
            {
                Debug.Log("Click event added & no Button component found, Adding Button component");
                gameObject.AddComponent<Button>();
            }
        }
#endif //UNITY_EDITOR

        /// <summary>
        /// Called by unity when this object is enabled
        /// </summary>
        protected virtual void OnEnable()
        {
            ApplyState(TriggerStateType.OnEnable);
        }

        /// <summary>
        /// This function is used to apply the state of _ObjStatesArray items on themself on the basis of trigger event and object state
        /// </summary>
        /// <param name="triggerStateEnum">Trigger event</param>
        private void ApplyState(TriggerStateType triggerStateEnum)
        {
            if (_ObjStatesArray == null) return;

            for (int i = 0; i < _ObjStatesArray.Length; i++)
            {
                GameObject obj = _ObjStatesArray[i]._GameObject;
                if (obj == null || !_ObjStatesArray[i]._TriggerState.HasFlag(triggerStateEnum)) continue;

                ObjectStateType state = _ObjStatesArray[i]._ObjectState;
                switch (state)
                {
                    case ObjectStateType.Visible:
                    case ObjectStateType.Invisible:
                        obj.SetActive(state == ObjectStateType.Visible);
                        break;

                    case ObjectStateType.Interactable:
                    case ObjectStateType.Noninteractable:
                        UiItem item = obj.GetComponent<UiItem>();
                        if (item != null) item.SetInteractive(state == ObjectStateType.Interactable);
                        break;
                }
            }
        }

        /// <summary>
        /// Set parent for the given Transform to the new parent. Parent can be null as well.
        /// </summary>
        /// <param name="transform">Transform whoese parent needs to be set/updated</param>
        /// <param name="parent">New parent. Can be null as well</param>
        public void SetParent(Transform parent)
        {
            SetParent(parent, Vector2.zero, Vector3.one, false);
        }

        /// <summary>
        /// Set parent for the given Transform to the new parent. Parent can be null as well.
        /// </summary>
        /// <param name="transform">Transform whoese parent needs to be set/updated</param>
        /// <param name="parent">New parent. Can be null as well</param>
        /// <param name="anchoredPosition">Anchored Position to set after parenting</param>
        /// <param name="localScale">Local scale to set after parenting</param>
        /// <param name="worldPositionStays">Should Transform postion be retained after setting new parent?</param>
        public void SetParent(Transform parent, Vector2 anchoredPosition, Vector3 localScale, bool worldPositionStays = false)
        {
            transform.SetParent(parent, worldPositionStays);
            RectTransform.anchoredPosition = anchoredPosition;
            transform.localScale = localScale;
        }
    }
}
