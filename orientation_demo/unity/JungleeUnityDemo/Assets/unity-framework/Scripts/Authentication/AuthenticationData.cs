using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.WebServices;

namespace JungleeGames.Authentication
{
    public class WSRequestOTPParams : WSParams
    {
        public string mobile;
        public Context context;

        public WSRequestOTPParams(int channelId, string signature)
        {
            context = new Context(channelId, signature);
        }
    }

    //These values are hard coded intentionally
    public class Context
    {
        public int channelId = 0;
        //Add a new signature in every session. This signature is used to validate the sender
        public string signature = null;

        public Context(int channelId, string signature)
        {
            this.channelId = channelId;
            this.signature = signature;
        }
    }

    public class RequestOTPResponse
    {
        public long userId;
        public bool passwordPresent;
    }

    public class WSPerformOTPSignIn : WSRequestOTPParams
    {
        public int password;

        public WSPerformOTPSignIn(int channelId, string signature) : base(channelId, signature)
        {
        }
    }
}
