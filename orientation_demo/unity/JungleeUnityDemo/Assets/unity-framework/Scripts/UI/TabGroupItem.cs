using UnityEngine;
using UnityEngine.UI;

/*
Altaf, Feb 25, 2017
Attach this script to InputFileds object that you want to be controlled via tab. Specify the order in which you want them to be rotated
*/

namespace XcelerateGames.UI
{
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(UiInputItem))]
    public class TabGroupItem : MonoBehaviour
    {
        public int _TabOrder = -1;

        //public TabGroup _TabGroup = null;

        private void Start()
        {
            //if (GetComponent<InputField>() == null)
                //throw new MissingComponentException("InputField component not found under : " + this.GetObjectPath());
        }
    }
}