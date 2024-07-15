using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using XcelerateGames.FlutterWidget;

namespace XcelerateGames
{
    /// <summary>
    /// @note only use when ResourceManager is not ready otherwise use SceneLoader
    /// Load a scene after the given delay
    /// </summary>
    public class UnitySceneLoader : SceneLoader
    {
        /// <summary>
        /// Start Loading the scene
        /// </summary>
        protected override void LoadSene()
        {
            //Debug.Log($"LTS: UnitySceneLoader LoadSene : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");

            SceneManager.LoadScene(_SceneName);
        }
    }
}
