using System;
using UnityEngine;

/* Purpose: Enable/Disable GameObjects based on current environment
 * Note: If GameObject on which this script is attached is disabled in the scene on load, then this wont set state of other objects
 */

namespace XcelerateGames
{
    public class GameObjectStateControllerByEnv : MonoBehaviour
    {
        [Serializable]
        public class EnvData
        {
			public PlatformUtilities.EnvironmentMask envType = PlatformUtilities.EnvironmentMask.None;
            public GameObject[] _Objects = null;
        }

        public enum ActionType
		{
			None,
			Enable,
			Disable,
			Destroy,
		}

		public EnvData[] _EnvData = null;
        public ActionType _ActionType = ActionType.None;

        void Awake()
		{
            ApplyState();
		}

        private void ApplyState()
        {
            EnvData stateData = Array.Find(_EnvData, e => PlatformUtilities.HasEnvironment(e.envType));
            if (stateData != null)
            {
                Array.ForEach(stateData._Objects, e =>
                {
                    if (e != null)
                    {
                        ApplyAction(e);
                    }
                });
            }
        }

        private void ApplyAction(GameObject obj)
        {
            switch(_ActionType)
            {
                case ActionType.Enable:
                    obj.SetActive(true);
                    break;
                case ActionType.Disable:
                    obj.SetActive(false);
                    break;
                case ActionType.Destroy:
                    Destroy(obj);
                    break;
            }
        }
    }
}
