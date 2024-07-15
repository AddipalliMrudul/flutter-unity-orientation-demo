using UnityEngine;
using UnityEngine.UI;

/* Author : Altaf
 * Date : April 19, 2018
 * Purpose : Toggle triggers onValueChanged event only on user input, 
 * If we have logic written in onValueChanged callback, then that piece of code wont execute till user clicks on it,
 * to fix this, we attach this script to Taggle object, it triggers a onValueChanged event on Awake.
*/
namespace XcelerateGames.UI
{
    [RequireComponent(typeof(Toggle))]
    public class UiToggleEventInvoker : MonoBehaviour
    {
        void Awake()
        {
            Toggle toggle = GetComponent<Toggle>();
            toggle.onValueChanged.Invoke(toggle.isOn);
        }
    }
}
