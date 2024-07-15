using UnityEngine;
using System.Collections;

namespace XcelerateGames
{
    public class ObjectRotator : MonoBehaviour
    {
        public Vector2 _Sensitivity = new Vector2(0.4f, 0.4f);
        public Transform _Target = null;
        private Vector3 _mouseReference;
        private Vector3 _mouseOffset;
        private Vector3 _rotation;
        private bool _isRotating;

        private void Awake()
        {
            if (_Target == null)
                _Target = transform;
        }

        void Start()
        {
            _rotation = Vector3.zero;
        }

        void Update()
        {
            if (_isRotating)
            {
                // offset
                _mouseOffset = (Input.mousePosition - _mouseReference);
                // apply rotation
                //_rotation.y = -(_mouseOffset.x + _mouseOffset.y) * _sensitivity;
                _rotation.y = -(_mouseOffset.x) * _Sensitivity.y;
                _rotation.x = -(_mouseOffset.y) * _Sensitivity.x;
                // rotate
                //transform.Rotate(_rotation);
                _Target.eulerAngles += _rotation;
                // store mouse
                _mouseReference = Input.mousePosition;
            }
        }

        private void LateUpdate()
        {
            if (Input.touchCount == 3 || Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.R))
                _Target.localRotation = Quaternion.identity;
        }

        void OnMouseDown()
        {
            // rotating flag
            _isRotating = true;

            // store mouse
            _mouseReference = Input.mousePosition;
        }

        void OnMouseUp()
        {
            // rotating flag
            _isRotating = false;
        }
    }
}