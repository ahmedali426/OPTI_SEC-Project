import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Auth/pages/login_page.dart';
import 'package:optisecapp/Auth/pages/send_code.dart';
import 'package:optisecapp/Auth/provider/auth_provider.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:optisecapp/CustomWidgets/top_section.dart';
import 'package:provider/provider.dart';

class ForgetPassword extends StatefulWidget {
  const ForgetPassword({super.key});

  @override
  State<ForgetPassword> createState() => _ForgetPasswordState();
}

class _ForgetPasswordState extends State<ForgetPassword> {
  late TextEditingController email;
  GlobalKey<FormState> formState = GlobalKey<FormState>();
  bool isloading = false;
  @override
  void initState() {
    super.initState();
    email = TextEditingController();
  }

  @override
  void dispose() {
    super.dispose();
    email.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();
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
        padding: const EdgeInsets.all(20.0),
        child: ListView(
          scrollDirection: Axis.vertical,
          shrinkWrap: true,
          children: [
            TopSection(
              title: 'Forgot Password?',
              subTitle:
                  'No worries! Enter your email and we\'ll send you a reset code.',
              icon: 'email',
            ),

            SizedBox(height: 25),
            Form(
              key: formState,
              child: Column(
                spacing: 20,
                children: [
                  CustomTextField(
                    controller: email,
                    text: 'Email',
                    hasborder: true,
                    prefixIconName: 'email',
                  ),
                  CustomButton(
                    text: 'Send',
                    isLoading: isloading,
                    onPressed: () async {
                      if (formState.currentState!.validate()) {
                        final navigator = Navigator.of(context);
                        final scaffoldMessenger = ScaffoldMessenger.of(context);
                        final emailText = email.text.trim();
                        setState(() {
                          isloading = true;
                        });
                        final isSuccess = await ApiService().forgetPassword(
                          email: emailText,
                        );
                        if (isSuccess) {
                          setState(() {
                            isloading = false;
                          });
                          authProvider.loadData(email: email.text);
                          print('dataa:${authProvider.data}');
                          navigator.push(
                            MaterialPageRoute(builder: (context) => SendCode()),
                          );
                        } else {
                          setState(() {
                            isloading = false;
                          });
                          scaffoldMessenger.showSnackBar(
                            SnackBar(content: Text('Failed sending email')),
                          );
                        }
                      }
                    },
                    bgColor: Constants.mainColor,
                    textColor: Colors.white,
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
