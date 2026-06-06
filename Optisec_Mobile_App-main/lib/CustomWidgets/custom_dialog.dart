import 'package:flutter/material.dart';

Future<dynamic> customDialog(
  BuildContext context, {
  required String content,
  required Color contentColor,
  required List<Widget> buttons,
  bool isSignOut = false,
}) {
  return showDialog(
    context: context,
    builder: (context) {
      return AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
        actionsAlignment: MainAxisAlignment.center,
        backgroundColor: Colors.white,
        actionsPadding: EdgeInsets.only(
          top: 15,
          bottom: 15,
          left: 20,
          right: 20,
        ),
        contentPadding: EdgeInsets.only(top: 20, left: 20, right: 20),
        content: Text(
          content,
          textAlign: TextAlign.center,
          style: TextStyle(
            color: contentColor,
            fontSize: 15,
            fontWeight: FontWeight.w600,
          ),
        ),
        actions: buttons,
      );
    },
  );
}
