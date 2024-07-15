using System;
using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames
{
    public class SceneLoadHandler : MonoBehaviour
    {
        public enum Trigger
        {
            None = 0,
            SceneLoadStart = 1 << 1,
            SceneLoadLoaded = 1 << 2,
        }

        public enum ActionType
        {
            None,
            Enable,
            Disable,
            Destroy,
        }

        [SerializeField] string[] _Scenes = null;
        [SerializeField] Trigger _Trigger = Trigger.None;
        [SerializeField] ActionType _ActionType = ActionType.None;

        void Awake()
        {
            ResourceManager.OnSceneLoadedEvent += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            ResourceManager.OnSceneLoadedEvent -= OnSceneLoaded;
        }

        private void OnSceneLoaded(string sceneName)
        {
            if ((_Trigger & Trigger.SceneLoadLoaded) != 0)
            {
                int index = Array.FindIndex(_Scenes, e => e == sceneName);
                if (index >= 0)
                    ApplyAction();
            }
        }

        private void ApplyAction()
        {
            switch (_ActionType)
            {
                case ActionType.Enable:
                    gameObject.SetActive(true);
                    break;
                case ActionType.Disable:
                    gameObject.SetActive(false);
                    break;
                case ActionType.Destroy:
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
