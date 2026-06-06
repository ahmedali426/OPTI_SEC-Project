import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Auth/pages/success_reset_password.dart';
import 'package:optisecapp/Auth/provider/auth_provider.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';
import 'package:optisecapp/CustomWidgets/custom_bar.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:optisecapp/CustomWidgets/top_section.dart';
import 'package:provider/provider.dart';

class ResetPassword extends StatefulWidget {
  const ResetPassword({super.key});

  @override
  State<ResetPassword> createState() => _ResetPasswordState();
}

class _ResetPasswordState extends State<ResetPassword> {
  late TextEditingController password;
  GlobalKey<FormState> formState = GlobalKey<FormState>();
  bool isloading = false;
  @override
  void initState() {
    super.initState();
    password = TextEditingController();
    password.addListener(() => setState(() {}));
  }

  @override
  void dispose() {
    super.dispose();
    password.dispose();
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
        padding: EdgeInsets.all(20),
        child: ListView(
          scrollDirection: Axis.vertical,
          shrinkWrap: true,
          children: [
            TopSection(
              icon: 'shield',
              title: 'Create New Password',
              subTitle:
                  'Must be at least 8 characters with a mix of letters and numbers.',
            ),
            SizedBox(height: 25),
            Form(
              key: formState,
              child: Column(
                children: [
                  CustomTextField(
                    controller: password,
                    text: 'password',
                    hasSuffix: true,
                    prefixIconName: 'password',
                    hasborder: true,
                    onchange: true,
                  ),
                  SizedBox(height: 10),
                  CustomBar(password: password.text),
                  SizedBox(height: 30),
                  CustomButton(
                    text: 'Set New Password',
                    isLoading: isloading,
                    onPressed: () async {
                      if (formState.currentState!.validate()) {
                        final passwordText = password.text;
                        authProvider.loadData(password: passwordText);
                        setState(() {
                          isloading = true;
                        });
                        final isSuccess = await ApiService().resetPassword(
                          data: authProvider.data,
                        );
                        print('daaata:${authProvider.data}');
                        if (isSuccess) {
                          setState(() {
                            isloading = false;
                          });
                          Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (context) => SuccessResetPassword(),
                            ),
                          );
                        } else {
                          setState(() {
                            isloading = false;
                          });
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(content: Text('Faild to reset password')),
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
          ],
        ),
      ),
    );
  }
}
