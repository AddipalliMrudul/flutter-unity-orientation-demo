using UnityEngine;

/* Author : Altaf
 * Date : July 04, 2018
 * Purpose : If Canvas RenderMode is set to : RenderMode.ScreenSpaceCamera, Then attach this script to the Canvas object, it will automatically assign camera to it.
 * 
*/

namespace XcelerateGames.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UiAttachCamera : MonoBehaviour
    {
        public Camera _Camera = null;

        void Awake()
        {
            Canvas canvas = GetComponent<Canvas>();
            canvas.worldCamera = (_Camera != null ? _Camera : Camera.main);
        }
    }
}
