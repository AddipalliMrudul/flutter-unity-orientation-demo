#if FB_ENABLED
using System;
using UnityEngine;
using XcelerateGames.UI;

namespace XcelerateGames.Social.Facebook
{
    public class FacebookUser : UiItemData, IComparable<FacebookUser>
    {
        public string ID;
        public string FirstName;
        public string LastName;
        public string EMail;
        public string UrlToPicture;
        public string Gender;
        public string BirthDate;
        public int YearOfBirth;
        public string Score = "loading...";
        public Texture Picture = null;
        public bool isSelected = false;

        public int CompareTo(FacebookUser other)
        {
            //Compare by Score. More - first
            // If other is not a valid object reference, this instance is greater.
            if (other == null)
                return 1;
            else
            {
                int thisScore = 0;
                int otherScore = 0;
                if (int.TryParse(this.Score, out thisScore) && int.TryParse(other.Score, out otherScore))
                    return thisScore < otherScore ? 1 : (thisScore > otherScore ? -1 : 0);
                else
                    return 1;
            }
        }

        public override string ToString()
        {
            string data = string.Format("Name : {0}, email : {1}, ID : {2}", FirstName, EMail, ID);
            return data;
        }

        public FacebookUser()
        {

        }

        public FacebookUser(FBUserData fBUserData)
        {
            ID = fBUserData.id;
            FirstName = fBUserData.first_name;
            LastName = fBUserData.last_name;
            UrlToPicture = fBUserData.picture.data.url;
        }
    }
 }
#endif //FB_ENABLED
