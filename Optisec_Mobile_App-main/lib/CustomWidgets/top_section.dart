import 'package:flutter/material.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';

class TopSection extends StatelessWidget {
  final String icon;
  final String title;
  final String subTitle;
  final bool isPassword;
  const TopSection({
    super.key,
    required this.icon,
    required this.title,
    required this.subTitle,
    this.isPassword = false,
  });

  @override
  Widget build(BuildContext context) {
    String firsttext = '', email = '';
    if (isPassword) {
      firsttext = (subTitle.split('\n') as List)[0];
      email = (subTitle.split('\n') as List)[1].toString();
    }
    return Column(
      children: [
        Align(
          alignment: Alignment.topCenter,
          child: Container(
            width: 90,
            height: 90,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: Constants.cardColor,
              border: Border.all(color: Constants.mainColor),
              borderRadius: BorderRadius.circular(15),
            ),
            child: Icon(
              icon.toLowerCase() == 'email'
                  ? Icons.email_outlined
                  : icon.toLowerCase() == 'password'
                  ? Icons.lock_outline
                  : icon.toLowerCase() == 'shield'
                  ? Icons.shield_outlined
                  : Icons.check,
              size: 45,
              color: Constants.mainColor,
            ),
          ),
        ),
        SizedBox(height: 25),
        Text(
          title,
          style: TextStyle(fontSize: 28, fontWeight: FontWeight.w400),
        ),
        SizedBox(height: 10),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 40.0),
          child: Column(
            children: [
              Text(
                isPassword ? firsttext : subTitle,
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w300,
                  height: 1.2,
                ),
              ),
              Text(
                isPassword ? email : '',
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 14,
                  color: Constants.mainColor,
                  fontWeight: FontWeight.w300,
                  height: 1.2,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
