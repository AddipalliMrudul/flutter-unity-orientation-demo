using System;
using UnityEngine;

/* Purpose: Enable/Disable other GameObjects by another GameObjects state
 * Note: If GameObject on which this script is attached is disabled in the scene on load, then this wont set state of other objects
 */

namespace XcelerateGames
{
    public class GameObjectStateController : MonoBehaviour
    {
        [Serializable]
        public class StateData
        {
            public StateType stateType = StateType.None;
            public GameObject[] _Objects = null;
        }

        public enum StateType
        {
            None,
            EnableOnEnable,
            DisableOnEnable,
            EnableOnDisable,
            DisableOnDisable,
        }

        public StateData[] _StateData = null;

        private void OnDisable()
        {
            ApplyState(StateType.EnableOnDisable, true);
            ApplyState(StateType.DisableOnDisable, false);
        }

        private void OnEnable()
        {
            ApplyState(StateType.EnableOnEnable, true);
            ApplyState(StateType.DisableOnEnable, false);
        }

        private void ApplyState(StateType stateType, bool active)
        {
            StateData stateData = Array.Find(_StateData, e => e.stateType == stateType);
            if (stateData != null)
            {
                Array.ForEach(stateData._Objects, e =>
                {
                    if (e != null)
                    {
                        //Debug.Log($"stateType:{stateType}, active:{active}, obj:{e.GetObjectPath()}");
                        e.SetActive(active);
                    }
                });
            }
        }
    }
}
