import 'package:flutter/material.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_dialog.dart';
// Future<dynamic> deleteDialog(BuildContext context) {
//   return customDialog(
//     context,
//     content: 'Are you sure you want to delete this ?',
//     contentColor: Colors.black,
//     buttons: [
//       SizedBox(
//         width: 100,
//         height: 40,
//         child: CustomButton(
//           text: 'Cancel',
//           textSize: 14,
//           onPressed: () {
//             Navigator.pop(context);
//           },
//           bgColor: Colors.black,
//           textColor: Colors.white,
//           borderRadius: 8,
//         ),
//       ),
//       SizedBox(
//         width: 100,
//         height: 40,
//         child: CustomButton(
//           text: 'Delete',
//           textSize: 14,
//           onPressed: () {
//             Navigator.pop(context);
//             okDialog(
//                 context,
//                 content: 'User has been deleted',
//                 contentColor: Colors.red,
//               );
//           },
//           bgColor: Colors.red,
//           textColor: Colors.white,
//           borderRadius: 8,
//         ),
//       ),
//     ],
//   );
// }
// ===========================
Future<bool> deleteDialog(BuildContext context) async {
  final result = await customDialog(
    context,
    content: 'Are you sure you want to delete this ?',
    contentColor: Colors.black,
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
        width: 100,
        height: 40,
        child: CustomButton(
          text: 'Delete',
          textSize: 14,
          onPressed: () {
            Navigator.pop(context, true);
          },
          bgColor: Colors.red,
          textColor: Colors.white,
          borderRadius: 8,
        ),
      ),
    ],
  );

  return result ?? false;
}
