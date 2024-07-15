using System;
using UnityEngine;

namespace XcelerateGames
{
    [Serializable]
    public class GameObjectData
    {
        public GameObject _Object = null;
        public bool _Enable = false;

        public void Apply()
        {
            if (_Object != null)
                _Object.SetActive(_Enable);
        }
    }
}
