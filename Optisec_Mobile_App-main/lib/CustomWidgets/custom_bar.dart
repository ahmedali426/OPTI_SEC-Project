
import 'package:flutter/material.dart';

class CustomBar extends StatelessWidget {
  final String password;
  const CustomBar({super.key, required this.password});

  int _getStrength() {
    int score = 0;
    if (password.length >= 8) score++;
    if (password.contains(RegExp(r'[0-9]'))) score++;
    if (password.contains(RegExp(r'[A-Z]'))) score++;
    if (password.contains(RegExp(r'[!@#\$&*~%^()]'))) score++;
    return score;
  }

  Color _getColor(int index, int strength) {
    if (index >= strength) return const Color(0xff1e2230);
    switch (strength) {
      case 1:
        return const Color(0xffef4444);
      case 2:
        return const Color(0xfff59e0b);
      case 3:
        return const Color(0xff0176EA);
      case 4:
        return const Color(0xff22c55e);
      default:
        return const Color(0xff1e2230);
    }
  }

  String _getLabel(int strength) {
    switch (strength) {
      case 1:
        return 'Weak';
      case 2:
        return 'Medium';
      case 3:
        return 'Strong';
      case 4:
        return 'Very Strong';
      default:
        return '';
    }
  }

  @override
  Widget build(BuildContext context) {
    final strength = _getStrength();
    return Column(
      spacing: 10,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          spacing: 8,
          children: List.generate(
            4,
            (index) => Expanded(
              child: AnimatedContainer(
                duration: Duration(milliseconds: 400),
                height: 5,
                decoration: BoxDecoration(
                  color: strength == 0
                      ? Colors.transparent
                      : _getColor(index, strength),
                  borderRadius: BorderRadius.circular(3),
                ),
              ),
            ),
          ),
        ),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              _getLabel(strength),
              style: TextStyle(fontSize: 14, fontWeight: FontWeight.w300),
            ),
            if (strength < 4)
              Text(
                'Add symbols to strengthen',
                style: TextStyle(fontSize: 12, fontWeight: FontWeight.w300),
              ),
          ],
        ),
      ],
    );
  }
}
