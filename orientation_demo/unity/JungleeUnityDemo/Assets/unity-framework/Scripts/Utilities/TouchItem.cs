using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames
{
    public class TouchItem : MonoBehaviour
    {
        public float _HoldTime = 1f;

        internal void Show()
        {
            gameObject.SetActive(true);
            CancelInvoke();
            Invoke("Hide", _HoldTime);
        }
    }
}
