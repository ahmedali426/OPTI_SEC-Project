import 'dart:async';
import 'dart:io' show Platform;

import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';
import 'package:optisecapp/main.dart';
import 'package:optisecapp/services/local_notifications_service.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:shared_preferences/shared_preferences.dart';

bool isRegisterd = false;

class FirebaseNotifications {
  final messaging = FirebaseMessaging.instance;

  Future<void> init() async {
    await messaging.requestPermission();
    String? token = await messaging.getToken();
    print('Firebase Token: $token');

    if (token != null) {
      sendToken(token);
    }

    messaging.onTokenRefresh.listen((value) {
      sendToken(value);
    });

    RemoteMessage? initialMessage = await FirebaseMessaging.instance
        .getInitialMessage();
    if (initialMessage != null) {
      final String? notificationType = initialMessage.data['type'];
      if (notificationType == 'Emergency' ||
          notificationType == 'SilentAlarm') {
        Future.delayed(const Duration(milliseconds: 1000), () {
          openEmergencyScreen(initialMessage.data);
        });
      }
    }

    // تسجيل الـ Handler اللي متأمن بالـ @pragma من برة الكلاس
    FirebaseMessaging.onBackgroundMessage(handleBackgroundMessaging);
    handleForegroundMessaging();
    handleNotificationClick();
  }

  void handleForegroundMessaging() {
    FirebaseMessaging.onMessage.listen((RemoteMessage message) {
      print("🔔 Foreground message received: ${message.data}");
      String? notificationType = message.data['type'];

      LocalNotificationService.handleIncomingMessage(message);

      if (notificationType == 'SilentAlarm') {
        openEmergencyScreen(message.data);
      }
    });
  }

  Future<void> sendToken(String token) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      String? userId = prefs.getString('userId');
      String? authtoken = prefs.getString('auth_token');
      if (authtoken != null && userId != null) {
        final result = await ApiService().addFCMToken(
          token: authtoken,
          data: {
            'fcmToken': token,
            'platform': Platform.isAndroid ? 'android' : 'ios',
          },
        );
        if (result) {
          isRegisterd = true;
          print('FCM token is registerd for userId:$userId');
        }
      }
    } catch (e) {
      print('error during uploading FCM token');
    }
  }

  Future<void> sendTokenAfterLogin() async {
    try {
      String? token = await messaging.getToken();
      if (token != null) {
        print(
          'Fetching and sending FCM token immediately after successful login...',
        );
        await sendToken(token);
      } else {
        print('FCM token is null after login.');
      }
    } catch (e) {
      print('Error in sendTokenAfterLogin: $e');
    }
  }

  void handleNotificationClick() {
    FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
      print("🔔 Notification clicked and opened app: ${message.data}");
      final String? notificationType = message.data['type'];

      if (notificationType == 'SilentAlarm') {
        openEmergencyScreen(message.data);
      }
    });
  }

  Future<void> checkAndRequestPermissions(BuildContext context) async {
    if (await Permission.notification.isDenied) {
      await Permission.notification.request();
    }
    bool isSystemAlertGranted = await Permission.systemAlertWindow.isGranted;

    final prefs = await SharedPreferences.getInstance();
    bool isSirenGrantedBefore = prefs.getBool('isSirenGranted') ?? false;

    if (!isSystemAlertGranted) {
      if (context.mounted) showDisplayOverAppsDialog(context);
    } else if (!isSirenGrantedBefore) {
      if (context.mounted) showWriteSettingsDialog(context);
    }
  }
}

void showDisplayOverAppsDialog(BuildContext context) {
  showDialog(
    context: context,
    barrierDismissible: false,
    builder: (context) => AlertDialog(
      title: const Row(
        children: [
          Icon(Icons.layers, color: Colors.blueAccent),
          SizedBox(width: 10),
          Text(
            'Security Permission 1',
            style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
          ),
        ],
      ),
      content: const Text(
        'Opti-Sec requires "Display over other apps" permission to instantly open the alarm screen during a security breach.',
        style: TextStyle(fontSize: 13),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Later', style: TextStyle(color: Colors.grey)),
        ),
        ElevatedButton(
          style: ElevatedButton.styleFrom(backgroundColor: Constants.mainColor),
          onPressed: () async {
            if (context.mounted) {
              Navigator.pop(context);
            }
            await Permission.systemAlertWindow.request();

            Future.delayed(const Duration(seconds: 1), () {
              final newContext = navigatorKey.currentContext;
              if (newContext != null) {
                showWriteSettingsDialog(newContext);
              }
            });
          },
          child: const Text('Enable', style: TextStyle(color: Colors.white)),
        ),
      ],
    ),
  );
}

void showWriteSettingsDialog(BuildContext context) {
  showDialog(
    context: context,
    barrierDismissible: false,
    builder: (context) => AlertDialog(
      title: Row(
        children: [
          Icon(Icons.volume_up, color: Constants.mainColor),
          const SizedBox(width: 10),
          const Text(
            'Security Permission 2',
            style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
          ),
        ],
      ),
      content: const Text(
        'Opti-Sec requires "Modify system settings" permission to trigger the continuous emergency siren alert sound.',
        style: TextStyle(fontSize: 13),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Later', style: TextStyle(color: Colors.grey)),
        ),
        ElevatedButton(
          style: ElevatedButton.styleFrom(backgroundColor: Constants.mainColor),
          onPressed: () async {
            Navigator.pop(context);
            final prefs = await SharedPreferences.getInstance();
            await prefs.setBool('isSirenGranted', true);
            try {
              const platform = MethodChannel('com.example.optisecapp/settings');
              await platform.invokeMethod('openWriteSettings');
            } catch (e) {
              print("Error opening write settings: $e");
            }
          },
          child: const Text('Enable', style: TextStyle(color: Colors.white)),
        ),
      ],
    ),
  );
}

@pragma('vm:entry-point')
Future<void> handleBackgroundMessaging(RemoteMessage message) async {
  await Firebase.initializeApp();
  print("🔔 Background message received: ${message.data}");
  LocalNotificationService.handleIncomingMessage(message);
}

@pragma('vm:entry-point')
Future<void> openEmergencyScreen(Map<String, dynamic> data) async {
  if (data.isNotEmpty) {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (navigatorKey.currentState != null) {
        navigatorKey.currentState?.pushNamed(
          '/emergency_screen',
          arguments: data,
        );
      } else {
        print("⚠️ Navigator is not ready to open emergency screen.");
      }
    });
  }
}

Future<bool> isDeviceLocked() async {
  const platform = MethodChannel('com.example.optisecapp/device_state');
  try {
    final bool isLocked = await platform.invokeMethod('isLocked');
    return isLocked;
  } on PlatformException catch (e) {
    print("Failed to get lock state: '${e.message}'.");
    return false;
  }
}
