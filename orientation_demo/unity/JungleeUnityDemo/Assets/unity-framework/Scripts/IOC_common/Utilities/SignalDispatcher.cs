using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    /// <summary>
    /// Dispatch signal that does not take any argument by type name. Type name must be a fully qualified name with name space
    /// EX: XcelerateGames.SigEngineReady
    /// </summary>
    public class SignalDispatcher : MonoBehaviour
    {
        //Fully qualified name of the class
        [SerializeField] protected string _SignalName = null;

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
                Signal signal = bindingManager.GetSignal<Signal>(_SignalName);
                if (signal != null)
                {
                    signal.Dispatch();
                }
                else
                {
                    XDebug.LogError($"Failed to find signal of type: {_SignalName}");
                }
            }
        }

        [ContextMenu("Dispatch")]
        private void PerformDispatch()
        {
            Dispatch();
        }
    }
}
