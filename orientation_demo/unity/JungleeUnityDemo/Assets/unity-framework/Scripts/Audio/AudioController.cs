using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using XcelerateGames.AssetLoading;
using XcelerateGames.Pooling;

namespace XcelerateGames.Audio
{
    [System.Serializable]
    public class AudioItem
    {
        public string Name = null;
        public int Probability = 100;
        public float Delay = 0f;
        public float Volume = 1f;
        public float Pitch = 1f;
        public bool Loop = false;

        public AudioItem()
        {
            Probability = 100;
            Volume = 1f;
            Pitch = 1f;
        }
    }

    public enum SoundEvent
    {
        Start,
        End,
        None
    }

    public class AudioController : MonoBehaviour
    {
        public enum Category
        {
            Music,
            VO,
            SFX
        }

        public static bool pIsReady { get { return mIsReady; } }

        [SerializeField]
        private AudioMixerGroup musics = null, sounds = null, vo = null;

        private bool _soundMuted, _musicMuted, _voMuted;
        private bool mIsSoundMuted, mIsMusicMuted, mIsVoMuted;
        private List<AudioSource> musicSources = new List<AudioSource>();
        private List<AudioSource> soundSources = new List<AudioSource>();
        private Dictionary<string, List<AudioSource>> mCategoryMap = new Dictionary<string, List<AudioSource>>();
        private Pool mPool = null;
        private AssetBundle mAssetBundle = null;
        private static AudioController mInstance = null;
        private static bool mIsReady = false;

        public const string CategoryMusic = "Music";
        public const string CategoryVO = "VO";
        public const string CategorySFX = "SFX";

        #region Delegates

        public static System.Action<bool> OnReady = null;
        public static System.Action<AudioClip, SoundEvent> OnAudioClipSoundEvent = null;

        #endregion Delegates

        #region Properties

        public AudioMixerGroup sfxMixerGroup { get { return sounds; } }

        public static bool SoundMuted
        {
            get { return mInstance._soundMuted; }
            set
            {
                mInstance._soundMuted = value;
                mInstance.sounds.audioMixer.SetFloat("SFXVolume", mInstance._soundMuted ? -80 : 0);
            }
        }

        public static bool VOMuted
        {
            get { return mInstance._voMuted; }
            set
            {
                mInstance._voMuted = value;
                mInstance.vo.audioMixer.SetFloat("VOVolume", mInstance._voMuted ? -80 : 0);
            }
        }

        public static bool MusicMuted
        {
            get { return mInstance._musicMuted; }
            set
            {
                mInstance._musicMuted = value;
                MusicVolume = mInstance._musicMuted ? -80 : 0;
            }
        }

        /// <summary>
        /// 0 is normal volume, -20 is low volume, 20 is max volume, -80 is mute
        /// </summary>
        public static float MusicVolume
        {
            get
            {
                float result = 0;
                mInstance.musics.audioMixer.GetFloat("MusicVolume", out result);
                return result;
            }
            set { mInstance.musics.audioMixer.SetFloat("MusicVolume", value); }
        }

        public static void MusicMute(bool mute)
        {
            MusicMuted = mute;
            if (MusicMuted)
                UnityEngine.PlayerPrefs.SetInt("MusicMuted", 0);
            else
                UnityEngine.PlayerPrefs.DeleteKey("MusicMuted");
        }

        public static void SoundMute(bool mute)
        {
            SoundMuted = mute;
            if (SoundMuted)
                UnityEngine.PlayerPrefs.SetInt("SoundMuted", 0);
            else
                UnityEngine.PlayerPrefs.DeleteKey("SoundMuted");
        }

        public static void VOMute(bool mute)
        {
            VOMuted = mute;
            if (VOMuted)
                UnityEngine.PlayerPrefs.SetInt("VOMuted", 0);
            else
                UnityEngine.PlayerPrefs.DeleteKey("VOMuted");
        }

        //Use this function to Mute all audio sources & later restore their vales
        public static void PushState()
        {
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Pushing state SoundMuted : {SoundMuted}, MusicMuted : {MusicMuted}, VOMuted : {VOMuted}", XDebug.Mask.Sound);

                mInstance.mIsSoundMuted = SoundMuted;
            mInstance.mIsMusicMuted = MusicMuted;
            mInstance.mIsVoMuted = VOMuted;

            SoundMuted = MusicMuted = VOMuted = true;
        }

        public static void PopState()
        {
            SoundMuted = mInstance.mIsSoundMuted;
            MusicMuted = mInstance.mIsMusicMuted;
            VOMuted = mInstance.mIsVoMuted;
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Pop state SoundMuted : {SoundMuted}, MusicMuted : {MusicMuted}, VOMuted : {VOMuted}", XDebug.Mask.Sound);
        }
        #endregion Properties

        #region Private Methods

        private void Awake()
        {
            if (mInstance == null)
            {
                mInstance = this;
                GameObject poolObject = new GameObject();
                poolObject.AddComponent<AudioSource>();
                mPool = PoolManager.CreatePool(AudioControllerSettings.pInstance._PoolName, AudioControllerSettings.pInstance._PoolSize, poolObject, true);

                DontDestroyOnLoad(gameObject);
            }
            else
                GameObject.Destroy(gameObject);
        }

        private static void OnAudioBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;

            if (inEvent == ResourceEvent.ERROR)
                XDebug.LogError("Failed to load audio bundle :" + inURL);
            else if (inEvent == ResourceEvent.COMPLETE)
                mInstance.mAssetBundle = inObject as AssetBundle;

            LoadState(inEvent == ResourceEvent.COMPLETE);
        }

        private static void LoadState(bool success)
        {
            MusicMuted = UnityEngine.PlayerPrefs.HasKey("MusicMuted");
            SoundMuted = UnityEngine.PlayerPrefs.HasKey("SoundMuted");
            VOMuted = UnityEngine.PlayerPrefs.HasKey("VOMuted");

            mIsReady = true;
            if (OnReady != null)
                OnReady(success);
        }

        private static AudioSource Play(string assetName, AudioMixerGroup mixerGroup, float volume = 1, bool loop = false, float pitch = 1, float delay = 0f)
        {
            if (!mIsReady || string.IsNullOrEmpty(assetName))
                return null;
            AudioClip clip = mInstance.LoadClip(assetName);
            if (clip == null)
                return null;

            AudioSource source = mInstance.createAudioSource(clip, volume, loop, delay);
            if (source == null)
                return null;
            source.pitch = pitch;
            source.outputAudioMixerGroup = mixerGroup;
            return source;
        }

        private static AudioSource Play(AudioClip clip, AudioMixerGroup mixerGroup, float volume = 1, bool loop = false, float pitch = 1, float delay = 0f)
        {
            if (!mIsReady || clip == null)
                return null;

            AudioSource source = mInstance.createAudioSource(clip, volume, loop, delay);
            if (source == null)
                return null;
            source.pitch = pitch;
            source.outputAudioMixerGroup = mixerGroup;
            return source;
        }

        private AudioClip LoadClip(string path)
        {
            return mAssetBundle.LoadAsset<AudioClip>(path);
        }

        private AudioSource FindSource(string clipName)
        {
            AudioSource source = musicSources.Find(src => src != null && src.clip != null && src.clip.name == clipName);
            if (source == null)
                source = soundSources.Find(src => src != null && src.clip != null && src.clip.name == clipName);

            return source;
        }

        private AudioSource FindSource(AudioClip clip)
        {
            AudioSource source = musicSources.Find(src => src != null && src.clip == clip);
            if (source == null)
                source = soundSources.Find(src => src != null && src.clip != null && src.clip == clip);

            return source;
        }

        private AudioSource createAudioSource(AudioClip clip, float volume = 1, bool loop = true, float delay = 0f)
        {
            GameObject go = mPool.Spawn();
            if (go == null)
                return null;
            go.transform.SetParent(transform);
            //go.SetActive(true);
            go.name = "Audio: " + clip.name;

            AudioSource source = go.GetComponent<AudioSource>();
            if (source == null)
                return null;
            source.clip = clip;
            source.volume = volume;
            source.loop = loop;
            source.playOnAwake = false;
            if (delay > 0f)
                source.PlayDelayed(delay);
            else
                source.Play();
            source.Play();
            if (!loop)
            {
                DespawnPooledObject despawner = go.GetComponent<DespawnPooledObject>();
                if (despawner == null)
                    despawner = go.AddComponent<DespawnPooledObject>();
                despawner.Init(clip.length + delay, mPool);
            }
            return source;
        }

        private void Update()
        {
            for (int i = 0; i < musicSources.Count; ++i)
            {
                if (musicSources[i] != null && !musicSources[i].loop)
                {
                    if (!musicSources[i].isPlaying)
                    {
                        AudioClip clip = musicSources[i].clip;
                        Stop(musicSources[i]);
                        if (OnAudioClipSoundEvent != null)
                        {
                            if (XDebug.CanLog(XDebug.Mask.Sound))
                                XDebug.Log("Sending End event for " + clip.name + " to " + OnAudioClipSoundEvent.Target + "::" + OnAudioClipSoundEvent.Method, XDebug.Mask.Sound);

                            OnAudioClipSoundEvent(clip, SoundEvent.End);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a random AudioItem based on probability.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private static AudioItem GetRandomAudioItem(AudioItem[] items)
        {
            int total = 0;
            int i = 0;
            for (i = 0; i < items.Length; ++i)
                total += items[i].Probability;
            int randNum = UnityEngine.Random.Range(0, total);
            total = 0;
            i = 0;
            while (total < randNum)
                total += items[i++].Probability;

            return items[Mathf.Max(0, i - 1)];
        }

        private static void AddToCategory(string category, AudioSource source)
        {
            RemoveFromCategoryMapping(source);
            if (!mInstance.mCategoryMap.ContainsKey(category))
                mInstance.mCategoryMap.Add(category, new List<AudioSource>());
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Adding {source.clip.name} + to category {category}", XDebug.Mask.Sound);
            mInstance.mCategoryMap[category].Add(source);
        }

        private static void RemoveFromCategory(string category, AudioSource source)
        {
            if (mInstance.mCategoryMap.ContainsKey(category))
                mInstance.mCategoryMap[category].Remove(source);
        }

        private static void RemoveFromCategoryMapping(AudioSource source)
        {
            foreach (string key in mInstance.mCategoryMap.Keys)
                mInstance.mCategoryMap[key].Remove(source);
        }


        #endregion Private Methods

        #region Public Methods

        public static void Init()
        {
            if (AudioControllerSettings.pInstance._SoundAssetBundleName.IsNullOrEmpty())
            {
                LoadState(true);
            }
            else
            {
                //Pre load Sounds bundle
                ResourceManager.Load(AudioControllerSettings.pInstance._SoundAssetBundleName, OnAudioBundleLoaded, inResourceType: ResourceManager.ResourceType.AssetBundle, loadDependency: true, dontDestroyOnLoad: true);
            }
        }

        public static AudioSource Play(string assetName, bool loop = false, string category = CategorySFX, float volume = 1, float pitch = 1, float delay = 0f)
        {
            if (category == CategoryVO)
                return PlayVO(assetName, loop, category, volume, pitch, delay);
            if (category == CategoryMusic)
                return PlayMusic(assetName, category, loop, volume);
            return PlaySFX(assetName, loop, category, volume, pitch, delay);
        }

        public static AudioSource Play(AudioClip audioClip, bool loop = false, string category = CategorySFX, float volume = 1, float pitch = 1, float delay = 0f)
        {
            if (category == CategoryVO)
                return PlayVO(audioClip, loop, category, volume, pitch, delay);
            if (category == CategoryMusic)
                return PlayMusic(audioClip, category, loop, volume, pitch, delay);
            return PlaySFX(audioClip, loop, category, volume, pitch, delay);
        }

        #region Playing SFX 

        public static AudioSource PlaySFX(string assetName, bool loop = false, string category = CategorySFX, float volume = 1, float pitch = 1, float delay = 0f)
        {
            if (mInstance == null || SoundMuted)
                return null;
            AudioSource source = Play(assetName, mInstance == null ? null : mInstance.sounds, volume, loop, pitch, delay);
            if (source == null)
                return null;
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Playing SFX : {assetName}", XDebug.Mask.Sound);
            AddToCategory(category, source);
            mInstance.soundSources.Add(source);
            return source;
        }

        public static AudioSource PlaySFX(AudioClip audioClip, bool loop = false, string category = CategorySFX, float volume = 1, float pitch = 1, float delay = 0f)
        {
            if (mInstance == null || SoundMuted || audioClip == null)
                return null;
            AudioSource source = Play(audioClip, mInstance == null ? null : mInstance.sounds, volume, loop, pitch, delay);
            if (source == null)
                return null;
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Playing SFX : {audioClip.name}", XDebug.Mask.Sound);
            AddToCategory(category, source);
            mInstance.soundSources.Add(source);
            return source;
        }

        public static AudioSource PlaySFX(string[] assetNames, bool loop = false, string category = CategorySFX, float volume = 1, float pitch = 1, float delay = 0f)
        {
            string assetName = assetNames[UnityEngine.Random.Range(0, assetNames.Length)];
            return PlaySFX(assetName, loop, category, volume, pitch, delay);
        }

        public static AudioSource PlaySFX(AudioItem[] audioItems, string category = CategorySFX)
        {
            AudioItem audioItem = GetRandomAudioItem(audioItems);
            return PlaySFX(audioItem.Name, audioItem.Loop, category, audioItem.Volume, audioItem.Pitch, audioItem.Delay);
        }
        #endregion Playing SFX

        #region Playing VO
        public static AudioSource PlayVO(string assetName, bool loop = false, string category = "VO", float volume = 1, float pitch = 1, float delay = 0f)
        {
            if (mInstance == null || VOMuted)
                return null;
            AudioSource source = Play(assetName, mInstance == null ? null : mInstance.vo, volume, loop, pitch, delay);
            if (source == null)
                return null;
            AddToCategory(category, source);
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Playing VO : {assetName}", XDebug.Mask.Sound);
            mInstance.soundSources.Add(source);
            return source;
        }

        public static AudioSource PlayVO(AudioClip audioClip, bool loop = false, string category = "VO", float volume = 1, float pitch = 1, float delay = 0f)
        {
            if (mInstance == null || VOMuted || audioClip == null)
                return null;
            AudioSource source = Play(audioClip, mInstance == null ? null : mInstance.vo, volume, loop, pitch, delay);
            if (source == null)
                return null;
            AddToCategory(category, source);
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Playing VO : {audioClip.name}", XDebug.Mask.Sound);
            mInstance.soundSources.Add(source);
            return source;
        }

        /// <summary>
        /// Randomly plays one VO from a range of Vo names.
        /// </summary>
        /// <param name="assetNames"></param>
        /// <param name="volume"></param>
        /// <param name="loop"></param>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public static AudioSource PlayVO(string[] assetNames, bool loop = false, string category = CategoryVO, float volume = 1, float pitch = 1)
        {
            string assetName = assetNames[UnityEngine.Random.Range(0, assetNames.Length)];
            return PlayVO(assetName, loop, category, volume, pitch);
        }

        /// <summary>
        /// Plays one random VO or no VO from a list of VO`s based on probability. 
        /// </summary>
        /// <param name="assetNames"></param>
        /// <param name="probablility"> </param>
        /// <param name="volume"></param>
        /// <param name="loop"></param>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public static AudioSource PlayVO(string[] assetNames, int probablility, string category = CategoryVO, bool loop = false, float volume = 1, float pitch = 1)
        {
            if (UnityEngine.Random.Range(0, 100) < probablility)
            {
                string assetName = assetNames[UnityEngine.Random.Range(0, assetNames.Length)];
                return PlayVO(assetName, loop, category, volume, pitch);
            }
            return null;
        }

        public static AudioSource PlayVO(string assetName, int probablility, string category = CategoryVO, bool loop = false, float volume = 1, float pitch = 1)
        {
            if (UnityEngine.Random.Range(0, 100) < probablility)
                return PlayVO(assetName, loop, category, volume, pitch);
            return null;
        }

        public static AudioSource PlayVO(AudioItem[] audioItems, string category = CategoryVO)
        {
            AudioItem audioItem = GetRandomAudioItem(audioItems);
            return PlayVO(audioItem.Name, audioItem.Loop, category, audioItem.Volume, audioItem.Pitch, audioItem.Delay);
        }
        #endregion Playing VO

        #region Playing Music
        public static AudioSource PlayMusic(string assetName, string category = CategoryMusic, bool loop = true, float volume = 1)
        {
            if (mInstance == null || string.IsNullOrEmpty(assetName))
                return null;
            AudioSource source = mInstance.FindSource(assetName);
            if (source != null)
            {
                source.gameObject.SetActive(true);
                source.Play();
                source.loop = loop;
                return source;
            }

            source = Play(assetName, mInstance == null ? null : mInstance.musics, volume, loop);
            if (source == null)
                return null;
            AddToCategory(category, source);
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Playing Music : {assetName}", XDebug.Mask.Sound);
            mInstance.musicSources.Add(source);

            return source;
        }

        public static AudioSource PlayMusic(AudioClip clip, string category = CategoryMusic, bool loop = true, float volume = 1, float pitch = 1, float delay = 0f)
        {
            if (!mIsReady || clip == null)
                return null;
            AudioSource source = mInstance.FindSource(clip);
            if (source != null)
            {
                source.gameObject.SetActive(true);
                source.Play();
                source.loop = loop;
                return source;
            }

            source = Play(clip, mInstance == null ? null : mInstance.musics, volume, loop, pitch, delay);
            if (source == null)
                return null;
            AddToCategory(category, source);
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Playing Music : {clip.name}", XDebug.Mask.Sound);
            mInstance.musicSources.Add(source);

            return source;
        }

        /// <summary>
        /// Determines if is playing music for the specified AudioClip.
        /// </summary>
        /// <returns><c>true</c> if is playing music for the specified assetName; otherwise, <c>false</c>.</returns>
        /// <param name="assetName">Asset name.</param>
        public static bool IsPlayingMusic(AudioClip audioClip)
        {
            if (mInstance == null || audioClip == null || mInstance.musicSources == null)
                return false;

            for (int i = 0; i < mInstance.musicSources.Count; i++)
            {
                if (mInstance.musicSources[i] != null && mInstance.musicSources[i].clip != null && audioClip != null)
                {
                    if (mInstance.musicSources[i].clip.name == audioClip.name)
                    {
                        if (mInstance.musicSources[i].clip != null)
                            return mInstance.musicSources[i].isPlaying;
                        XDebug.LogError($"Deleted : {audioClip.name}");
                        return false;
                    }
                }
            }

            mInstance.musicSources.RemoveAll((source) => source == null);

            return false;
        }
        #endregion Playing Music


        /// <summary>
        /// Determines if is playing sfx for the specified assetName.
        /// </summary>
        /// <returns><c>true</c> if is playing sfx for the specified assetName; otherwise, <c>false</c>.</returns>
        /// <param name="assetName">Asset name.</param>
        public static bool IsPlayingSound(string assetName)
        {
            if (mInstance == null || mInstance.soundSources == null || assetName == null)
                return false;

            for (int i = 0; i < mInstance.soundSources.Count; i++)
            {
                if (mInstance.soundSources[i] != null && mInstance.soundSources[i].clip != null && mInstance.soundSources[i].clip.name != null)
                {
                    if (mInstance.soundSources[i].clip.name == assetName)
                    {
                        if (mInstance.soundSources[i].clip != null)
                            return mInstance.soundSources[i].isPlaying;
                        XDebug.LogError($"Deleted : {assetName}");
                        return false;
                    }
                }
            }

            mInstance.musicSources.RemoveAll((source) => source == null);

            return false;
        }

        /// <summary>
        /// Determines if is playing sfx for the specified clip.
        /// </summary>
        /// <returns><c>true</c> if is playing sfx for the specified clip; otherwise, <c>false</c>.</returns>
        /// <param name="clip">Asset name.</param>
        public static bool IsPlayingSound(AudioClip clip)
        {
            if (mInstance == null || clip == null || mInstance.soundSources == null)
                return false;

            for (int i = 0; i < mInstance.soundSources.Count; i++)
            {
                if (mInstance.soundSources[i] != null && mInstance.soundSources[i].clip != null)
                {
                    if (mInstance.soundSources[i].clip.name == clip.name)
                    {
                        if (mInstance.soundSources[i].clip != null)
                            return mInstance.soundSources[i].isPlaying;
                        XDebug.LogError($"Deleted : {clip.name}");
                        return false;
                    }
                }
            }

            mInstance.musicSources.RemoveAll((source) => source == null);

            return false;
        }

        public static bool IsPlayingCategory(string categoryName)
        {
            bool isPlaying = false;
            if (mInstance.mCategoryMap.ContainsKey(categoryName))
            {
                for (int i = 0; i < mInstance.mCategoryMap[categoryName].Count; ++i)
                {
                    if (mInstance.mCategoryMap[categoryName][i].isPlaying)
                        return true;
                }
            }
            return isPlaying;
        }

        public static List<AudioSource> GetAudioSourceByCategory(string categoryName)
        {
            if (mInstance.mCategoryMap.ContainsKey(categoryName))
                return mInstance.mCategoryMap[categoryName];
            return null;
        }

        public static void Stop(AudioSource source)
        {
            if (source == null || source.clip == null)
                return;
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Stopping : {source.clip.name}", XDebug.Mask.Sound);
            source.Stop();
            source.clip = null;
            source.volume = 0;

            DespawnPooledObject despawner = source.gameObject.GetComponent<DespawnPooledObject>();
            if (despawner != null)
                despawner.CancelInvoke();

            mInstance.musicSources.Remove(source);
            mInstance.soundSources.Remove(source);

            mInstance.soundSources.RemoveAll(e => e.clip == null);
            RemoveFromCategoryMapping(source);

            mInstance.mPool.Despawn(source.gameObject);
        }

        public static void StopMusic(string inName)
        {
            AudioSource source = mInstance.FindSource(inName);
            if (source != null)
                Stop(source);
        }

        public static void StopMusic(AudioSource source)
        {
            if (source != null)
                Stop(source);
        }

        public static void StopAllMusic()
        {
            if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log("Stopping all music", XDebug.Mask.Sound);
            while (mInstance.musicSources.Count != 0)
            {
                if (mInstance.musicSources[0] != null)
                    Stop(mInstance.musicSources[0]);
            }
        }

        public static void StopCategory(string categoryName)
        {
            if (mInstance.mCategoryMap.ContainsKey(categoryName))
            {
                if (XDebug.CanLog(XDebug.Mask.Sound)) XDebug.Log($"Stoping all sounds in Category {categoryName}", XDebug.Mask.Sound);
                while (mInstance.mCategoryMap[categoryName].Count != 0)
                {
                    AudioSource audioSource = mInstance.mCategoryMap[categoryName][0];
                    Stop(audioSource);
                    mInstance.mCategoryMap[categoryName].Remove(audioSource);
                }
                mInstance.mCategoryMap[categoryName].Clear();
            }
            else
                XDebug.LogWarning($"Warning! StopCategory Category {categoryName} not found", XDebug.Mask.Sound);
        }

        public static void Stop(string assetName)
        {
            AudioSource source = mInstance.FindSource(assetName);
            if (source != null)
                Stop(source);
        }

        public static void Stop(AudioClip clip)
        {
            AudioSource source = mInstance.FindSource(clip);
            if (source != null)
                Stop(source);
        }

#if UNITY_EDITOR

        public static void Dump()
        {
            foreach (string categoryName in mInstance.mCategoryMap.Keys)
            {
                XDebug.LogError($"All sounds in Category {categoryName}", XDebug.Mask.Sound);
                foreach (AudioSource audioSource in mInstance.mCategoryMap[categoryName])
                {
                    if (audioSource != null && audioSource.clip != null)
                        UnityEngine.Debug.LogError(audioSource.clip.name + " : " + categoryName + " : " + audioSource.GetInstanceID());
                }
            }
        }
#endif
        #endregion Public Methods
    }
}
