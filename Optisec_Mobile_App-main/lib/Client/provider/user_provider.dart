import 'dart:io';

import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';

class UserProvider extends ChangeNotifier {
  List users = [];
  List authorizedUsers = [];
  List unauthorizedUsers = [];
  List gates = [];
  List gatesStatus = [];
  File? picture;
  bool isUsersLoading = false;
  bool isAuthUsersLoading = false;
  bool isUnAuthUsersLoading = false;
  bool isGatesLoading = false;
  bool isGatesStatusLoading = false;

  // final ImagePicker _picker = ImagePicker();

  // ======= Fetch all Users ======
  Future<void> fetchUsers() async {
    isUsersLoading = true;
    notifyListeners();

    try {
      final token = await getToken();

      if (token == null) {
        print('No token found');
      } else {
        users = await ApiService().getUsers(token);
      }
    } catch (e) {
      print('Error in fetching Users: $e');
    } finally {
      isUsersLoading = false;
      notifyListeners();
    }
  }
  // =======fetch Authorized Users=======

  Future<void> fetchAuthorizedUsers() async {
    isAuthUsersLoading = true;
    notifyListeners();

    try {
      final token = await getToken();

      if (token != null) {
        authorizedUsers = await ApiService().getAuthorizedUsers(token);
      }
    } catch (e) {
      print(e);
    }

    isAuthUsersLoading = false;
    notifyListeners();
  }

  // =======fetch UnAuthorized Users=======
  Future<void> fetchUnAuthorizedUsers() async {
    isUnAuthUsersLoading = true;
    notifyListeners();

    try {
      final token = await getToken();

      if (token != null) {
        unauthorizedUsers = await ApiService().getUnauthorizedUsers(token);
      }
    } catch (e) {
      print(e);
    }

    isUnAuthUsersLoading = false;
    notifyListeners();
  }

  // =======fetch Gates=======
  Future<void> fetchGates() async {
    isGatesLoading = true;
    notifyListeners();
    try {
      final token = await getToken();
      if (token != null) {
        gates = await ApiService().getGates(token: token);
        print(gates);
      }
    } catch (e) {
      print(e);
    }
    isGatesLoading = false;
    notifyListeners();
  }
  // fetch gates status
  Future<void> fetchGatesStatus() async {
    isGatesStatusLoading = true;
    notifyListeners();
    try {
      final token = await getToken();
      if (token != null) {
        gatesStatus = await ApiService().geyGatesStatus(token: token);
        print(gatesStatus);
      }
    } catch (e) {
      print(e);
    }
    isGatesStatusLoading = false;
    notifyListeners();
  }

  // ====== Count users ========
  int usersCount() {
    return users.length;
  }

  // ====== Count gates ========
  int gatesCount() {
    return gates.length;
  }

  // ========Image ========

  // Future<void> pickFromGallery() async {
  //   final img = await _picker.pickImage(source: ImageSource.gallery);
  //   if (img != null) {
  //     picture = img;
  //     notifyListeners();
  //   }
  // }

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
