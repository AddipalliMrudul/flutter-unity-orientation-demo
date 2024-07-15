//using UnityEditor;
//using XcelerateGames.WebServices;

//namespace XcelerateGames.Editor.WebRequestSimulator
//{
//    public class WebRequestSimulator
//    {
//        const string SimulateNetworkFailMenu = Utilities.MenuName + "WebRequest/Simulate Network Fail/";
//        const string SimulateNetworkDelayMenu = Utilities.MenuName + "WebRequest/Simulate Network Delay";

//        const string Zero = SimulateNetworkFailMenu + "0%";
//        const string TwentyFive = SimulateNetworkFailMenu + "25%";
//        const string Fifty = SimulateNetworkFailMenu + "50%";
//        const string SeventyFive = SimulateNetworkFailMenu + "75%";
//        const string Hundred = SimulateNetworkFailMenu + "100%";

//        #region Network Fail
//        [MenuItem(Zero, false, 51)]
//        public static void ToggleZeroNetworkFail()
//        {
//            WebRequestV2.pSimulateNetworkFail = 0f;
//        }

//        [MenuItem(Zero, true, 51)]
//        public static bool ToggleZeroNetworkFailValidate()
//        {
//            Menu.SetChecked(Zero, WebRequestV2.pSimulateNetworkFail == 0f);
//            return true;
//        }

//        [MenuItem(TwentyFive, false, 51)]
//        public static void ToggleTwentyFiveNetworkFail()
//        {
//            WebRequestV2.pSimulateNetworkFail = 25f;
//        }

//        [MenuItem(TwentyFive, true, 51)]
//        public static bool ToggleTwentyFiveNetworkFailValidate()
//        {
//            Menu.SetChecked(TwentyFive, WebRequestV2.pSimulateNetworkFail == 25f);
//            return true;
//        }

//        [MenuItem(Fifty, false, 51)]
//        public static void ToggleFiftyNetworkFail()
//        {
//            WebRequestV2.pSimulateNetworkFail = 50;
//        }

//        [MenuItem(Fifty, true, 51)]
//        public static bool ToggleFiftyNetworkFailValidate()
//        {
//            Menu.SetChecked(Fifty, WebRequestV2.pSimulateNetworkFail == 50f);
//            return true;
//        }


//        [MenuItem(SeventyFive, false, 51)]
//        public static void ToggleSeventyFiveNetworkFail()
//        {
//            WebRequestV2.pSimulateNetworkFail = 75f;
//        }

//        [MenuItem(SeventyFive, true, 51)]
//        public static bool ToggleSeventyFiveNetworkFailValidate()
//        {
//            Menu.SetChecked(SeventyFive, WebRequestV2.pSimulateNetworkFail == 75f);
//            return true;
//        }

//        [MenuItem(Hundred, false, 51)]
//        public static void ToggleHundredNetworkFail()
//        {
//            WebRequestV2.pSimulateNetworkFail = 100;
//        }

//        [MenuItem(Hundred, true, 51)]
//        public static bool ToggleHundredNetworkFailValidate()
//        {
//            Menu.SetChecked(Hundred, WebRequestV2.pSimulateNetworkFail == 100f);
//            return true;
//        }
//        #endregion Network Fail

//        #region Network Delay
//        [MenuItem(SimulateNetworkDelayMenu, false, 51)]
//        public static void ToggleSimulateLoadingDelay()
//        {
//            WebRequestV2.pSimulateNetworkDelay = !WebRequestV2.pSimulateNetworkDelay;
//        }

//        [MenuItem(SimulateNetworkDelayMenu, true, 51)]
//        public static bool ToggleSimulateLoadingDelayValidate()
//        {
//            Menu.SetChecked(SimulateNetworkDelayMenu, WebRequestV2.pSimulateNetworkDelay);
//            return true;
//        }
//        #endregion Network Delay
//    }
//}