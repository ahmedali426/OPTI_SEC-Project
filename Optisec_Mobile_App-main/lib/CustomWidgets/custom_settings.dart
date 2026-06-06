import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Auth/pages/login_page.dart';
import 'package:optisecapp/Auth/provider/auth_provider.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_dialog.dart';
import 'package:provider/provider.dart';

class CustomSettings extends StatefulWidget {
  const CustomSettings({super.key});

  @override
  State<CustomSettings> createState() => _CustomSettingsState();
}

class _CustomSettingsState extends State<CustomSettings> {
  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();
    final user = authProvider.user;
    var isSwitched = authProvider.isSwitched;
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'SETTINGS',
          style: TextStyle(fontSize: TextTheme.of(context).bodyLarge?.fontSize),
        ),
        leading: Container(
          margin: const EdgeInsets.only(left: 15.0),
          child: IconButton(
            onPressed: () {
              Navigator.pop(context);
            },
            icon: Icon(Icons.arrow_back_ios),
          ),
        ),
      ),
      body: Padding(
        padding: EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Card(
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  vertical: 20.0,
                  horizontal: 16,
                ),
                child: Row(
                  children: [
                    Container(
                      width: 70,
                      height: 70,
                      alignment: Alignment.center,
                      decoration: BoxDecoration(
                        color: const Color.fromARGB(255, 157, 205, 245),
                        shape: BoxShape.circle,
                        border: Border.all(color: Color(0xff0176EA), width: 3),
                      ),
                      child: Text(
                        user['fname'][0].toString().toUpperCase(),
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          fontSize: 35,
                          color: Color(0xff0176EA),
                          fontWeight: FontWeight.w800,
                        ),
                      ),
                    ),
                    SizedBox(width: 15),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '${user['fname']} ${user['lname']}',
                          style: TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        SizedBox(height: 5),
                        Text(
                          '${user['email']}',
                          style: TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.w300,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            SizedBox(height: 30),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 10),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Preferences',
                    style: TextStyle(fontSize: 22, fontWeight: FontWeight.w300),
                  ),
                  SizedBox(height: 8),
                  ListTile(
                    onTap: () {
                      authProvider.triggerNotifications();
                    },
                    contentPadding: EdgeInsets.zero,
                    leading: Container(
                      padding: EdgeInsets.all(15),
                      decoration: BoxDecoration(
                        color: Constants.cardColor,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Icon(Icons.notifications_outlined, size: 30),
                    ),
                    title: Text(
                      'Notifications',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                    subtitle: Text(
                      'Alerts',
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w300,
                      ),
                    ),
                    trailing: Padding(
                      padding: const EdgeInsets.all(10.0),
                      child: Switch(
                        value: isSwitched,
                        activeTrackColor: Constants.mainColor,
                        activeThumbColor: Colors.white,
                        onChanged: (value) {
                          authProvider.triggerNotifications();
                        },
                      ),
                    ),
                  ),
                ],
              ),
            ),
            SizedBox(height: 15),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 10),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                spacing: 10,
                children: [
                  Text(
                    'System',
                    style: TextStyle(fontSize: 22, fontWeight: FontWeight.w300),
                  ),
                  ListTile(
                    contentPadding: EdgeInsets.zero,
                    leading: Container(
                      padding: EdgeInsets.all(15),
                      decoration: BoxDecoration(
                        color: Constants.cardColor,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Icon(Icons.adjust_outlined, size: 30),
                    ),
                    title: Text(
                      'About',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                    subtitle: Text(
                      'Version 1.0.0',
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w300,
                      ),
                    ),
                  ),
                  ListTile(
                    onTap: () {
                      customDialog(
                        context,
                        content: 'Are you sure you want to sign out?',
                        contentColor: Colors.black,
                        isSignOut: true,
                        buttons: [
                          SizedBox(
                            width: 100,
                            height: 40,
                            child: CustomButton(
                              text: 'Cancel',
                              textSize: 14,
                              onPressed: () {
                                Navigator.pop(context, false);
                              },
                              bgColor: Colors.black,
                              textColor: Colors.white,
                              borderRadius: 8,
                            ),
                          ),
                          SizedBox(
                            width: 110,
                            height: 40,
                            child: CustomButton(
                              text: 'SignOut',
                              textSize: 14,
                              onPressed: () async {
                                final navigator = Navigator.of(context);
                                final scaffoldMessenger = ScaffoldMessenger.of(
                                  context,
                                );
                                navigator.pop();
                                showDialog(
                                  context: context,
                                  barrierDismissible: false,
                                  builder: (context) => Center(
                                    child: CircularProgressIndicator(),
                                  ),
                                );
                                final isSuccess = await ApiService().signOut();
                                if (isSuccess) {
                                  navigator.pushAndRemoveUntil(
                                    MaterialPageRoute(
                                      builder: (context) => LoginPage(),
                                    ),
                                    (route) => false,
                                  );
                                } else {
                                  navigator.pop(true);
                                  scaffoldMessenger.showSnackBar(
                                    SnackBar(
                                      content: Text(
                                        'Sign out failed. Please try again.',
                                      ),
                                    ),
                                  );
                                }
                              },
                              bgColor: Colors.white,
                              textColor: Colors.red,
                              borderRadius: 8,
                            ),
                          ),
                        ],
                      );
                    },
                    contentPadding: EdgeInsets.zero,
                    leading: Container(
                      padding: EdgeInsets.all(15),
                      decoration: BoxDecoration(
                        color: Constants.cardColor,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Icon(
                        Icons.logout_outlined,
                        color: Colors.red,
                        size: 30,
                      ),
                    ),
                    title: Text(
                      'Sign Out',
                      style: TextStyle(
                        fontSize: 18,
                        color: Colors.red,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                    trailing: Padding(
                      padding: const EdgeInsets.all(10.0),
                      child: Icon(
                        Icons.arrow_forward_ios,
                        color: const Color.fromARGB(137, 238, 238, 238),
                        size: 18,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
