import 'dart:async';
import 'dart:convert';

import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/material.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart'; // تأكدي من وجود هذا الـ import للـ LaunchDetails
import 'package:optisecapp/Admin/provider/admin_provider.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Auth/pages/login_page.dart';
import 'package:optisecapp/Auth/provider/auth_provider.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';
import 'package:optisecapp/auth_wrapper.dart';
import 'package:optisecapp/firebase_options.dart';
import 'package:optisecapp/services/emergency_screen.dart';
import 'package:optisecapp/services/firebase_notifications.dart';
import 'package:optisecapp/services/local_notifications_service.dart';
import 'package:optisecapp/services/notifications.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

final GlobalKey<NavigatorState> navigatorKey = GlobalKey<NavigatorState>();

@pragma('vm:entry-point')
Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  await Firebase.initializeApp();
  print("🚨 Background Message Received: ${message.messageId}");
}

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await Firebase.initializeApp(options: DefaultFirebaseOptions.currentPlatform);
  await FirebaseNotifications().init();
  await LocalNotificationService.init();
  FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);

  await initializeBackgroundService();

  RemoteMessage? initialMessage = await FirebaseMessaging.instance.getInitialMessage();

  final prefs = await SharedPreferences.getInstance();
  final refreshExpirydateStr = prefs.getString('refresh_token_expiry');
  final token = prefs.getString('auth_token');
  bool isSessionValid = false;

  if (refreshExpirydateStr != null && token != null) {
    final refreshExpiry = DateTime.parse(refreshExpirydateStr);
    if (refreshExpiry.isAfter(DateTime.now())) {
      isSessionValid = true;
    }
  }

  final authProvider = AuthProvider();
  Widget initialScreen;
  if (isSessionValid) {
    await authProvider.loadUserData();
    initialScreen = AuthWrapper();
  } else {
    await ApiService().clearSession(prefs);
    initialScreen = const LoginPage();
  }

  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (context) => AdminProvider()),
        ChangeNotifierProvider(create: (context) => UserProvider()),
        ChangeNotifierProvider.value(value: authProvider),
      ],
      child: MyApp(startScreen: initialScreen, initialMessage: initialMessage),
    ),
  );
}

class MyApp extends StatefulWidget {
  final Widget startScreen;
  final RemoteMessage? initialMessage;
  const MyApp({super.key, required this.startScreen, this.initialMessage});
  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> with WidgetsBindingObserver {
  DateTime? appPausedTime;
  late StreamSubscription _notificationSubscription;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _managePollingStatus();

    _notificationSubscription = LocalNotificationService.streamController.stream
        .listen((notificationResponse) {
      if (notificationResponse.payload != null) {
        _checkAndNavigate(notificationResponse.payload!);
      }
    });

    WidgetsBinding.instance.addPostFrameCallback((_) async {
      RemoteMessage? initialMessage = await FirebaseMessaging.instance.getInitialMessage();
      if (initialMessage != null && initialMessage.data['type'] == 'SilentAlarm') {
        _navigateToEmergency(initialMessage.data);
        return; 
      }

      final NotificationAppLaunchDetails? launchDetails =
          await LocalNotificationService.flutterLocalNotificationsPlugin.getNotificationAppLaunchDetails();
          
      if (launchDetails != null && launchDetails.didNotificationLaunchApp) {
        final String? payload = launchDetails.notificationResponse?.payload;
        if (payload != null) {
          _checkAndNavigate(payload);
        }
      }
    });
  }

  void _checkAndNavigate(String payload) {
    try {
      Map<String, dynamic> data = jsonDecode(payload);
      if (data['type'] == 'SilentAlarm') {
        _navigateToEmergency(data);
      }
    } catch (e) {
      print("Error parsing launch notification payload: $e");
    }
  }

  void _navigateToEmergency(Map<String, dynamic> data) {
    Future.delayed(const Duration(milliseconds: 400), () {
      if (navigatorKey.currentState != null) {
        navigatorKey.currentState?.pushNamedAndRemoveUntil(
          '/emergency_screen',
          (route) => false,
          arguments: data,
        );
      } else {
        print("⚠️ Navigator state is not initialized yet.");
      }
    });
  }

  Future<void> _managePollingStatus() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('auth_token');
    if (token != null && token.isNotEmpty) {
      startNotificationPolling();
      print("🔄 Opti-Sec Polling Pipeline Initialized...");
    } else {
      print("⚠️ Polling missed: Token is null or empty in SharedPreferences");
    }
  }

  @override
  void dispose() {
    stopNotificationPolling();
    _notificationSubscription.cancel();
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) async {
    final prefs = await SharedPreferences.getInstance();
    if (state == AppLifecycleState.paused || state == AppLifecycleState.inactive) {
      final now = DateTime.now();
      appPausedTime = now;
      await prefs.setString('app_paused_time', now.toIso8601String());
      print("App moved to background. Foreground Service keeps Polling alive! 🔥");
    } else if (state == AppLifecycleState.resumed) {
      _managePollingStatus();
      await checkSessionTimeout(context);
    }
  }

  Future<void> checkSessionTimeout(BuildContext context) async {
    DateTime? pausedTime;
    if (appPausedTime != null) {
      pausedTime = appPausedTime;
    } else {
      final prefs = await SharedPreferences.getInstance();
      final pausedTimeStr = prefs.getString('app_paused_time');
      if (pausedTimeStr != null) {
        pausedTime = DateTime.parse(pausedTimeStr);
      }
    }
    if (pausedTime != null) {
      final now = DateTime.now();
      const sessionDuration = Duration(minutes: 30);
      final difference = now.difference(pausedTime);

      if (difference < const Duration(seconds: 10) || difference.isNegative) {
        appPausedTime = null;
        return;
      }
      if (difference > sessionDuration) {
        appPausedTime = null;
        final prefs = await SharedPreferences.getInstance();
        await prefs.remove('app_paused_time');
        Provider.of<AuthProvider>(context, listen: false).setLoggedOut();
        await Future.delayed(const Duration(milliseconds: 300));

        stopNotificationPolling();

        navigatorKey.currentState?.pushAndRemoveUntil(
          MaterialPageRoute(builder: (_) => const LoginPage()),
          (route) => false,
        );
      } else {
        appPausedTime = null;
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      navigatorKey: navigatorKey,
      theme: ThemeData(
        scaffoldBackgroundColor: const Color(0xFF0B0F14),
        cardTheme: CardThemeData(color: Constants.cardColor),
        colorScheme: const ColorScheme.dark(
          primary: Colors.white,
          secondary: Color(0xFF4ADE80),
          surface: Color(0xFF0B0F14),
        ),
        textTheme: const TextTheme(
          bodySmall: TextStyle(fontSize: 16),
          bodyMedium: TextStyle(fontSize: 18),
          bodyLarge: TextStyle(fontSize: 25),
        ),
        appBarTheme: const AppBarTheme(surfaceTintColor: Colors.transparent),
        bottomNavigationBarTheme: BottomNavigationBarThemeData(
          backgroundColor: const Color(0xFF0B0F14),
          selectedItemColor: Constants.mainColor,
          unselectedItemColor: Colors.white,
          selectedLabelStyle: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
          unselectedLabelStyle: const TextStyle(fontSize: 14),
        ),
      ),
      debugShowCheckedModeBanner: false,
      routes: {
        '/emergency_screen': (context) {
          final args = ModalRoute.of(context)?.settings.arguments as Map<String, dynamic>?;
          return EmergencyScreen(data: args);
        },
      },
      home: widget.startScreen,
    );
  }
}