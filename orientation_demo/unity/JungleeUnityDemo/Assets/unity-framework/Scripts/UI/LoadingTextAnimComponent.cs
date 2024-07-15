using TMPro;
using UnityEngine;

namespace XcelerateGames.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LoadingTextAnimComponent : MonoBehaviour
    {
        [SerializeField] private string _Text = ".";
        [SerializeField] private int _MaxLength = 3;
        [SerializeField] private float _AmimSpeed = 0.4f;
        [SerializeField] private bool _KeepCentered = false;


        private float mElapsedTime = 0f;
        private TextMeshProUGUI mTextMeshProUGUI;
        private string mBaseText;
        private string mGeneratedString = ".";

        void Start()
        {
            mTextMeshProUGUI = GetComponent<TextMeshProUGUI>();
            mBaseText = mTextMeshProUGUI.text;
        }

        void Update()
        {
            mElapsedTime += Time.deltaTime;
            if (mElapsedTime >= _AmimSpeed)
            {
                mElapsedTime = 0f;

                if (_KeepCentered == false)
                {
                    mGeneratedString += _Text;
                    if (mGeneratedString.Length > _MaxLength)
                        mGeneratedString = string.Empty;
                }
                else
                {
                    mGeneratedString = mGeneratedString.Replace("<alpha=#00>" + _Text, " ");
                    int lastIndex = mGeneratedString.LastIndexOf(_Text);

                    mGeneratedString = "";
                    for (int i = 0; i < _MaxLength; i++)
                    {
                        if (lastIndex == _MaxLength - 1)
                        {
                            if (i == 0)
                            {
                                mGeneratedString += _Text;
                            }
                            else
                            {
                                mGeneratedString += ("<alpha=#00>" + _Text);
                            }
                        }
                        else
                        {
                            if (i <= lastIndex + 1)
                            {
                                mGeneratedString += _Text;
                            }
                            else
                            {
                                mGeneratedString += ("<alpha=#00>" + _Text);
                            }
                        }
                    }
                }

                mTextMeshProUGUI.SetText(mBaseText + mGeneratedString);
            }
        }

        public void KeepCentered()
        {
            _KeepCentered = true;
        }
    }
}
