using UnityEngine;

namespace XcelerateGames.Audio
{
    /// <summary>
    /// Class to handle playing of Audio. We can play Audio from either an AudioClip or name of audio clip from asset bundle.
    /// Named audio clip is played from loading the AudioClip from <br>sounds</br> asset bundle.
    /// If an AudioClip is attached & a name of Audio clip is also specified, then Audio clip that is attached will be played. Name is ignored.
    /// </summary>
    [System.Serializable]
    public class AudioVars
    {
        public AudioClip _SoundClip = null;/**<AudioClip to be played */
        public string _SoundName = null;/**<Name of the AudioClip to be played from <br>sounds</br> asset bundle*/
        public AudioController.Category _Category = AudioController.Category.SFX;/**<AudioController.Category for the Audio Clip */
        public float _Delay = 0f;   /**<Delay (in seconds) to play the audio clip */
        public float _Volume = 1f;  /**<Volume of Audio */
        public float _Pitch = 1f;   /**<Pitch of Audio */
        public bool _Loop = false;  /**<Whether to loop the sound? */

        private AudioSource mAudioSource = null;/**<AudioSource of the AudioClip being played */

        /// <summary>
        /// Play the Audio Clip
        /// </summary>
        public void Play()
        {
            if (_SoundClip != null)
                mAudioSource = AudioController.Play(_SoundClip, _Loop, _Category.ToString(), _Volume, _Pitch, _Delay);
            else if (!_SoundName.IsNullOrEmpty())
                mAudioSource = AudioController.Play(_SoundName, _Loop, _Category.ToString(), _Volume, _Pitch, _Delay);
        }

        /// <summary>
        /// Stop sound if its being played
        /// </summary>
        public void Stop()
        {
            AudioController.Stop(mAudioSource);
        }

        /// <summary>
        /// Pause the sound
        /// </summary>
        public void Pause()
        {
            mAudioSource?.Pause();
        }

        /// <summary>
        /// To check whether the respective clip is being played or not
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            if (mAudioSource != null && mAudioSource.clip != null)
            {
                if (_SoundClip == mAudioSource.clip || (_SoundName == mAudioSource.clip.name))
                    return mAudioSource.isPlaying;
            }
            return false;
        }
    }
}
