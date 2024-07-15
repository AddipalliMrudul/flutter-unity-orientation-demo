using XcelerateGames;
using UnityEngine;

public class CacheConsole : MonoBehaviour, IConsole
{
    void Start()
    {
        UiConsole.Register("cache", this);
    }

    public void OnHelp()
    {
        UiConsole.WriteLine("Help from CacheConsole");
    }

    public bool OnExecute(string[] args, string baseCommand)
    {
        if (Utilities.Equals(args[0],"clear") || Utilities.Equals(args[0], "cls"))
        {
            UiConsole.WriteLine("Successfully cleared cache.");
            System.IO.Directory.Delete(PlatformUtilities.GetPersistentDataPath(), true);
            UnityEngine.PlayerPrefs.DeleteAll();
            UnityEngine.PlayerPrefs.Save();
        }
        return true;
    }
}
