using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiRotate : MonoBehaviour
{
    public Vector3 _RotationStep = Vector3.zero;
    public float _Time = 0.25f;

    private float mElapsedTime = 0f;
    private Transform mTransform = null;
    private Vector3 mTargetRotation = Vector3.zero;

    private void Awake()
    {
        mTransform = transform;
    }

    void Update()
    {
        mElapsedTime += Time.deltaTime;
        float t = mElapsedTime / _Time;
        if(t < 1)
            mTransform.localRotation = Quaternion.Euler(Vector3.Slerp(mTransform.localRotation.eulerAngles, mTargetRotation, t));
        else
        {
            mTransform.localRotation = Quaternion.Euler(mTargetRotation);
            enabled = false;
        }
    }

    public void OnClickRotateRight()
    {
        enabled = true;
        mTargetRotation = mTransform.localRotation.eulerAngles + (_RotationStep * -1);
        mElapsedTime = 0f;
    }

    public void OnClickRotateLeft()
    {
        enabled = true;
        mTargetRotation = mTransform.localRotation.eulerAngles + _RotationStep;
        mElapsedTime = 0f;
    }
}
