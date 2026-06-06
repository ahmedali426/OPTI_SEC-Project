import 'dart:async';
import 'dart:convert';
import 'package:flutter_background_service/flutter_background_service.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'package:optisecapp/services/local_notifications_service.dart';

Future<void> initializeBackgroundService() async {
  final service = FlutterBackgroundService();

  await service.configure(
    androidConfiguration: AndroidConfiguration(
      onStart: onStartPollingService,
      autoStart: false,
      isForegroundMode: true,
      notificationChannelId: 'optisec_channel_v5', // تم التحديث ليتوافق مع أحدث تغيير
      initialNotificationTitle: 'Opti-Sec Gate Security Active',
      initialNotificationContent: 'Monitoring gate security layers...',
    ),
    iosConfiguration: IosConfiguration(
      autoStart: false,
      onForeground: onStartPollingService,
    ),
  );
}

void startNotificationPolling() async {
  final service = FlutterBackgroundService();
  bool isRunning = await service.isRunning();

  if (!isRunning) {
    await service.startService();
    print("🔄 Opti-Sec Foreground Service & Polling Pipeline Started...");
  }
}

void stopNotificationPolling() async {
  final service = FlutterBackgroundService();
  bool isRunning = await service.isRunning();

  if (isRunning) {
    service.invoke("stopService");
    print("🛑 Opti-Sec Foreground Service & Polling Pipeline Stopped.");
  }
}

@pragma('vm:entry-point')
void onStartPollingService(ServiceInstance service) async {
  print("🚀 Background Isolate Initialized for Polling.");

  String baseUrl = "https://opti-sec.runasp.net";
  int? lastNotificationId;

  service.on("stopService").listen((event) {
    service.stopSelf();
    print("🛑 Background Isolate Stopped via Main App.");
  });

  Timer.periodic(const Duration(seconds: 3), (timer) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.reload();
      final currentToken = prefs.getString('auth_token');

      if (currentToken == null || currentToken.isEmpty) {
        print("⚠️ Background Polling skipped: Token is null or empty.");
        return;
      }

      final response = await http.get(
        Uri.parse('$baseUrl/api/MobileCommands/notifications'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $currentToken',
        },
      );

      if (response.statusCode == 200) {
        List<dynamic> notifications = jsonDecode(response.body);

        // الحماية الأساسية: التأكد أولاً أن القائمة تحتوي على عناصر بالفعل
        if (notifications.isNotEmpty) {
          List<String> dismissedList = prefs.getStringList('dismissed_emergencies') ?? [];

          // تعديل السطر المسبب للـ Bad state: No element واستبداله بـ الشرط الآمن ده
          lastNotificationId ??= (notifications.isNotEmpty ? notifications.first['id'] : 0) - 1;

          int maxIdInResponse = lastNotificationId!;

          for (var notification in notifications.reversed) {
            int currentId = notification['id'];
            if (currentId > maxIdInResponse) maxIdInResponse = currentId;

            if (dismissedList.contains(currentId.toString())) continue;

            if (currentId > lastNotificationId!) {
              String type = notification['type'] ?? '';
              String title = notification['title'] ?? 'Security Alert';
              String body = notification['body'] ?? '';
              String? imageUrl = notification['imageUrl'] ?? notification['image'];

              print(
                "🔔 Background Polling Detected New Notification! ID: $currentId, Type: $type",
              );

              Map<String, dynamic> localData = {
                'id': currentId,
                'type': type,
                'title': title,
                'body': body,
                'gateId': notification['gateId']?.toString() ?? '0', // حماية إضافية لو الـ gateId جاي null
                'imageUrl': imageUrl,
              };

              if (type == 'SilentAlarm') {
                dismissedList.add(currentId.toString());
                await prefs.setStringList('dismissed_emergencies', dismissedList);

                await LocalNotificationService.showFlexibleNotification(
                  id: currentId,
                  title: title,
                  body: body.isNotEmpty ? body : "Attempted entry detected at the gate!",
                  imageUrl: imageUrl,
                  payload: jsonEncode(localData),
                  isEmergency: true, 
                );
              } else {
                await LocalNotificationService.showFlexibleNotification(
                  id: currentId,
                  title: title,
                  body: body,
                  imageUrl: imageUrl,
                  payload: jsonEncode(localData),
                  isEmergency: false,
                );
              }
            }
          }
          lastNotificationId = maxIdInResponse;
        }
      }
    } catch (e) {
      print("⚠️ Background Polling Error inside loop: $e");
    }
  });
}