import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';

class EmergencyScreen extends StatefulWidget {
  final data;
  const EmergencyScreen({super.key, this.data});

  @override
  State<EmergencyScreen> createState() => _EmergencyScreenState();
}

class _EmergencyScreenState extends State<EmergencyScreen>
    with TickerProviderStateMixin {
  late AnimationController animationController;
  late AnimationController controller;
  late Animation<double> glowAnimation;
  late Animation<double> animation;

  @override
  void initState() {
    super.initState();
    animationController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 900),
    )..repeat(reverse: true);

    glowAnimation = Tween<double>(begin: 3, end: 45).animate(
      CurvedAnimation(parent: animationController, curve: Curves.easeInOut),
    );

    controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 900),
    )..repeat(reverse: true);

    animation = Tween<double>(begin: 4, end: 40).animate(
      CurvedAnimation(parent: controller, curve: Curves.easeInOut),
    );
  }

  void _dismissEmergencyNotification() async {
    try {
      await FlutterLocalNotificationsPlugin().cancelAll();
      print("🔔 Opti-Sec: Emergency siren and notification stopped by user hand.");
    } catch (e) {
      print("⚠️ Error dismissing notification: $e");
    }
  }

  @override
  void dispose() {
    animationController.dispose();
    controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final Map<String, dynamic>? data = widget.data;
    return Scaffold(
      backgroundColor: const Color(0xFF0B0F14), 
      body: Container(
        padding: const EdgeInsets.all(20),
        margin: const EdgeInsets.only(top: 100),
        width: double.infinity,
        child: Column(
          children: [
            Column(
              children: [
                AnimatedBuilder(
                  animation: glowAnimation,
                  builder: (context, child) {
                    return Container(
                      width: 90,
                      height: 90,
                      alignment: Alignment.center,
                      decoration: BoxDecoration(
                        color: Colors.transparent,
                        shape: BoxShape.circle,
                        boxShadow: [
                          BoxShadow(
                            color: Colors.red.withOpacity(
                              0.5 * (glowAnimation.value / 45),
                            ),
                            blurRadius: glowAnimation.value,
                            spreadRadius: glowAnimation.value / 2,
                          ),
                        ],
                      ),
                      child: Icon(
                        Icons.gpp_maybe_outlined,
                        size: 80,
                        color: Colors.red.withOpacity(0.8),
                      ),
                    );
                  },
                ),
                const SizedBox(height: 25),
                Text(
                  data?['title'] ?? 'EMERGENCY CASE',
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    fontSize: 24,
                    fontWeight: FontWeight.w500,
                    letterSpacing: 1.2,
                    color: Colors.red,
                  ),
                ),
                const SizedBox(height: 10),
                Text(
                  data?['body'] ?? 'An unregistered individual is attempting entry.',
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    fontSize: 16,
                    color: Colors.grey,
                    height: 1.4,
                  ),
                ),
              ],
            ),
            const Spacer(),
            AnimatedBuilder(
              animation: animation,
              builder: (context, child) {
                return GestureDetector(
                  onTap: () {
                    _dismissEmergencyNotification();
                    SystemNavigator.pop(); 
                  },
                  child: Container(
                    width: 100,
                    height: 100,
                    alignment: Alignment.center,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      color: const Color.fromARGB(255, 63, 63, 63),
                      boxShadow: [
                        BoxShadow(
                          color: Constants.cardColor.withOpacity(
                            0.4 * (animation.value / 40),
                          ),
                          blurRadius: animation.value,
                          spreadRadius: animation.value / 2,
                        ),
                      ],
                    ),
                    child: const Icon(Icons.close, size: 30),
                  ),
                );
              },
            ),
            const SizedBox(height: 50),
          ],
        ),
      ),
    );
  }
}