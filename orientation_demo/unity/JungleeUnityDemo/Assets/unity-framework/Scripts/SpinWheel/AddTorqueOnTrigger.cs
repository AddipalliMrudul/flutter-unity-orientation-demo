using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.Audio;

namespace XcelerateGames.SpinWheel
{
    /// <summary>
    /// This class moves the Pin when pin collides with spoke's metal button
    /// </summary>
    public class AddTorqueOnTrigger : BaseBehaviour
    {
        [SerializeField] private AudioVars _AudioVars;
        [SerializeField] private int _Torque = 5;

        /// <summary>
        /// Add torque to pin for rotational movement and plays audio
        /// Please note audio will only be played when framework is initialized
        /// </summary>
        /// <param name="collision">Collider2D</param>
        private void OnTriggerExit2D(Collider2D collision)
        {
            _AudioVars.Play();
            transform.GetComponent<Rigidbody2D>().AddTorque(_Torque, ForceMode2D.Impulse);
        }
    }
}