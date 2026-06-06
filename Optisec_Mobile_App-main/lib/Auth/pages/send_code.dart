import 'dart:async';

import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Auth/pages/login_page.dart';
import 'package:optisecapp/Auth/pages/reset_password.dart';
import 'package:optisecapp/Auth/provider/auth_provider.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:optisecapp/CustomWidgets/top_section.dart';
import 'package:provider/provider.dart';

class SendCode extends StatefulWidget {
  const SendCode({super.key});

  @override
  State<SendCode> createState() => _SendCodeState();
}

class _SendCodeState extends State<SendCode> {
  late TextEditingController number1;
  late TextEditingController number2;
  late TextEditingController number3;
  late TextEditingController number4;
  late TextEditingController number5;
  late TextEditingController number6;
  GlobalKey<FormState> formState = GlobalKey<FormState>();
  bool isloading = false;
  int start = 300;
  Timer? timer;
  void startTimer() {
    start = 300;
    timer = Timer.periodic(Duration(seconds: 1), (time) {
      if (start == 0) {
        setState(() {
          timer?.cancel();
        });
      } else {
        setState(() {
          start--;
        });
      }
    });
  }

  String formatTime(int seconds) {
    final minutes = seconds ~/ 60;
    final remainingSeconds = seconds % 60;
    return '$minutes:${remainingSeconds.toString().padLeft(2, '0')}';
  }

  @override
  void initState() {
    super.initState();
    startTimer();
    number1 = TextEditingController();
    number2 = TextEditingController();
    number3 = TextEditingController();
    number4 = TextEditingController();
    number5 = TextEditingController();
    number6 = TextEditingController();
  }

  @override
  void dispose() {
    super.dispose();
    timer?.cancel();
    number1.dispose();
    number2.dispose();
    number3.dispose();
    number4.dispose();
    number5.dispose();
    number6.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final authprovider = context.watch<AuthProvider>();
    return Scaffold(
      appBar: AppBar(
        leading: Container(
          margin: const EdgeInsets.only(left: 15.0),
          child: IconButton(
            onPressed: () => Navigator.pop(context, true),
            icon: Icon(Icons.arrow_back_ios),
          ),
        ),
      ),
      body: Padding(
        padding: EdgeInsets.all(20),
        child: ListView(
          scrollDirection: Axis.vertical,
          shrinkWrap: true,
          children: [
            TopSection(
              icon: 'password',
              title: 'Check Your Email',
              subTitle:
                  'We sent a 6-digit code to\n${authprovider.data['email']}',
              isPassword: true,
            ),
            SizedBox(height: 25),
            Form(
              key: formState,
              child: Column(
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceAround,
                    children: [
                      SizedBox(
                        width: 50,
                        child: CustomTextField(
                          controller: number1,
                          text: '',
                          borderRadius: 15,
                          hasborder: true,
                          isOTP: true,
                        ),
                      ),
                      SizedBox(
                        width: 50,
                        child: CustomTextField(
                          controller: number2,
                          text: '',
                          borderRadius: 15,
                          hasborder: true,
                          isOTP: true,
                        ),
                      ),
                      SizedBox(
                        width: 50,
                        child: CustomTextField(
                          controller: number3,
                          text: '',
                          borderRadius: 15,
                          hasborder: true,
                          isOTP: true,
                        ),
                      ),
                      SizedBox(
                        width: 50,
                        child: CustomTextField(
                          controller: number4,
                          text: '',
                          borderRadius: 15,
                          hasborder: true,
                          isOTP: true,
                        ),
                      ),
                      SizedBox(
                        width: 50,
                        child: CustomTextField(
                          controller: number5,
                          text: '',
                          hasborder: true,
                          borderRadius: 15,
                          isOTP: true,
                        ),
                      ),
                      SizedBox(
                        width: 50,
                        child: CustomTextField(
                          controller: number6,
                          text: '',
                          hasborder: true,
                          borderRadius: 15,
                          isOTP: true,
                        ),
                      ),
                    ],
                  ),
                  SizedBox(height: 15),
                  Text.rich(
                    TextSpan(
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w300,
                        height: 1.2,
                      ),
                      children: [
                        TextSpan(text: 'code expires in '),
                        TextSpan(
                          text: formatTime(start),
                          style: TextStyle(
                            color: start == 0 ? Colors.red : Colors.amber,
                          ),
                        ),
                      ],
                    ),
                  ),
                  SizedBox(height: 30),
                  CustomButton(
                    text: 'Verify Code',
                    onPressed: () {
                      if (formState.currentState!.validate()) {
                        authprovider.loadData(
                          code:
                              '${number1.text}${number2.text}${number3.text}${number4.text}${number5.text}${number6.text}',
                        );
                        print('dataaa:${authprovider.data}');
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (context) => ResetPassword(),
                          ),
                        );
                      }
                    },
                    bgColor: Constants.mainColor,
                    textColor: Colors.white,
                  ),
                  SizedBox(height: 10),
                  CustomButton(
                    text: 'Resend Code',
                    isLoading: isloading,
                    onPressed: () async {
                      if (start == 0 && !isloading) {
                        setState(() {
                          isloading = true;
                        });
                        final isSuccess = await ApiService().forgetPassword(
                          email: authprovider.data['email'],
                        );
                        if (isSuccess) {
                          startTimer();
                          setState(() {
                            isloading = false;
                          });
                          authprovider.loadData(
                            email: authprovider.data['email'],
                          );
                          print('dataa:${authprovider.data}');
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text(
                                'Successfully sending email, check your email',
                              ),
                            ),
                          );
                        } else {
                          setState(() {
                            isloading = false;
                          });
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(content: Text('Failed sending email')),
                          );
                        }
                      }
                    },
                    bgColor: Colors.transparent,
                    textColor: Colors.grey[400]!,
                    borderColor: start == 0
                        ? Constants.cardColor
                        : Color(0xFF1C2631),
                  ),
                ],
              ),
            ),
            SizedBox(height: 10),
            Row(
              spacing: 5,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(
                  'Remember your password?',
                  style: TextStyle(
                    fontSize: 14,
                    fontWeight: FontWeight.w300,
                    height: 1.2,
                  ),
                ),
                InkWell(
                  onTap: () {
                    Navigator.pushAndRemoveUntil(
                      context,
                      MaterialPageRoute(builder: (context) => LoginPage()),
                      (route) => false,
                    );
                  },
                  child: Text(
                    'Sign in',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontSize: 15,
                      color: Constants.mainColor,
                      fontWeight: FontWeight.w300,
                      height: 1.2,
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
