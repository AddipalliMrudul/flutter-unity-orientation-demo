using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Tutorials
{
    public class TutorialModel : XGModel
    {
        private List<string> mTutorials = null;
        public bool _Skip = false;
        public TutorialData _CurrentTutorialData = null;

        private readonly string mKey = "jg-tuts";

        public bool IsTutorialPlaying { get { return _CurrentTutorialData; }}

        public TutorialModel()
        {
            if (PlayerPrefs.HasKey(mKey))
            {
                mTutorials = PlayerPrefs.GetString(mKey).FromJson<List<string>>();
                XDebug.Log("Total tutorials complete so far : " + mTutorials.Count, XDebug.Mask.Tutorials);
            }
            else
                mTutorials = new List<string>();
        }

        public bool IsTutorialComplete(string tutorialName)
        {
            if (_Skip)
                return true;
            bool result = mTutorials.Contains(tutorialName);
            XDebug.Log("IsTutorialComplete : " + tutorialName + " : " + result, XDebug.Mask.Tutorials);
            return result;
        }

        public void MarkTutorialComplete(string tutorialName)
        {
            if (!mTutorials.Contains(tutorialName))
            {
                XDebug.Log("Marking " + tutorialName + " as completed.", XDebug.Mask.Tutorials);

                mTutorials.Add(tutorialName);
                PlayerPrefs.SetString(mKey, mTutorials.ToJson());
                PlayerPrefs.Save();
                XDebug.Log("Tutorials status saved, complete so far : " + mTutorials.Count, XDebug.Mask.Tutorials);
            }
        }

        //Used for debugging & testing purpose only
        public void UnMarkTutorialComplete(string tutorialName)
        {
            if (mTutorials.Contains(tutorialName))
            {
                XDebug.Log("UnMarking " + tutorialName + " as completed.", XDebug.Mask.Tutorials);

                mTutorials.Remove(tutorialName);
                PlayerPrefs.SetString(mKey, mTutorials.ToJson());
                PlayerPrefs.Save();
                XDebug.Log("Tutorials status saved, complete so far : " + mTutorials.Count, XDebug.Mask.Tutorials);
            }
        }

        //Used for debugging & testing purpose only
        public void ClearAll()
        {
            XDebug.Log("Clearing tutorial status. " + mTutorials.Count + " completed so far.", XDebug.Mask.Tutorials);
            mTutorials.Clear();
            PlayerPrefs.DeleteKey(mKey);
            PlayerPrefs.Save();
        }

        //Used for debugging & testing purpose only
        //public string GetStatus()
        //{
        //    return mTutorials.ToCSV();
        //}
    }
}
