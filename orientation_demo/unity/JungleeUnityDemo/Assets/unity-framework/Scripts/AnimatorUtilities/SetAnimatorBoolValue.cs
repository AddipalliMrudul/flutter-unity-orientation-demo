using UnityEngine;

namespace XcelerateGames.AnimatorUtils
{
    public class SetAnimatorBoolValue : SetAnimatorParamsBase
    {
        [SerializeField] private bool _Value = false;

        protected override void SetValue()
        {
            base.SetValue();
            mAnimator.SetBool(_Name, _Value);
        }
    }
}
