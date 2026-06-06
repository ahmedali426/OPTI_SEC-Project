import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AuthProvider extends ChangeNotifier {
  bool isHidden = true;
  Map<String, dynamic> user = {};
  bool isSwitched = true;
  Map<String, dynamic> data = {};
  void makePasswordHidden() {
    isHidden = !isHidden;
    notifyListeners();
  }

  void triggerNotifications() async {
    isSwitched = !isSwitched;
    final pref = await SharedPreferences.getInstance();
    final userId = await pref.getString('userId');
    if (userId != null) {
      await pref.setBool('isNotified_$userId', isSwitched);
    }
    notifyListeners();
  }

  // =============== load user data =======
  Future loadUserData() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      bool isLogged = prefs.getBool('isLogged') ?? false;
      if (isLogged == true) {
        user = {
          'isLogged': true,
          'auth_token': prefs.getString('auth_token'),
          'token_expiry': prefs.getString('token_expiry'),
          'refresh_token_expiry': prefs.getString('refresh_token_expiry'),
          'id': prefs.getString('userId'),
          'fname': prefs.getString('fName'),
          'lname': prefs.getString('lName'),
          'email': prefs.getString('email'),
          'roles': List<String>.from(
            jsonDecode(prefs.getString('roles') ?? '[]'),
          ),
        };
        final userId = prefs.getString('userId');
        if (userId != null) {
          isSwitched = prefs.getBool('isNotified_$userId') ?? true;
        }
      } else {
        user = {'isLogged': false};
      }
      print('Successfully user data is loaded');
      print(user);
      notifyListeners();
    } catch (e) {
      print('Error in loading user data: $e');
    }
  }

  void loadData({String? email, String? code, String? password}) {
    if (email != null) data['email'] = email;
    if (code != null) data['code'] = code;
    if (password != null) data['password'] = password;
    notifyListeners();
  }

  void setLoggedOut() async {
    final prefs = await SharedPreferences.getInstance();
    prefs.setBool('isLogged', false);
    notifyListeners(); 
  }
}
