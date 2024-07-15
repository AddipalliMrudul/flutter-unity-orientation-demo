using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiFirebaseTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickException()
    {
        Debug.Log("OnClickException");
        //Firebase.Crashlytics.Crashlytics.LogException(new System.Exception("Exception " + Time.frameCount));
    }

    public void OnClickFirebaseEvent()
    {
        Debug.Log("OnClickFirebaseEvent");
        //Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);
    }

    public void OnClickFirebaseCustomEvent()
    {
        Debug.Log("OnClickFirebaseCustomEvent");
        //Firebase.Analytics.FirebaseAnalytics.LogEvent("CustomEvent", "env1", "qa1");
    }
}
