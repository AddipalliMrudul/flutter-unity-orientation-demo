using System;

namespace XcelerateGames
{
    //I had to name this class as XGTime, else it will conflict with UnityEngine Time class.
    //Adding namespace everywhere would make code less readable
    public struct XGTime
    {
        private byte mHours;
        private byte mMinutes;
        private byte mSeconds;

        public byte Hours
        {
            get { return mHours; }
            set
            {
                if (value > 23)
                    throw new ArgumentException($"Inavlid hours passed: {value}");
                mHours = value;
            }
        }

        public byte Minutes
        {
            get { return mMinutes; }
            set
            {
                if (value > 59)
                    throw new ArgumentException($"Inavlid minutes passed: {value}");
                mMinutes = value;
            }
        }

        public byte Seconds
        {
            get { return mSeconds; }
            set
            {
                if (value > 59)
                    throw new ArgumentException($"Inavlid seconds passed: {value}");
                mSeconds = value;
            }
        }

        public XGTime(byte hours, byte minutes, byte seconds)
        {
            mHours = hours;
            mMinutes = minutes;
            mSeconds = seconds;
        }

        public XGTime(string time, string format = "hh:mm:ss")
        {
            mHours = mMinutes = mSeconds = 0;

            int index = format.IndexOf("hh", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                if (!byte.TryParse(time.Substring(index, 2), out mHours))
                    throw new ArgumentException($"Could not parse hours from {time} with format {format}");
            }

            index = format.IndexOf("mm", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                if (!byte.TryParse(time.Substring(index, 2), out mMinutes))
                    throw new ArgumentException($"Could not parse minutes from {time} with format {format}");
            }

            index = format.IndexOf("ss", StringComparison.OrdinalIgnoreCase);
            if (index >= 0 && index < time.Length)
            {
                if (!byte.TryParse(time.Substring(index, 2), out mSeconds))
                    throw new ArgumentException($"Could not parse seconds from {time} with format {format}");
            }
        }

        public override string ToString()
        {
            return string.Format("{0:00}:{1:00}:{2:00}", mHours, mMinutes, mSeconds);
        }

        public string ToString(string format)
        {
            throw new NotImplementedException("ToString");
            //return $"{mHours}:{mMinutes}:{mSeconds}";
        }

        public string ToStringTemp()
        {
            return string.Format("{0:00}:{1:00}", mHours, mMinutes);
        }

        public string ToString12HourFormat()
        {
            string meridian = "AM";
            if (mHours > 12)
            {
                meridian = "PM";
                mHours -= 12;
            }
            return string.Format("{0:00}:{1:00} {2}", mHours, mMinutes, meridian);
        }

        public DateTime ToDateTime()
        {
            DateTime dateTimeNow = DateTime.Now;
            DateTime dateTime = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, mHours, mMinutes, 0);
            return dateTime;
        }

        public DateTime ToDateTime(DateTime inDateTime)
        {
            DateTime dateTime = new DateTime(inDateTime.Year, inDateTime.Month, inDateTime.Day, mHours, mMinutes, 0);
            return dateTime;
        }
    }
}
