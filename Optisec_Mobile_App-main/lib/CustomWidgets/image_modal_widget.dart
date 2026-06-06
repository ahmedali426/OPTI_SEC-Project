import 'dart:io';

import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:path/path.dart' as p;
import 'package:provider/provider.dart';
class ImageModalWidget extends StatelessWidget {
  const ImageModalWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Align(
          alignment: Alignment.topRight,
          child: IconButton(
            onPressed: () {
              Navigator.pop(context);
            },
            icon: Icon(Icons.close, color: Colors.grey[200], size: 25),
          ),
        ),
        ListTile(
          onTap: () async {
            var image = await ImagePicker().pickImage(
              source: ImageSource.gallery,
              maxWidth: 1024,
              maxHeight: 1024,
              imageQuality: 85,
            );
            if (image != null) {
              File cleanImage = await getSanitizedFileFromXFile(image);
              context.read<UserProvider>().uploadImage(image: cleanImage);
            }
            Navigator.pop(context);
          },
          leading: Icon(Icons.photo_library_outlined),
          title: Text('Choose from Library', style: TextStyle(fontSize: 16)),
        ),
        ListTile(
          onTap: () async {
            var image = await ImagePicker().pickImage(
              source: ImageSource.camera,
              maxWidth: 1024,
              maxHeight: 1024,
              imageQuality: 85,
            );
            if (image != null) {
              File cleanImage = await getSanitizedFileFromXFile(image);
              context.read<UserProvider>().uploadImage(image: cleanImage);
            }
            Navigator.pop(context);
          },
          leading: Icon(Icons.camera_alt_outlined),
          title: Text('Take Photo', style: TextStyle(fontSize: 16)),
        ),
      ],
    );
  }
}

Future<File> getSanitizedFileFromXFile(XFile xFile) async {
  final String directoryPath = p.dirname(xFile.path);
  final String fileExtension = p.extension(xFile.path);
  final String newFileName =
      "${DateTime.now().millisecondsSinceEpoch}$fileExtension";
  final String newPath = p.join(directoryPath, newFileName);
  File originalFile = File(xFile.path);
  return await originalFile.copy(newPath);
}
