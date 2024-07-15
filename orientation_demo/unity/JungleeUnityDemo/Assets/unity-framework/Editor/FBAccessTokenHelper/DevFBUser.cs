#if FB_ENABLED
/*
 * Author : Altaf
 * Date : March 23, 2017
 * Purpose : Helper script to apply FB access tokens.
 * Usage : In the text file FBAccessTokens.txt, put all user names & token separated by colon(:), ex : Altaf:ghjdfsghjhgshghjfghfdgh.
 * The token expire every 4 hours, Its better to use extended tokens, These token stay alive for a month.
*/

namespace XcelerateGames.Editor.FBHelper
{
    public class DevFBUser
    {
        public int Id;
        public string Name;
        public string Token;

        private static int Count = 1;

        public DevFBUser(DevFBUser user)
        {
            Id = user.Id;
            Name = user.Name;
            Token = user.Token;
        }

        public DevFBUser()
        {
            Id = Count++;
        }
    }
}
#endif //FB_ENABLED
