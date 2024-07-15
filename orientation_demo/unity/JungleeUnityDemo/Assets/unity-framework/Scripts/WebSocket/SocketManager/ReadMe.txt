To use websocket add Native web socket into project
	1) Add package https://github.com/endel/NativeWebSocket.git#upm
	2) Add compiler flag under Assets/csc.rsp  -define:USE_NATIVE_WEBSOCKET

To use websocketsharp
	1) Download & Import package from https://drive.google.com/file/d/1ce1rAlbWzo28zYbT5Q38wIPifghfN5NM/view?usp=sharing
	2) Add compiler flag under Assets/csc.rsp  -define:USE_WEBSOCKET_SHARP

Setup:
	1) Add NativeWebSocketsManager or WebSocketSharpManager to the scene based on which plugin you are using.
        As of now, we use WebSocketSharpManager in all games
	2) Add script that derives from MessageProcessorBase to the scene.
    3) Add script that derives from SocketManagerBase to the scene
    