import 'dart:io';

import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';

class AdminProvider extends ChangeNotifier {
  List clients = [];
  bool isloading = false;
  File? picture;

  Future<void> fetchClients() async {
    isloading = true;
    notifyListeners();
    try {
      final token = await getToken();
      if (token == null) {
        print('no token found');
      } else {
        clients = await ApiService().getClients(token);
      }
    } catch (e) {
      print('Error in fetching Clients: $e');
    } finally {
      isloading = false;
      notifyListeners();
    }
  }

  int clientsCount() {
    return clients.length;
  }

  // ====from gallery=========
  // Future<void> pickFromGallery() async {
  //   final img = await _picker.pickImage(source: ImageSource.gallery);
  //   if (img != null) {
  //     picture = img;
  //     notifyListeners();
  //   }
  // }

  // from camera==============
  // Future<void> pickFromCamera() async {
  //   final img = await _picker.pickImage(source: ImageSource.camera);
  //   if (img != null) {
  //     picture = img;
  //     notifyListeners();
  //   }
  // }

  void clearImage() {
    picture = null;
    notifyListeners();
  }

  void uploadImage({required File image}) {
    picture = image;
    notifyListeners();
  }
}
