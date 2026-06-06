import 'package:flutter/material.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_dialog.dart';

Future<dynamic> okDialog(
    BuildContext context, {
    required String content,
    required Color contentColor,
  }) {
    return customDialog(
      context,
      content: content,
      contentColor: contentColor,
      buttons: [
        SizedBox(
          width: 100,
          height: 40,
          child: CustomButton(
            text: 'Ok',
            textSize: 14,
            onPressed: () {
              Navigator.pop(context);
            },
            bgColor: Colors.black,
            textColor: Colors.white,
            borderRadius: 8,
          ),
        ),
      ],
    );
  }