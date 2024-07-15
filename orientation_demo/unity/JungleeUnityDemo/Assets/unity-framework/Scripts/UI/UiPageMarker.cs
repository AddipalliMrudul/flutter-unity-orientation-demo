using System.Collections;
using System.Collections.Generic;
using XcelerateGames.UI.Animations;
using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiPageMarker : UiItem
    {
        protected override void Awake()
        {
            base.Awake();

            //Animation is under child object, we are re-referencing child anim here
            _UiAnim = GetComponentInChildren<UiAnim>();
        }
    }
}
