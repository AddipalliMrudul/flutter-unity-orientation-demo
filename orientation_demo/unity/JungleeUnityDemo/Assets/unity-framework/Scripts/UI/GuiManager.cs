using System.Collections;
using UnityEngine;

namespace XcelerateGames.UI
{
    public class GuiManager : MonoBehaviour
    {
        public static GuiManager pInstance { get; private set; }

        [HideInInspector]
        public bool mBlockClickEvents = false;

        public static bool pBlockClickEvents
        {
            get
            {
                if (pInstance == null)
                    new GameObject("GuiManager").AddComponent<GuiManager>();
                return pInstance.mBlockClickEvents;
            }
            set
            {
                pInstance.mBlockClickEvents = value;
            }
        }

        private void Awake()
        {
            pInstance = this;
            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator WaitAndEnable(float delay)
        {
            mBlockClickEvents = true;
            yield return new WaitForSecondsRealtime(delay);
            mBlockClickEvents = false;
        }
    }
}