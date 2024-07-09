

class InitData {
  final int? playerId;
  final String? userName;
  final String? email;
  final String? authToken;
  final String? latitude;
  final String? longitude;
  final String? userDeviceType;
  final String? apkVersion;
  final String? osVersion;
  final bool? isMultiTableEnabled;
  final int? maxTableLimit;
  final bool? isRichDesign;
  final int? avatarId;
  ///todo: Updating Unity-flutter communication for rumble data[SWAT-438]
  final double? minDeposit;
  final int? vipCategory;
  final String? loyalty;
  final String? rumbleBaseUrl;
  final String? bearerToken;
  final bool isGameWSLogsEnabled;
  final bool isChallengerWidgetAnimEnabled;
  final String? wsReconnWaitTime;
  final String? nickname;
  final String? userType;
  final int? rumbleRefreshIntervalOnLobby;
  final int? rumbleRefreshIntervalOnRumble;
  final String? gatewaybaseUrl;
  InitData({
    this.playerId,
    this.userName,
    this.email,
    this.authToken,
    this.latitude,
    this.longitude,
    this.userDeviceType,
    this.apkVersion,
    this.osVersion,
    this.isMultiTableEnabled,
    this.maxTableLimit,
    this.isRichDesign,
    this.avatarId,
    ///todo: Updating Unity-flutter communication for rumble data[SWAT-438]
    this.minDeposit,
    this.vipCategory,
    this.loyalty,
    this.rumbleBaseUrl,
    this.bearerToken,
    this.isGameWSLogsEnabled = false,
    this.isChallengerWidgetAnimEnabled = false,
    this.wsReconnWaitTime,
    this.nickname,
    this.userType,
    this.rumbleRefreshIntervalOnLobby,
    this.rumbleRefreshIntervalOnRumble,
    this.gatewaybaseUrl,
  });

  factory InitData.fromJson(Map<String, dynamic> json) {
    return InitData(
      playerId: json["playerId"],
      userName: json["userName"],
      email: json["email"],
      authToken: json["authToken"],
      latitude: json["latitude"],
      longitude: json["longitude"],
      userDeviceType: json["userDeviceType"],
      apkVersion: json["apkVersion"],
      osVersion: json["osVersion"],
      isMultiTableEnabled: json["isMultiTableEnabled"],
      maxTableLimit: json["maxTableLimit"],
      isRichDesign: json["isRichDesign"],
      avatarId: json["avatarId"],
      ///todo: Updating Unity-flutter communication for rumble data[SWAT-438]
      bearerToken: json["bearerToken"],
      loyalty: json["loyality"],
      minDeposit: json["minDeposit"],
      rumbleBaseUrl: json["rumbleBaseUrl"],
      vipCategory: json["vipCategory"],
      isGameWSLogsEnabled: json["isGameWSLogsEnabled"],
      isChallengerWidgetAnimEnabled: json["isChallengerWidgetAnimEnabled"],
      wsReconnWaitTime: json["wsReconnWaitTime"],
      nickname: json['nickname'],
      gatewaybaseUrl : json['gatewaybaseUrl'],
    );
  }

  Map<String, dynamic> toJson() => {
    "playerId": this.playerId,
    "userName": this.userName,
    "email": this.email,
    "authToken": this.authToken,
    "latitude": this.latitude,
    "longitude": this.longitude,
    "userDeviceType": this.userDeviceType,
    "apkVersion": this.apkVersion,
    "osVersion": this.osVersion,
    "isMultiTableEnabled": this.isMultiTableEnabled,
    "maxTableLimit": this.maxTableLimit,
    "isRichDesign": this.isRichDesign,
    "avatarId": this.avatarId,
    ///todo: Updating Unity-flutter communication for rumble data[SWAT-438]
    "minDeposit": this.minDeposit,
    "vipCategory": this.vipCategory,
    "loyalty": this.loyalty,
    "rumbleBaseUrl": this.rumbleBaseUrl,
    "bearerToken": this.bearerToken,
    "isGameWSLogsEnabled": this.isGameWSLogsEnabled,
    "isChallengerWidgetAnimEnabled": this.isChallengerWidgetAnimEnabled,
    "wsReconnWaitTime":this.wsReconnWaitTime,
    "nickname" : this.nickname,
    "userType": userType,
    "rumbleRefreshIntervalOnLobby": rumbleRefreshIntervalOnLobby,
    "rumbleRefreshIntervalOnRumble": rumbleRefreshIntervalOnRumble,
    "gatewaybaseUrl":gatewaybaseUrl
  };
}
