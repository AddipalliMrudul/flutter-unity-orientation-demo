using UnityEngine;
using System.Collections.Generic;
using XcelerateGames.Pooling;
using XcelerateGames.UI;
using XcelerateGames.IOC;
using System;
using XcelerateGames.AssetLoading;

namespace XcelerateGames
{
    public enum NotificationHideAction
    {
        Timer,
        Callback,
        ButtonClick
    }

    //public enum NotificationUIType
    //{
    //    RectangularToast,
    //    SquareCard,
    //    RectToastOutlined
    //}

    public enum NotificationPosition
    {
        UpperLeft,
        UpperCenter,
        UpperRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        LowerLeft,
        LowerCenter,
        LowerRight,
        Custom,
        Default
    }

    public class Notification
    {
        private static long IDCounter = 0;

        public Sprite Icon = null;/**<icon to be shown with notification>*/
        public string ID; /**< id will be timestamp when notification was created*/
        public float ShowTime = 3f; /**< notification display time*/
        public int Type; /**< notification ui type*/
        public NotificationPosition Position; /**< notification position on scrren*/
        public NotificationHideAction ActionType; /** notification hide on action*/
        public Vector2 CustomPosition; /**< only use when NotificationType is custom*/
        public string Message; /**<send localized string */
        public int Priority; /**<based on the priority notification will show one by one */
        public bool CanClearOtherNotifications = false;  /**< can clear all other notification which waitingfor their time to show but wait for the one which is on screen to close if show immediatly is false */
        public bool ShowImmediately = false; /**<will clear the current notfication if of same or lower priority and show self */
        public bool ShowLoader = false; /**< show looader ui if true */
        public Action ButtonCallback = null; /**< only needed if someaction required on button cklick other than hiding notification */
        public bool IsLinkButton = false; /**< show link button if on */ 
        public bool ShowBlockPanel = false; /**< show a back panel which will not allow user to click anywhere on screen while showing notification */
        public string ButtonText = null; /**< only change if need to change button text from OK to something else*/
        public Transform Parent = null; /**< notificayion parent */
        public bool CanChangeSortOrder = false; /**< Mark true if sort order need to change for particular notification and also pass sortorder. By default it will take from the canvas component. */
        public int SortOrder = 0; /**< Sort Order for UI */
        public bool CanvasEnabled = true; /**< if true canvas is enabled */
        public int ButtonTextColorIndex = -1; /**< set text color for the button according to index */
        public Action<Transform> GetAnimatedIconCallback = null;
       public Notification()
        {
            IDCounter++; /**< increate counter*/
            ID = IDCounter.ToString(); /**< set id of counter number*/
        }
    }

    /// <summary>
    /// This class handles notifications
    /// </summary>
    public class NotificationController : BaseBehaviour
    {
        [InjectSignal] private SigOnGetNotification mSigOnGetNotification = null;
        [InjectSignal] private SigClearAllNotfiication mSigClearAllNotfiication = null;

        private List<Notification> mNotificationList = new List<Notification>(); /**< list of notification */
        protected Notification mCurrentNotification = null; /**< current notification */
        protected UiNotification mCurrentUiNotification = null; /**< current notification ui */

        /// <summary>
        /// Add Listenrs
        /// </summary>
        public void Start()
        {
            mSigOnGetNotification.AddListener(OnSendNotification);
            mSigClearAllNotfiication.AddListener(Reset);
        }

        /// <summary>
        /// Remove Listeners
        /// </summary>
        protected override void OnDestroy()
        {
            mSigOnGetNotification.RemoveListener(OnSendNotification);
            mSigClearAllNotfiication.RemoveListener(Reset);
            base.OnDestroy();
        }

        /// <summary>
        /// Clear all notification of ids from the list
        /// </summary>
        /// <param name="IDList">type of List<string></param>
        public void ClearNotificationListByID(List<string> IDList)
        {
            //Debug.Log("Clear all notification & count is" + IDList.Count);
            for (int i = 0; i < IDList.Count; i++)
            {
                int index = mNotificationList.FindIndex(a => a.ID.Contains(IDList[i]));
                if (index > -1)
                    mNotificationList.RemoveAt(index);
                mNotificationList.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            }
            if (mCurrentNotification != null)
            {
                if (IDList.Contains(mCurrentNotification.ID))
                {
                    //Debug.Log("clearing current notification");
                    HideNotificationUI(mCurrentNotification.ID);
                }
            }
        }

        /// <summary>
        /// Clear all notification from mNotificationList
        /// </summary>
        private void ClearNotificationList()
        {
            mNotificationList.Clear();
            if (mCurrentNotification != null)
            {
                HideNotificationUI(mCurrentNotification.ID);
            }
        }

        /// <summary>
        /// Send Notification by calling public function directly, done for multitable
        /// </summary>
        /// <param name="n">type of Notification</param>
        /// /// ### Example
        /// @code
        //Notification n = new Notification();
        //n.Type = NotificationUIType.RectangularToast;
        //n.ActionType = NotificationHideAction.Callback;
        //n.Position = NotificationPosition.Default;
        //n.ShowImmediately = true;
        //n.Priority = 3;
        //n.Message = "i am notification";
        //n.Parent = this.transform;
        //NotificationController.SendNotification(n);
        /// @endcode
        /// @see OnSendNotification(Notification)
        public void SendNotification(Notification n)
        {
            OnSendNotification(n);
        }

        /// <summary>
        /// Send Notification by dispatching signal, this function listens to that signal XcelerateGames::SigOnGetNotification
        /// </summary>
        /// <param name="n">type of Notification</param>
        /// /// ### Example
        /// @code
        //Notification n = new Notification();
        //n.Type = NotificationUIType.RectangularToast;
        //n.ActionType = NotificationHideAction.Callback;
        //n.Position = NotificationPosition.Default;
        //n.ShowImmediately = true;
        //n.Priority = 3;
        //n.Message = "i am notification";
        //n.Parent = this.transform;
        //mSigOnGetNotification.Dispatch(n);
        /// @endcode
        /// @see Notification
        private void OnSendNotification(Notification notification)
        {
            if (!CanSendNotification(notification))
                return;

            // Debug.Log("notification id " + notification.ID);
            if (notification.CanClearOtherNotifications)
            {
                ClearNotificationList();
            }
            if (notification.ShowImmediately && mCurrentUiNotification != null && notification.Priority <= mCurrentNotification.Priority)
            {
                notification.Priority = 0; //forcefully set priority high to avoid conflicts
                mCurrentUiNotification.HideImmediately();
            }
            mNotificationList.Add(notification);
            mNotificationList.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            if (mCurrentNotification == null)
            {
                ShowNotificationUI();
            }
        }

        /// <summary>
        /// Check if notification can be shown on screen or not
        /// </summary>
        /// <param name="n">type of Notification</param>
        /// <returns>bool</returns>
        bool CanSendNotification(Notification n)
        {
            if (mCurrentNotification == null)
            {
                XDebug.Log("no notification currently been shown on screen", XDebug.Mask.Game);
                return true;
            }
            else
            {
                string loadedMsg = mCurrentNotification.Message;
                if (n.Message.Length != loadedMsg.Length)
                {
                    XDebug.Log("not same table msg", XDebug.Mask.Game);
                    return true;
                }
                else if (string.CompareOrdinal(n.Message, loadedMsg) != 0)
                {
                    XDebug.Log("not same table msg", XDebug.Mask.Game);
                    return true;
                }
                else
                {
                    mCurrentNotification.ID = n.ID;
                    XDebug.Log("same table msg req", XDebug.Mask.Game);
                    return false;
                }
            }
        }

        /// <summary>
        /// Show notification UI
        /// </summary>
        protected virtual void ShowNotificationUI()
        {
            if (mNotificationList.Count <= 0)
                return;

            mCurrentNotification = mNotificationList[0];
            mNotificationList.RemoveAt(0);
            CreateNotification();

            mCurrentUiNotification.transform.SetParent(mCurrentNotification.Parent);
            mCurrentUiNotification.SetActive(true);
            mCurrentUiNotification.ShowUI(mCurrentNotification, HideNotificationUI, mCurrentNotification.GetAnimatedIconCallback);
        }


        protected virtual void CreateNotification()
        {
            string prefabName = "PfUiNotification";
            GameObject mCurrentNotificationObject = ResourceManager.LoadFromResources(prefabName) as GameObject;
            if (mCurrentNotificationObject == null)
            {
                Debug.LogError("Could not find : " + prefabName + " under resources.");
                return;
            }
            mCurrentUiNotification = Instantiate(mCurrentNotificationObject).GetComponent<UiNotification>();
            if (mCurrentUiNotification == null)
                return;
        }

        /// <summary>
        /// Hide Notification
        /// </summary>
        /// <param name="ID">type of string - Notificationid</param>
        public void HideNotification(string ID)
        {
            HideNotificationUI(ID);
        }

        /// <summary>
        /// Hide Notification UI
        /// </summary>
        /// <param name="ID">type of string - Notificationid</param>
        private void HideNotificationUI(string ID)
        {
            //Debug.Log("clearing current notification " + ID);
            if (mCurrentNotification != null && mCurrentNotification.ID == ID)
            {
                mCurrentNotification = null;
                Destroy(mCurrentUiNotification.gameObject);
                mCurrentUiNotification = null;
                mCurrentUiNotification = null;

                if (mNotificationList.Count > 0 && mCurrentUiNotification == null)
                {
                    ShowNotificationUI();
                }
            }
        }

        /// <summary>
        /// Listens to XcelerateGames::SigClearAllNotfiication
        /// ClearNotificationList 
        /// </summary>
        public void Reset()
        {
            ClearNotificationList();
        }
    }
}
