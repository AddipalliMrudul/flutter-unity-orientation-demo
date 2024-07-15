using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchZoom : MonoBehaviour
{
    public float _PerspectiveZoomSpeedTouch = 0.05f;
    public float _PerspectiveZoomSpeedMouse = 12f;
    public float _OrthoZoomSpeed = 0.5f;

    public Camera _Camera = null;

    private void Awake()
    {
        if (_Camera == null)
            _Camera = Camera.main;
    }

    void Update()
    {
        if (_Camera == null)
            return;
#if UNITY_EDITOR || UNITY_STANDALONE

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (_Camera.orthographic)
        {
            _Camera.orthographicSize += scroll * _OrthoZoomSpeed;
            _Camera.orthographicSize = Mathf.Max(_Camera.orthographicSize, 0.1f);
        }
        else
        {
            _Camera.fieldOfView += scroll * _PerspectiveZoomSpeedMouse;
            _Camera.fieldOfView = Mathf.Clamp(_Camera.fieldOfView, 0.1f, 179.9f);
        }
#else

        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            Vector2 prevPos2 = touch2.position - touch2.deltaPosition;

            float prevDelatMag = (prevPos1 - prevPos2).magnitude;
            float delatMag = (touch1.position - touch2.position).magnitude;

            float deltaMagDiff = prevDelatMag - delatMag;

            if (_Camera.orthographic)
            {
                _Camera.orthographicSize += deltaMagDiff * _OrthoZoomSpeed;
                _Camera.orthographicSize = Mathf.Max(_Camera.orthographicSize, 0.1f);
            }
            else
            {
                _Camera.fieldOfView += deltaMagDiff * _PerspectiveZoomSpeedTouch;
                _Camera.fieldOfView = Mathf.Clamp(_Camera.fieldOfView, 5f, 150f);
            }
        }
#endif
    }
}
