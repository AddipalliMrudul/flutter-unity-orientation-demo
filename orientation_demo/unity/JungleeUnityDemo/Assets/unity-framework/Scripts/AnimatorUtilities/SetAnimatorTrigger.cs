namespace XcelerateGames.AnimatorUtils
{
    public class SetAnimatorTrigger : SetAnimatorParamsBase
    {
        protected override void SetValue()
        {
            base.SetValue();
            mAnimator.SetTrigger(_Name);
        }
    }
}
