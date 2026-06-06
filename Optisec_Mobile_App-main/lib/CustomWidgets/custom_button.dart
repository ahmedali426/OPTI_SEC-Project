import 'package:flutter/material.dart';

class CustomButton extends StatelessWidget {
  const CustomButton({
    super.key,
    required this.text,
    this.textSize,
    required this.onPressed,
    required this.bgColor,
    required this.textColor,
    this.borderColor,
    this.borderRadius,
    this.isLoading = false,
  });

  final String text;
  final double? textSize;
  final void Function()? onPressed;
  final Color bgColor;
  final Color textColor;
  final Color? borderColor;
  final double? borderRadius;
  final bool isLoading;

  @override
  Widget build(BuildContext context) {
    return ElevatedButton(
      onPressed: onPressed,
      style: ElevatedButton.styleFrom(
        elevation: 0,
        backgroundColor: bgColor,
        foregroundColor: textColor,
        shape: RoundedRectangleBorder(
          side: BorderSide(
            color: borderColor == null ? Colors.transparent : borderColor!,
          ),
          borderRadius: BorderRadius.circular(
            borderRadius == null ? 10 : borderRadius!,
          ),
        ),
        minimumSize: Size(double.infinity, 45),
        textStyle: TextStyle(
          color: Colors.white,
          fontWeight: FontWeight.w500,
          fontSize: textSize == null ? 17 : textSize!,
        ),
      ),
      child: isLoading
          ? Center(
              child: CircularProgressIndicator(
                color: Colors.white,
                constraints: BoxConstraints(minWidth: 25, minHeight: 25),
              ),
            )
          : Text(text),
    );
  }
}
