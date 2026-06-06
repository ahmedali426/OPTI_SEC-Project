import 'package:flutter/material.dart';
import 'package:optisecapp/Auth/pages/login_page.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/top_section.dart';

class SuccessResetPassword extends StatelessWidget {
  const SuccessResetPassword({super.key});

  @override
  Widget build(BuildContext context) {
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
        child: Column(
          spacing: 140,
          children: [
            TopSection(
              icon: 'check',
              title: 'Password Reset!',
              subTitle:
                  'Your password has been successfully updated. You can now sign in with your new password.',
            ),
            CustomButton(
              text: 'Go to Sign in',
              onPressed: () {
                Navigator.pushAndRemoveUntil(
                  context,
                  MaterialPageRoute(builder: (context) => LoginPage()),
                  (route) => false,
                );
              },
              bgColor: Constants.mainColor,
              textColor: Colors.white,
            ),
          ],
        ),
      ),
    );
  }
}
