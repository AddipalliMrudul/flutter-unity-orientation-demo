using XcelerateGames;
using UnityEngine;

public class PlayerPrefsHelper : MonoBehaviour
{
    public bool _DeleteAll = false;
    public string[] _Keys = null;

    private void Awake()
    {
#if UNITY_EDITOR
        if (_DeleteAll)
            UnityEngine.PlayerPrefs.DeleteAll();
        else
        {
            foreach (string key in _Keys)
            {
                if (PlayerPrefs.HasKey(key))
                    PlayerPrefs.DeleteKey(key);
                else
                    Debug.LogWarning("Key not found : " + key);
            }
        }
#else
        XDebug.LogException("The script PlayerPrefsHelper is supposed to be used in IDE only.");
#endif
    }
}