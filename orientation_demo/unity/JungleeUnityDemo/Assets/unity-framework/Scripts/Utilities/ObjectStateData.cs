using System;
using UnityEngine;

namespace XcelerateGames 
{
    /// <summary>
    /// This enum is use for Object state. Add more if needed 
    /// </summary>
    public enum ObjectStateType
    {
        Visible,
        Invisible,
        Interactable,
        Noninteractable
    }

    /// <summary>
    /// This enum is being use for different event (Lifecycle + Other). Used as flag to choose one or more event.
    /// </summary>
    [Flags]
    public enum TriggerStateType
    {
        OnEnable            = (1<<1),
        OnDisable           = (1<<2),
        OnSetInteracive     = (1<<3),
        OnSetNonInteractive = (1<<4)
    }

    /// <summary>
    /// Data wrapper class for game object and trigger state which will be responsible  to change this game obj state to defined Object State 
    /// </summary>
    [Serializable]
    public class ObjectStateData
    {
        public ObjectStateType _ObjectState;
        public GameObject _GameObject;
        public TriggerStateType _TriggerState;
    }
}