import 'package:optisecapp/CustomWidgets/constants.dart';
import 'package:flutter/material.dart';
import 'package:optisecapp/CustomWidgets/image_modal_widget.dart';

Future<dynamic> bottomModal(
  BuildContext context, {
  required Widget childWidget,
}) {
  // var model = context.read<AdminProvider>();
  return showModalBottomSheet(
    context: context,
    builder: (context) {
      return Container(
        height: childWidget == ImageModalWidget ? 200 : 300,
        padding: EdgeInsets.symmetric(vertical: 5, horizontal: 10),
        decoration: BoxDecoration(
          color: Constants.cardColor,
          borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
        ),
        child: childWidget,
      );
    },
  );
}
