import 'package:flutter/material.dart';
import 'package:optisecapp/Admin/provider/admin_provider.dart';
import 'package:optisecapp/Client/Pages/gates_page.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:optisecapp/CustomWidgets/custom_settings.dart';
import 'package:provider/provider.dart';

class Customdashboard extends StatelessWidget {
  final String username;
  final Widget page;
  final bool isClient;
  Customdashboard({
    super.key,
    required this.username,
    required this.page,
    required this.isClient,
  });
  final Color mainColor = Color(0xff0176EA);
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('DASHBOARD', style: Theme.of(context).textTheme.bodyLarge),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 10),
            child: IconButton(
              onPressed: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(builder: (context) => CustomSettings()),
                );
              },
              icon: Icon(Icons.settings_outlined, size: 28),
            ),
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Welcome, $username',
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.w400,
                color: Colors.white,
              ),
            ),
            SizedBox(height: 10),
            Card(
              margin: EdgeInsets.zero,
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: 20,
                  vertical: 25,
                ),
                child: SizedBox(
                  // height: 110,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Row(
                        children: [
                          Icon(
                            Icons.group_outlined,
                            size: 50,
                            color: mainColor,
                          ),
                          SizedBox(width: 10),
                          context.watch<AdminProvider>().isloading
                              ? Center(child: CircularProgressIndicator())
                              : Text(
                                  isClient
                                      ? context
                                            .watch<UserProvider>()
                                            .usersCount()
                                            .toString()
                                      : context
                                            .watch<AdminProvider>()
                                            .clientsCount()
                                            .toString(),
                                  style: TextStyle(
                                    fontSize: 35,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                        ],
                      ),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            isClient ? 'Users' : 'Clients',
                            style: TextStyle(
                              fontSize: 20,
                              fontWeight: FontWeight.w400,
                            ),
                          ),
                          GestureDetector(
                            onTap: () {
                              Navigator.push(
                                context,
                                MaterialPageRoute(builder: (context) => page),
                              );
                            },
                            child: Icon(
                              Icons.remove_red_eye_outlined,
                              size: 24,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
            ),
            if (isClient) ...[
              Card(
                margin: EdgeInsets.only(top: 15),
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 20,
                    vertical: 16,
                  ),
                  child: SizedBox(
                    // height: 110,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Row(
                          children: [
                            Icon(
                              Icons.meeting_room_outlined,
                              size: 50,
                              color: mainColor,
                            ),
                            SizedBox(width: 8),
                            context.watch<UserProvider>().isGatesLoading
                                ? Center(child: CircularProgressIndicator())
                                : Text(
                                    context
                                        .watch<UserProvider>()
                                        .gatesCount()
                                        .toString(),
                                    style: TextStyle(
                                      fontSize: 35,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                          ],
                        ),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Gates',
                              style: TextStyle(
                                fontSize: 20,
                                fontWeight: FontWeight.w400,
                              ),
                            ),
                            GestureDetector(
                              onTap: () {
                                Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (context) =>
                                        GatesPage(showBackArrow: true),
                                  ),
                                );
                              },
                              child: Icon(
                                Icons.remove_red_eye_outlined,
                                size: 24,
                              ),
                            ),
                          ],
                        ),
                        // SizedBox(height: 10),
                        // CustomButton(
                        //   text: 'Send',
                        //   onPressed: () async {
                        //     await Future.delayed(const Duration(seconds: 2));

                        //     // استدعاء دالة المنبه اللي في الـ Service عندكِ مباشرة
                        //     await LocalNotificationService.showEmergencyNotification(
                        //       RemoteMessage(
                        //         data: {
                        //           'title': 'تيست داخلي',
                        //           'body': 'محاولة دخول طارئة',
                        //           'isEmergency': 'true',
                        //         },
                        //       ),
                        //     );
                        //   },
                        //   bgColor: Colors.green,
                        //   textColor: Colors.white,
                        // ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
