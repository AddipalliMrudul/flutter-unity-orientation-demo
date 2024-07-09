import 'package:flutter/material.dart';
import 'package:orientation_demo/game.dart';

void main() {
  runApp(const MainApp());
}

class MainApp extends StatelessWidget {
  const MainApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      home: Scaffold(
        body: Container(
          width: double.infinity,
          height: double.infinity,
          child: Center(
            child: Container(
              height: 100,
              margin: const EdgeInsets.all(20),
              child: ElevatedButton(
                  child: const Text("Open Unity Game"),
                  onPressed: () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => GameUI(),
                      ),
                    );
                  }),
            ),
          ),
        ),
      ),
    );
  }
}
