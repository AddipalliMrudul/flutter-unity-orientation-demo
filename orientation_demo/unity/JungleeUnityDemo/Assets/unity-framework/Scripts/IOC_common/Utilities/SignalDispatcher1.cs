using System;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    /// <summary>
    /// Dispatch signal that does not take any argument by type name. Type name must be a fully qualified name with name space
    /// EX: XcelerateGames.SigEngineReady
    /// </summary>
    public class SignalDispatcher1 : MonoBehaviour
    {
        //Fully qualified name of the class
        [SerializeField] protected string _SignalName = null;
        [SerializeField] protected ArgumentData[] _Arguments = null;

        /// <summary>
        /// Call this function to dispatch the signal
        /// </summary>
        public virtual void Dispatch()
        {
            Dispatch(BindingManager.Instance);
        }

        protected virtual void Dispatch(BindingManager bindingManager)
        {
            if (_SignalName.IsNullOrEmpty())
            {
                XDebug.LogError($"{nameof(_SignalName)} is null or empty.");
            }
            else
            {
                Type type = GetType(ArgumentType.Bool);
                AbstractSignal signal = bindingManager.GetSignal<Signal<bool>>(_SignalName);
                if (signal != null)
                {
                    // signal.Dispatch(true);
                }
                else
                {
                    XDebug.LogError($"Failed to find signal of type: {_SignalName}");
                }
            }
        }

        protected virtual Type GetType(ArgumentType argumentType)
        {
            if (argumentType == ArgumentType.Int)
                return Type.GetType("System.Int32");
            if (argumentType == ArgumentType.String)
                return Type.GetType("System.String");
            if (argumentType == ArgumentType.Bool)
                return Type.GetType("System.Boolean");
            if (argumentType == ArgumentType.Float)
                return Type.GetType("System.Single");

            XDebug.LogError($"Invalid argumentType: {argumentType}");
            return null;
        }

        [ContextMenu("Dispatch")]
        private void PerformDispatch()
        {
            Dispatch();
        }
    }
}
