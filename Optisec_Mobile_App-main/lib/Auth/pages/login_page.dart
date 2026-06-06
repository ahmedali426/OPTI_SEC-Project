// ignore_for_file: avoid_print

import 'package:flutter/material.dart';
import 'package:optisecapp/Admin/Pages/admin_home_page.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Auth/pages/forget_password.dart';
import 'package:optisecapp/Auth/provider/auth_provider.dart';
import 'package:optisecapp/Client/Pages/client_home_page.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:optisecapp/services/firebase_notifications.dart';
import 'package:optisecapp/services/notifications.dart';
import 'package:provider/provider.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  late TextEditingController email;
  late TextEditingController password;
  GlobalKey<FormState> formState = GlobalKey<FormState>();
  @override
  void initState() {
    email = TextEditingController();
    password = TextEditingController();
    super.initState();
  }

  @override
  void dispose() {
    email.dispose();
    password.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();
    return Scaffold(
      body: Padding(
        padding: const EdgeInsets.only(
          top: 50,
          left: 30,
          right: 30,
          bottom: 30,
        ),
        child: ListView(
          scrollDirection: Axis.vertical,
          shrinkWrap: true,
          children: [
            Container(
              alignment: Alignment.center,
              margin: EdgeInsets.only(bottom: 20),
              child: Image.asset(
                'images/logo.png',
                width: 150,
                fit: BoxFit.cover,
              ),
            ),
            Text(
              'PLEASE ENTER YOUR ACCOUNT',
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.w400,
                color: Colors.white,
              ),
            ),
            SizedBox(height: 20),
            Form(
              key: formState,
              child: Column(
                children: [
                  CustomTextField(
                    controller: email,
                    text: 'Email',
                    hasLabel: true,
                    isLogin: true,
                    hasPrefix: false,
                  ),
                  SizedBox(height: 15),
                  CustomTextField(
                    controller: password,
                    text: 'Password',
                    isLogin: true,
                    hasSuffix: true,
                    hasLabel: true,
                    hasPrefix: false,
                  ),
                  SizedBox(height: 15),
                  InkWell(
                    onTap: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (context) => ForgetPassword(),
                        ),
                      );
                    },
                    child: Text(
                      'Forgot Password?',
                      style: TextStyle(fontSize: 15, color: Color(0xff0176EA)),
                    ),
                  ),
                  SizedBox(height: 25),
                  CustomButton(
                    bgColor: Color(0xff0176EA),
                    textColor: Colors.white,
                    text: 'Login',
                    onPressed: () async {
                      if (formState.currentState!.validate()) {
                        final navigator = Navigator.of(context);
                        final scaffoldMessenger = ScaffoldMessenger.of(context);
                        showDialog(
                          context: context,
                          barrierDismissible: false,
                          builder: (context) =>
                              Center(child: CircularProgressIndicator()),
                        );
                        final token = await ApiService().login(
                          email: email.text.trim(),
                          password: password.text.trim(),
                        );
                        if (token != null) {
                          navigator.pop();
                          await authProvider.loadUserData();
                          await FirebaseNotifications().sendTokenAfterLogin();
                          startNotificationPolling();
                          if (authProvider.user['roles'][0] == 'Admin') {
                            print('this is admin');
                            navigator.pushReplacement(
                              MaterialPageRoute(
                                builder: (context) => AdminHomePage(),
                              ),
                            );
                          } else {
                            print('this is client');
                            navigator.pushReplacement(
                              MaterialPageRoute(
                                builder: (context) => ClientHomePage(),
                              ),
                            );
                          }
                        } else {
                          navigator.pop();
                          scaffoldMessenger.showSnackBar(
                            SnackBar(
                              content: Text(
                                "Login failed, please check credentials",
                              ),
                            ),
                          );
                        }
                      }
                    },
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
