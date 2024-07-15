namespace XcelerateGames.SpinWheel
{
    /// <summary>
    /// Plain c# class used for calculating wheel stop angle so that unit tests can we run easily on this class
    /// </summary>
    public class SpinWheelPlain : ISpin
    {
        /// <summary>
        /// Returns wheel stop angle when spoke index at which to stop wheel, total num of rewards
        /// and wheel rotation time is provided
        /// </summary>
        /// <param name="rewardIndex">int</param>
        /// <param name="totalNumOfRewards">int</param>
        /// <param name="rotationTime">int</param>
        /// <returns>wheel stop angle</returns>
        public int GetWheelStopAngle(int rewardIndex, int totalNumOfRewards, int rotationTime)
        {
            int anglePerSpoke = 360 / totalNumOfRewards;
            int angle = 360 * rotationTime + ((totalNumOfRewards - rewardIndex) * anglePerSpoke);
            return angle;
        }
    }
}
