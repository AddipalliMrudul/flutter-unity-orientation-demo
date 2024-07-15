using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames
{
	public class ShowTouches : MonoBehaviour
	{
		[SerializeField] public TouchItem _Template = null;
		[SerializeField] private float _WaitTime = 5f;
		private List<TouchItem> mTouches = null;
		private bool mShowTouches = false;
		private float mElapsedTime = 0f;

		void Start()
		{
#if DEV_BUILD || QA_BUILD
            mTouches = new List<TouchItem>();
            for (int i = 0; i < 5; ++i)
            {
                mTouches.Add(Utilities.Instantiate<TouchItem>(_Template.gameObject, "TouchItem" + i, transform));
            }
            DontDestroyOnLoad(gameObject);
#else
            Destroy(gameObject);
#endif
        }

        void Update()
		{
            if (Input.touchCount == 3 || (Application.isEditor && Input.GetMouseButton(2)))
            {
                mElapsedTime += Time.deltaTime;
                if (mElapsedTime >= _WaitTime)
                {
                    mShowTouches = !mShowTouches;
                    mElapsedTime = 0;
                    Debug.Log($"Show touch : {mShowTouches}");
                }
            }

            if (mShowTouches)
			{
#if UNITY_EDITOR
            for (int i = 0; i < 3; ++i)
            {
                if (Input.GetMouseButtonDown(i))
                    ShowTouch(i);
            }
#else
				for (int i = 0; i < Input.touchCount; ++i)
				{
					if (Input.touches[i].phase == TouchPhase.Began)
					{
						ShowTouch(i);
					}
				}
#endif
			}
		}

		private void ShowTouch(int index)
		{
#if UNITY_EDITOR
            mTouches[index].transform.position = Input.mousePosition;
#else
			mTouches[index].transform.position = Input.touches[index].position;
#endif
			mTouches[index].Show();
		}
	}
}
