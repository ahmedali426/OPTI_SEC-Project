import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class CustomTextField extends StatefulWidget {
  const CustomTextField({
    super.key,
    required this.controller,
    required this.text,
    this.hasSuffix = false,
    this.hasLabel = false,
    this.hasborder = false,
    this.borderRadius = 12,
    this.hasPrefix = true,
    this.prefixIconName,
    this.isenable = true,
    this.isOTP = false,
    this.onchange = false,
    this.isLogin = false,
  });

  final TextEditingController controller;
  final String text;
  final bool hasSuffix;
  final bool hasborder;
  final double borderRadius;
  final bool hasLabel;
  final bool hasPrefix;
  final String? prefixIconName;
  final bool isenable;
  final bool isOTP;
  final bool onchange;
  final bool isLogin;

  @override
  State<CustomTextField> createState() => _CustomTextFieldState();
}

class _CustomTextFieldState extends State<CustomTextField> {
  bool isHidden = true;
  final Color contentColor = Colors.grey[400]!;

  final Map<dynamic, Icon> prefixIcons = {
    'name': Icon(Icons.person_outline),
    'username': Icon(Icons.person_pin_outlined),
    'email': Icon(Icons.email_outlined),
    'password': Icon(Icons.lock_outline),
    'phone': Icon(Icons.phone_outlined),
    'location': Icon(Icons.location_on_outlined),
    'gate': Icon(Icons.meeting_room_outlined),
    'device': Icon(Icons.developer_board_outlined),
    'alarm': Icon(Icons.campaign_outlined),
  };
  @override
  void initState() {
    super.initState();
    widget.controller.addListener(onTextChanged);
  }

  @override
  void dispose() {
    super.dispose();
    widget.controller.addListener(onTextChanged);
  }

  void onTextChanged() {
    if (mounted) {
      setState(() {});
    }
  }

  @override
  Widget build(BuildContext context) {
    bool hasText = widget.controller.text.isNotEmpty;
    return SizedBox(
      width: double.infinity,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          TextFormField(
            controller: widget.controller,
            keyboardType: widget.isOTP
                ? TextInputType.number
                : TextInputType.text,
            inputFormatters: widget.isOTP
                ? [LengthLimitingTextInputFormatter(1)]
                : null,
            onChanged: (value) {
              if (value.length == 1 && widget.isOTP) {
                FocusScope.of(context).nextFocus();
              }
            },
            validator: (value) {
              if (widget.text == '') {
                if (value!.isEmpty) {
                  return ' ';
                }
              } else {
                if (value == null || value.trim().isEmpty) {
                  return '${widget.text} is required , please fill it';
                }
                if (widget.text == 'Email' && !widget.isLogin) {
                  if (!RegExp(
                    r'^.+@[a-zA-Z]+\.{1}[a-zA-Z]+(\.{0,1}[a-zA-Z]+)$',
                  ).hasMatch(value)) {
                    return 'Enter a valid email address';
                  }
                }
                if (widget.text == 'Username') {
                  if (!RegExp(r'^[a-zA-Z0-9._]+$').hasMatch(value)) {
                    return 'Only letters, numbers, . and _ allowed';
                  }
                }
                if (widget.text == 'Password' && !widget.isLogin) {
                  if (value.length < 8) {
                    return 'Password must be at least 8 characters';
                  }
                  if (!RegExp(r'[A-Z]').hasMatch(value)) {
                    return 'Must contain at least one uppercase letter';
                  }
                  if (!RegExp(r'[a-z]').hasMatch(value)) {
                    return 'Must contain at least one lowercase letter';
                  }
                  if (!RegExp(r'\d').hasMatch(value)) {
                    return 'Password must contain at least one number';
                  }
                  if (!RegExp(r'[@$!%*#?&]').hasMatch(value)) {
                    return 'Password must contain at least one special character (@\$!%*#?&)';
                  }
                }
                if (widget.text == 'Phone') {
                  if (!RegExp(r'^(010|011|012|015)[0-9]{8}$').hasMatch(value)) {
                    return 'Enter a valid Egyptian phone number';
                  }
                }
                return null;
              }
              return null;
            },
            enabled: widget.isenable,
            textAlign: widget.text == '' ? TextAlign.center : TextAlign.start,
            obscureText: widget.hasSuffix ? isHidden : false,
            style: TextStyle(
              color: Colors.grey[200],
              fontSize: widget.text == '' ? 25 : 17,
            ),
            decoration: InputDecoration(
              hintText: widget.hasLabel
                  ? null
                  : 'Enter ${widget.text.toLowerCase()}',
              hintStyle: TextStyle(
                fontSize: widget.text == '' ? 25 : 17,
                color: contentColor,
              ),
              labelText: widget.hasLabel ? widget.text : null,
              labelStyle: TextStyle(fontSize: 17, color: contentColor),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(widget.borderRadius),
              ),
              errorStyle: TextStyle(fontSize: 14),
              filled: widget.isOTP
                  ? (widget.hasborder ? hasText : true)
                  : (widget.hasborder ? false : true),
              fillColor: widget.isOTP
                  ? (widget.hasborder
                        ? (hasText ? Color(0xFF111820) : Colors.transparent)
                        : Color(0xFF111820))
                  : Color(0xFF111820),
              enabledBorder: OutlineInputBorder(
                borderSide: widget.hasborder
                    ? (widget.isOTP && hasText
                          ? BorderSide.none
                          : BorderSide(color: Color(0xFF1C2631), width: 2))
                    : BorderSide.none,
                borderRadius: BorderRadius.circular(widget.borderRadius),
              ),
              focusedBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(widget.borderRadius),
                borderSide: BorderSide(color: Colors.grey[200]!),
              ),
              disabledBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(widget.borderRadius),
                borderSide: BorderSide(color: Color(0xFF1C2631), width: 2),
              ),
              prefixIcon: widget.hasPrefix
                  ? prefixIcons[widget.prefixIconName]
                  : null,
              prefixIconColor: contentColor,
              suffixIconColor: contentColor,
              suffixIcon: widget.hasSuffix
                  ? IconButton(
                      onPressed: () {
                        setState(() {
                          isHidden = !isHidden;
                        });
                      },
                      icon: isHidden
                          ? Icon(Icons.visibility_off_outlined)
                          : Icon(Icons.visibility_outlined),
                    )
                  : null,
            ),
          ),
        ],
      ),
    );
  }
}
