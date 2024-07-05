import 'package:flutter/material.dart';
import 'package:flutter_unity_widget/flutter_unity_widget.dart';

void main() {
  runApp(const MainApp());
}

class MainApp extends StatelessWidget {
  const MainApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      home: Scaffold(
        body: UnityWidget(
          onUnityCreated: (controller) {
            // _provider.setUnityController(controller);
            controller.isPaused()?.then((bool? isPaused) {
              if (isPaused ?? false) {
                controller.resume();
              }
            });
          },
          onUnityMessage: (message) {
            print('Received message from unity: ${message.toString()}');
          },
          useAndroidViewSurface: true,
          onUnitySceneLoaded:(message) => print('Received scene loaded from unity: ${message.toString()}'),
        ),
      ),
    );
  }
}
