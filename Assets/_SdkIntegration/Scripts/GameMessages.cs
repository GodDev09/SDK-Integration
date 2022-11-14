//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------
namespace ATSoft
{
    public class GameMessages
    {
        public class AppOpenAdMessage
        {
            public AppOpenAdState AppOpenAdState = AppOpenAdState.NONE;
        }

        public class FirebaseMessage
        {
            public FirebaseAction FirebaseMessageAction = FirebaseAction.None;
        }
        
        public class InternetMessage
        {
            public bool isConnected = true;
        }
    }
}