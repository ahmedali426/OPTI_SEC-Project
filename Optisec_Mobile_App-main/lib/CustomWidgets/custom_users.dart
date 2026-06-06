import 'package:flutter/material.dart';

class Customusers extends StatelessWidget {
  final String name;
  final String img;
  final String date;
  final String time;
  final bool isAuthorized;

  const Customusers({
    super.key,
    required this.name,
    required this.img,
    required this.date,
    required this.time,
    required this.isAuthorized,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 10),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 15),
              child: CircleAvatar(
                radius: 40,
                backgroundImage: NetworkImage(img),
              ),
            ),

            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(date),
                  Text(time),
                  Text(
                    name,
                    style: TextStyle(
                      color: isAuthorized ? Colors.green : Colors.red,
                      fontWeight: FontWeight.w500,
                    ),
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
