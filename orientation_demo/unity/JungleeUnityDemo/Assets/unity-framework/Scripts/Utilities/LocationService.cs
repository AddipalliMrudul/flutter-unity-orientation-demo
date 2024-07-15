using UnityEngine;
using System.Collections;
using System;

namespace XcelerateGames
{
    public class LocationService : MonoBehaviour
    {
        private Action<double,double,double> OnReady = null;

        public static void Init(Action<double,double,double> callback)
        {
            LocationService locationService = new GameObject("LocationService").AddComponent<LocationService>();
            locationService.OnReady = callback;
        }

        IEnumerator Start()
        {
            // First, check if user has location service enabled
            if (!Input.location.isEnabledByUser)
                yield break;

            // Start service before querying location
            Input.location.Start();

            // Wait until service initializes
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            // Service didn't initialize in 20 seconds
            if (maxWait < 1)
            {
                Debug.LogError("Timed out");
                yield break;
            }

            // Connection has failed
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.LogError("Unable to determine device location");
                yield break;
            }
            else
            {
                // Access granted and location value could be retrieved
                Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            }

            // Stop service if there is no need to query location updates continuously
            Input.location.Stop();
            OnReady?.Invoke(Input.location.lastData.latitude, Input.location.lastData.longitude, Input.location.lastData.altitude);
        }
    }
}