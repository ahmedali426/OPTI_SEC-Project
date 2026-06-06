import 'package:flutter/material.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';

class CustomCard extends StatelessWidget {
  final String name;
  final String img;
  final bool selected;
  final bool isGate;
  final String location;
  const CustomCard({
    super.key,
    required this.name,
    required this.img,
    this.selected = false,
    this.isGate = false,
    this.location = '',
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      color: selected
          ? Color.fromARGB(33, 1, 118, 234)
          : Constants.cardColor,
      child: Container(
        // height: 130,
        padding: EdgeInsets.symmetric(vertical: 10, horizontal: 15),
        child: Row(
          children: [
            isGate
                ? Text('')
                : selected
                ? Container(
                    width: 80,
                    height: 80,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      color: Color(0xff0176EA),
                    ),
                    child: Icon(
                      Icons.check_rounded,
                      size: 40,
                      color: Colors.white,
                      fontWeight: FontWeight.w600,
                    ),
                  )
                : CircleAvatar(radius: 40, backgroundImage: NetworkImage(img)),
            // : Text(''),
            Expanded(
              flex: 2,
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 15),
                child: isGate
                    ? Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        spacing: 5,
                        children: [
                          Text(name, style: TextStyle(color: Colors.white)),
                          Text(
                            'Location : $location',
                            style: TextStyle(color: Colors.white),
                          ),
                        ],
                      )
                    : Text(name, style: TextStyle(color: Colors.white)),
              ),
            ),
            (isGate && selected)
                ? Container(
                    width: 40,
                    height: 40,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      color: Color(0xff0176EA),
                    ),
                    child: Icon(
                      Icons.check_rounded,
                      size: 20,
                      color: Colors.white,
                      fontWeight: FontWeight.w600,
                    ),
                  )
                : Text(''),
          ],
        ),
      ),
    );
  }
}
