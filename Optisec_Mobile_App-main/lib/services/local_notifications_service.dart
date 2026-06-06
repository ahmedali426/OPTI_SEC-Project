import 'dart:async';
import 'dart:convert';
import 'dart:typed_data';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:http/http.dart' as http;
import 'package:optisecapp/main.dart';

class LocalNotificationService {
  static FlutterLocalNotificationsPlugin flutterLocalNotificationsPlugin =
      FlutterLocalNotificationsPlugin();

  static StreamController<NotificationResponse> streamController =
      StreamController<NotificationResponse>.broadcast();

  // تغيير الـ ID إلى v3 لضمان تصفير الكاش وقراءة الإعدادات الجديدة فوراً
  static const String emergencyChannelId = 'emergency_channel_fixed_v3';
  static const String normalChannelId = 'optisec_channel_v5';

  static void onTap(NotificationResponse response) {
    streamController.add(response);
    _handleNavigation(response.payload);
  }

  static void _handleNavigation(String? payload) async {
    if (payload == null) return;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      try {
        Map<String, dynamic> data = jsonDecode(payload);
        if (data['type'] == 'SilentAlarm') {
          navigatorKey.currentState?.pushNamedAndRemoveUntil(
            '/emergency_screen',
            (route) => false,
            arguments: data,
          );
        }
      } catch (e) {
        print("Error in navigation: $e");
      }
    });
  }

  static Future init() async {
    InitializationSettings settings = const InitializationSettings(
      android: AndroidInitializationSettings('@drawable/ic_notification'),
      iOS: DarwinInitializationSettings(),
    );
    
    await flutterLocalNotificationsPlugin.initialize(
      settings,
      onDidReceiveNotificationResponse: onTap,
      onDidReceiveBackgroundNotificationResponse: onTap,
    );
    
    final androidPlugin = flutterLocalNotificationsPlugin
        .resolvePlatformSpecificImplementation<AndroidFlutterLocalNotificationsPlugin>();

    if (androidPlugin != null) {
      // إعداد قناة الطوارئ كـ Alarm رسمي من النظام (وهذا يجبر الصوت على التكرار تلقائياً في أندرويد)
      const AndroidNotificationChannel emergencyChannel = AndroidNotificationChannel(
        emergencyChannelId,
        'Emergency Alerts',
        description: 'Critical alerts for wrong password attempts and gate security.',
        importance: Importance.max,
        playSound: true,
        sound: RawResourceAndroidNotificationSound('emergency_sound'),
        enableVibration: true,
      );
      await androidPlugin.createNotificationChannel(emergencyChannel);

      const AndroidNotificationChannel normalChannel = AndroidNotificationChannel(
        normalChannelId,
        'Opti-Sec Notifications',
        description: 'General notifications for system status.',
        importance: Importance.high,
        playSound: true,
        enableVibration: true,
      );
      await androidPlugin.createNotificationChannel(normalChannel);
    }
  }

  static Future<void> showFlexibleNotification({
    required int id,
    required String title,
    required String body,
    required String? imageUrl,
    String? payload,
    bool isEmergency = false,
  }) async {
    AndroidNotificationDetails androidDetails;

    String channelId = isEmergency ? emergencyChannelId : normalChannelId;
    String channelName = isEmergency ? 'Emergency Alerts' : 'Opti-Sec Notifications';

    if (isEmergency) {
      androidDetails = AndroidNotificationDetails(
        channelId,
        channelName,
        importance: Importance.max,
        priority: Priority.high,
        playSound: true,
        sound: const RawResourceAndroidNotificationSound('emergency_sound'),
        ongoing: true,
        autoCancel: false,
        fullScreenIntent: true,
        visibility: NotificationVisibility.public,
        category: AndroidNotificationCategory.alarm,
        audioAttributesUsage: AudioAttributesUsage.alarm, 
      );
    } else {
      androidDetails = AndroidNotificationDetails(
        channelId,
        channelName,
        importance: Importance.max,
        priority: Priority.high,
        playSound: true,
        autoCancel: true,
        ongoing: false,
      );
    }

    if (imageUrl != null && imageUrl.isNotEmpty) {
      try {
        final http.Response response = await http.get(Uri.parse(imageUrl));
        if (response.statusCode == 200) {
          final ByteArrayAndroidBitmap bigPicture = ByteArrayAndroidBitmap(response.bodyBytes);

          androidDetails = AndroidNotificationDetails(
            androidDetails.channelId,
            androidDetails.channelName,
            importance: androidDetails.importance,
            priority: androidDetails.priority,
            playSound: androidDetails.playSound,
            sound: androidDetails.sound,
            ongoing: androidDetails.ongoing,
            autoCancel: androidDetails.autoCancel,
            fullScreenIntent: androidDetails.fullScreenIntent,
            category: androidDetails.category,
            audioAttributesUsage: androidDetails.audioAttributesUsage,
            styleInformation: BigPictureStyleInformation(
              bigPicture,
              largeIcon: bigPicture,
              contentTitle: title,
              summaryText: body,
            ),
          );
        }
      } catch (e) {
        print("⚠️ error in loading image in notification $e");
      }
    }

    final NotificationDetails generalDetails = NotificationDetails(android: androidDetails);

    await flutterLocalNotificationsPlugin.show(
      id,
      title,
      body,
      generalDetails,
      payload: payload,
    );
  }

  static void handleIncomingMessage(RemoteMessage message) {
    final String? notificationType = message.data['type'];
    if (notificationType == 'SilentAlarm') {
      showEmergencyNotification(message);
    } else {
      showBasicNotification(message);
    }
  }

  static void showBasicNotification(RemoteMessage message) async {
    AndroidNotificationDetails androidDetails;
    String title = message.notification?.title ?? message.data['title'] ?? 'Security Alert';
    String body = message.notification?.body ?? message.data['body'] ?? 'Wrong passcode attempt.';

    final imageUrl = message.notification?.android?.imageUrl;
    if (imageUrl != null && imageUrl.isNotEmpty) {
      try {
        final http.Response response = await http.get(Uri.parse(imageUrl));
        if (response.statusCode == 200) {
          BigPictureStyleInformation bigPictureStyleInformation = BigPictureStyleInformation(
            ByteArrayAndroidBitmap.fromBase64String(base64Encode(response.bodyBytes)),
            largeIcon: ByteArrayAndroidBitmap.fromBase64String(base64Encode(response.bodyBytes)),
          );

          androidDetails = AndroidNotificationDetails(
            normalChannelId,
            'Opti-Sec Notifications',
            importance: Importance.max,
            priority: Priority.high,
            styleInformation: bigPictureStyleInformation,
            autoCancel: true,
            ongoing: false,
          );
        } else {
          androidDetails = const AndroidNotificationDetails(normalChannelId, 'Opti-Sec Notifications', importance: Importance.max, priority: Priority.high, autoCancel: true);
        }
      } catch (e) {
        androidDetails = const AndroidNotificationDetails(normalChannelId, 'Opti-Sec Notifications', importance: Importance.max, priority: Priority.high, autoCancel: true);
      }
    } else {
      androidDetails = const AndroidNotificationDetails(normalChannelId, 'Opti-Sec Notifications', importance: Importance.max, priority: Priority.high, autoCancel: true);
    }

    NotificationDetails details = NotificationDetails(android: androidDetails);
    await flutterLocalNotificationsPlugin.show(
      0,
      title,
      body,
      details,
      payload: jsonEncode(message.data),
    );
  }

  static Future<void> showEmergencyNotification(RemoteMessage message) async {
    final AndroidFlutterLocalNotificationsPlugin? androidPlugin =
        flutterLocalNotificationsPlugin.resolvePlatformSpecificImplementation<AndroidFlutterLocalNotificationsPlugin>();

    if (androidPlugin != null) {
      await androidPlugin.requestFullScreenIntentPermission();
    }

    // تم تنظيف الكود تماماً من البارامترز المسببة للإيرورات لتوافق كافة الإصدارات
    // ضبط الـ category والـ audioAttributesUsage كـ alarm هو كافٍ برمجياً لجعل النظام يكرر الصوت
    const AndroidNotificationDetails androidDetails = AndroidNotificationDetails(
      emergencyChannelId,
      'Emergency Alerts',
      importance: Importance.max,
      priority: Priority.high,
      sound: RawResourceAndroidNotificationSound('emergency_sound'),
      playSound: true,
      ongoing: true,
      autoCancel: false,
      enableVibration: true,
      fullScreenIntent: true,
      visibility: NotificationVisibility.public,
      category: AndroidNotificationCategory.alarm,
      audioAttributesUsage: AudioAttributesUsage.alarm,
    );

    const NotificationDetails generalNotificationDetails = NotificationDetails(android: androidDetails);

    String title = message.data['title'] ?? message.notification?.title ?? "SECURITY BREACH WARNING";
    String body = message.data['body'] ?? message.notification?.body ?? "Critical security alert triggered.";

    await flutterLocalNotificationsPlugin.show(
      0,
      title,
      body,
      generalNotificationDetails,
      payload: jsonEncode(message.data),
    );
  }
}