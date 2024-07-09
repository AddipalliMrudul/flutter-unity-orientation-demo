import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_unity_widget/flutter_unity_widget.dart';

class GameUI extends StatelessWidget {
  GameUI({super.key});
  UnityProvider _provider = UnityProvider();

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      home: Scaffold(
        body: UnityWidget(
          onUnityCreated: (controller) {
            _provider.setUnityController(controller);
          },
          onUnityMessage: (message) {
            _provider.onMessageFromUnity(context, message);
          },
          useAndroidViewSurface: true,
          onUnitySceneLoaded: (message) =>
              print('Received scene loaded from unity: ${message.toString()}'),
        ),
      ),
    );
  }
}

class UnityProvider {
  UnityWidgetController? _controller;

  void setUnityController(UnityWidgetController controller) async {
    print("INFO Setting Unity controller");
    if (_controller != null) return;
    _controller = controller;
    resumeUnity();
    checkIfUnityReady();
    //TODO Enable by Indrajeet after production launch
    // DopamineManager().trigger(TriggerPoint.gt_ready_to_play);
  }

  void resumeUnity() {
    _controller?.isPaused()?.then((bool? isPaused) {
      if (isPaused ?? false) {
        _controller?.resume();
      }
    });
  }

  void checkIfUnityReady() {
    this.sendMessageToUnity({"type": "IsUnityReady"});
  }

  sendMessageToUnity(Map<String, dynamic> message) {
    if (_controller == null) {
      return;
    }

    return _controller?.postMessage(
      'UnityMessageManager',
      'onMessage',
      json.encode(message),
    );
  }

  void onMessageFromUnity(BuildContext context, String message) async {
    Map<String, dynamic>? unityMessage = {};
    try {
      unityMessage = json.decode(message);
      if (unityMessage == null) {
        return;
      }
    } catch (e) {
      return;
    }

    String type = unityMessage["type"].toString();

    print("INFO Message from Unity $type");

    if (type == MessageType.UnityReady.name) {
      this.setAppConfigAndInitUnity();
    }
    if (type == MessageType.GameEnd.name) {
      Navigator.pop(context);
    }
  }

  void setAppConfigAndInitUnity() {
    // InitData gameInitData = InitData(
    //   playerId: _userInfoModel.userId,
    //   userName: _userInfoModel.userName,
    //   email: _userInfoModel.email,
    //   authToken: _userLoginDetails.gsAuthToken,
    //   latitude: _applicationGlobalStateModel.userPosition?.lat.toString() ?? "",
    //   longitude:
    //       _applicationGlobalStateModel.userPosition?.lng.toString() ?? "",
    //   userDeviceType: _applicationGlobalStateModel.appType.typeName,
    //   apkVersion: _applicationGlobalStateModel.appVersion,
    //   osVersion: getOSVersion(),
    //   isMultiTableEnabled: checkMultiTableEnabled(),
    //   maxTableLimit: (_multiTableConfigResponse.multiTableEnabled ?? false)
    //       ? (_multiTableConfigResponse.defaultTableCount ?? 2)
    //       : 1,
    //   isRichDesign: _utilityManager.isRichGameDesignShown(),
    //   avatarId: _userInfoModel.avatarId,

    //   ///todo: Updating Unity-flutter communication for rumble data[SWAT-438]
    //   minDeposit: _userInfoModel.totalDeposits ?? 0.0,
    //   vipCategory: _userInfoModel.vip ?? 0,
    //   loyalty: (_userInfoModel.loyaltyInfo?.segmentTitle ?? "NA").toUpperCase(),
    //   rumbleBaseUrl: "https://${JWRHttpConfigs.rumbleGatewayBaseUrl}/",
    //   bearerToken: _localStore
    //       .getString(JWRConstants.LOCALSTORE_API_ACCESS_TOKEN)
    //       .ifEmpty(() => _userLoginDetails.apiAccessToken),
    //   isGameWSLogsEnabled: this
    //       ._applicationGlobalStateModel
    //       .lobbyABConfigValues
    //       .isGameWSLogsEnabled,
    //   isChallengerWidgetAnimEnabled: this
    //       ._applicationGlobalStateModel
    //       .lobbyABConfigValues
    //       .isChallengerWidgetAnimEnabled,
    //   wsReconnWaitTime: _applicationGlobalStateModel.wsReconnWaitTime,
    //   nickname: _localStore.getNickname() ?? _userInfoModel.userName,
    //   userType: _userInfoModel.userType.name.replaceFirst('USER', ""),
    //   rumbleRefreshIntervalOnLobby:
    //       locator<RegistrationConfigModel>().rumbleRefreshIntervalOnLobby,
    //   rumbleRefreshIntervalOnRumble:
    //       locator<RegistrationConfigModel>().rumbleRefreshIntervalOnRumble,
    //   gatewaybaseUrl: JWRConstants.BASE_HTTP_GATEWAY_URL,
    // );

    // final initDataMap = gameInitData.toJson();
    // final lobbyABConfigValues = locator.globalState.lobbyABConfigValues;
    // initDataMap["isAutoOptInEnabled"] = lobbyABConfigValues.challengerAutoOptInAb;
    // initDataMap["isNewExitFlowEnabled"] = lobbyABConfigValues.isNewExitFlowEnabled;

    // final gameInitConfigs = locator<GameInitConfig>().configs;

    // if (gameInitConfigs != null) {
    //   initDataMap.addEntries(gameInitConfigs);
    // }

    // sendMessageToUnity({
    //   "type": MessageType.Init.name,
    //   "data": json.encode(initDataMap)
    // });

    sendMessageToUnity(
        {"type": MessageType.Init.name, "data": json.encode({})});
  }
}

enum MessageType {
  UnityOpen,
  UnityClose,
  UnityReady,
  RequestDesignInfo,
  Init,
  GameEnd,
  GameStart,
  GameTableFTUEComplete,
  GameTableFTUESkipped,
  AnalyticsEvent,
  ShowUI,
  AddTable,
  GetAvatarList,
  AvatarList,
  UpdatePlayerInfo,
  ScoreBoard,
  NoDetails,
  FillDetails,
  CloseAddTablePopup,
  GetRumbleLBListMsg,
  GetRumbleLBResponseMsg,
  LBResponseOptIn_request,
  RummyRumbleConfig,
  SetRumbleLBResponseMsg,
  SetRumbleLBListMsg,
  LBResponseOptIn_response,
  ClickStream,
  MailLogSS,
  TableIDToRemove,
  QuitTable,
  DesignInfo,
  GetRAPData,
  FrameworkEvent,

  ///todo: Updating Unity-flutter communication for rumble data[SWAT-438]
  // GetUserRumbleV2List,
  // UserRumbleV2List,
  // InitiateUserRumbleOptIn,
  // UserRumbleOptinResponse,
  // GetRumbleRankData,
  // UserMegaRumbleRankDetails,
  // UserMiniRumbleRankDetails,
  // OtherUsersMegaRumbleRankDetails,
  // OtherUsersMiniRumbleRankDetails,
  GameResult,
  PlayTutorial,
  ContactUsQuerySubmitted,
  GetUpdatedBearerToken,
  UpdatedBearerTokenReceived,
  GameTableState,
  InvitePlayer,
  AppResumed,
  AppPaused,
}
