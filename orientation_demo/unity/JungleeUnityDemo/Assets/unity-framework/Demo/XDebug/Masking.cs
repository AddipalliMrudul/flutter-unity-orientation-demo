using UnityEngine;
using XcelerateGames;

public class Masking : MonoBehaviour
{
    void Start()
    {
        //To enable all logs from ResourceManager
        XDebug.AddMask(XDebug.Mask.Resources);

        //You can add multiple masks, Ex ResourceManager & Game
        XDebug.AddMask(XDebug.Mask.Resources | XDebug.Mask.Game);
    }
}
