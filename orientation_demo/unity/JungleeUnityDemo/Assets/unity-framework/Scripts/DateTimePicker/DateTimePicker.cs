using System;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.UI
{
    public class DateTimePicker : UiBase
    {
        [Header("Date")]
        public UiItem _Year = null;
        public UiItem _Month = null;
        public UiItem _SelectedDate = null;
        public UiItem _BtnOkay = null;
        public GridLayoutGroup _Grid = null;
        public DateTimePickerItem _DateTemplate = null;
        public Color _SelectedDateColor = Color.red;

        [Header("Time")]
        public Text _Hours = null;
        public Text _Minutes = null;
        public UiItem _AmPm = null;

        private DateTime mDateTime;
        private DateTime mSelectedDateTime;
        private DateTime mStartDateTime;

        private DateTimePickerItem mSelectedItem = null;

        private static Action<DateTime, object> OnDateTimeSelectedEvent = null;
        private static Action<object> OnDateTimeCancelEvent = null;
        private static object mUserData = null;

        #region Private methods

        //For testing purpose only
        //protected override void Start()
        //{
        //    mDateTime = DateTime.Now;
        //    mStartDateTime = mDateTime;
        //    Show();
        //}

        public override void Show()
        {
            base.Show();
            int hours = mDateTime.Hour;
            if (hours > 12)
            {
                hours -= 12;
                _AmPm.SetText("PM");
            }
            _Hours.text = hours.ToString();
            _Minutes.text = mDateTime.Minute.ToString();

            _Month.SetText(mDateTime.GetMonthName());
            _Year.SetText(mDateTime.Year.ToString());
            _SelectedDate.SetText(mDateTime.GetDayName() + ", " + mDateTime.GetAbbreviatedMonthName() + " " + mDateTime.Day);

            DateTime firstDayOfMonth = new DateTime(mDateTime.Year, mDateTime.Month, 1);

            int first = (int)firstDayOfMonth.DayOfWeek;

            _Grid.transform.DestroyChildren();

            //Go back one month
            DateTime lastMonth = mDateTime.AddMonths(-1);

            //Now, calculate from which day of last month we need to begin
            int startDatOfLastMonth = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month) - (first - 1);
            _DateTemplate.gameObject.SetActive(true);
            int daysAdded = 0;
            for (int i = startDatOfLastMonth; i <= DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month); ++i)
            {
                DateTimePickerItem go = Utilities.Instantiate<DateTimePickerItem>(_DateTemplate.gameObject, "Date-" + i, _Grid.transform);
                go._Date.text = i.ToString();
                go._Date.color = Color.gray;
                go._Button.interactable = false;
                daysAdded++;
            }

            //Now populate the days for current month
            int daysinThisMonth = DateTime.DaysInMonth(mDateTime.Year, mDateTime.Month);
            for (int i = 1; i <= daysinThisMonth; ++i)
            {
                DateTimePickerItem go = Utilities.Instantiate<DateTimePickerItem>(_DateTemplate.gameObject, "Date-" + i, _Grid.transform);
                go._Date.text = i.ToString();
                //Set the color to red only for the current day.
                if (mDateTime.Month == mStartDateTime.Month && mDateTime.Year == mStartDateTime.Year && i == mStartDateTime.Day)
                {
                    SetSelected(go);
                }
                daysAdded++;
            }

            //Now populate the days for current month
            for (int i = 1; i <= 42 - daysAdded; ++i)
            {
                DateTimePickerItem go = Utilities.Instantiate<DateTimePickerItem>(_DateTemplate.gameObject, "Date-" + i, _Grid.transform);
                go._Date.text = i.ToString();
                go._Date.color = Color.gray;
                go._Button.interactable = false;
            }

            _DateTemplate.gameObject.SetActive(false);
            _BtnOkay.SetInteractive(false);
        }

        public override void Hide()
        {
            OnDateTimeSelectedEvent = null;
            OnDateTimeCancelEvent = null;
            base.Hide();
        }

        private void SetSelected(DateTimePickerItem item)
        {
            //Reset the previously selcted item(if any)
            if (mSelectedItem != null)
            {
                mSelectedItem._Button.targetGraphic.enabled = false;
                mSelectedItem._Button.targetGraphic.color = Color.white;
            }
            mSelectedItem = item;
            mSelectedItem._Button.targetGraphic.enabled = true;
            mSelectedItem._Button.targetGraphic.color = _SelectedDateColor;
            UpdateTime();
        }

        public void UpdateTime()
        {
            _BtnOkay.SetInteractive(true);

            int day = int.Parse(mSelectedItem._Date.text);
            int hours = int.Parse(_Hours.text);
            int minutes = int.Parse(_Minutes.text);
            if (_AmPm.GetText() == "PM")
                hours += 12;
            if (hours == 24)
                hours = 0;
            mSelectedDateTime = new DateTime(mDateTime.Year, mDateTime.Month, day, hours, minutes, 0);
            Debug.Log(mSelectedDateTime.ToString());
        }

        #endregion Private methods

        #region Public methods

        public void OnClickNextMonth()
        {
            mDateTime = mDateTime.AddMonths(1);
            Show();
        }

        public void OnClickPrevMonth()
        {
            mDateTime = mDateTime.AddMonths(-1);
            Show();
        }

        public void OnClickCancel()
        {
            OnDateTimeCancelEvent?.Invoke(mUserData);
            Hide();
        }

        public void OnClickOk()
        {
            OnDateTimeSelectedEvent?.Invoke(mSelectedDateTime, mUserData);
            Hide();
        }

        public void OnDatePicked(DateTimePickerItem inItem)
        {
            SetSelected(inItem);
        }

        public void OnAddHours(bool add)
        {
            int hours = int.Parse(_Hours.text);
            hours += add ? +1 : -1;
            hours = Mathf.Clamp(hours, 0, 11);
            _Hours.text = hours.ToString();
            UpdateTime();
        }

        public void OnAddMinutes(bool add)
        {
            int minutes = int.Parse(_Minutes.text);
            minutes += add ? +1 : -1;
            minutes = Mathf.Clamp(minutes, 0, 59);
            _Minutes.text = minutes.ToString();
            UpdateTime();
        }

        public void OnChangeMeridiem()
        {
            _AmPm.SetText(_AmPm.GetText() == "AM" ? "PM" : "AM");
            UpdateTime();
        }

        #endregion Public methods

        #region static methods

        public static void ShowUI(DateTime startDate, Action<DateTime, object> selectedCallback = null, Action<object> cancelCallback = null, object userData = null)
        {
            UiLoadingCursor.Show(true);
            OnDateTimeSelectedEvent = selectedCallback;
            OnDateTimeCancelEvent = cancelCallback;
            mUserData = userData;
            ResourceManager.Load("datetimepicker/PfUiDateTimePicker", OnDateTimePickerBundleLoaded, ResourceManager.ResourceType.Object, inUserData: startDate);
        }

        private static void OnDateTimePickerBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.COMPLETE)
            {
                DateTimePicker dateTimePicker = Utilities.Instantiate<DateTimePicker>(inObject, "PfUiDateTimePicker", null);
                DateTime startDate = (DateTime)inUserData;
                dateTimePicker.mDateTime = startDate;
                dateTimePicker.mStartDateTime = startDate;
                dateTimePicker.mSelectedDateTime = startDate;
                dateTimePicker.Show();
                UiLoadingCursor.Show(false);
            }
            else if (inEvent == ResourceEvent.ERROR)
            {
                UiDialogBox.Show("Failed to load : " + inURL, DialogBoxHeaderType.ERROR, DialogBoxType.CLOSE);
                UiLoadingCursor.Show(false);
            }
        }

        #endregion static methods
    }
}