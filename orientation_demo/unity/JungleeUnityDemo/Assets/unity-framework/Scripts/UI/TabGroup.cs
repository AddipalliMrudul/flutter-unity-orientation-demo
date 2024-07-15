using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
Altaf, Feb 25, 2017
Attach tis script to a parent GameObject InputFileds that you want to be controlled via tab
*/

namespace XcelerateGames.UI
{
    public class TabGroup : MonoBehaviour
    {
        public List<TabGroupItem> _TabItems = null;

        private void Start()
        {
            //_TabItems = new List<TabGroupItem>(GetComponentsInChildren<TabGroupItem>());
            _TabItems.Sort(delegate (TabGroupItem item1, TabGroupItem item2)
            {
                return item1._TabOrder.CompareTo(item2._TabOrder);
            });
            //foreach (TabGroupItem item in _TabItems)
            //item._TabGroup = this;
            SetSelected(0);
        }

        public void OnTabPressed(int order)
        {
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                TabGroupItem item = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().GetComponent<TabGroupItem>();
                if (item != null)
                {
                    int startIndex = _TabItems.FindIndex(e => e == item);
                    int index = startIndex + 1;
                    if (Input.GetKey(KeyCode.LeftShift))
                        index = startIndex - 1;
                    if (index >= _TabItems.Count)
                        index = 0;
                    else if (index < 0)
                        index = _TabItems.Count - 1;
                    //Altaf:TODO:Handle non-interactive / in-active objects
                    SetSelected(index);
                }
            }
        }

        private void SetSelected(int index)
        {
            if(index >= 0 && index < _TabItems.Count)
                EventSystem.current.SetSelectedGameObject(_TabItems[index].gameObject);
        }
    }
}