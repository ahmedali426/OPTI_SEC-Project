// ignore_for_file: avoid_print

import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:optisecapp/Auth/pages/login_page.dart';
import 'package:optisecapp/main.dart';
import 'package:optisecapp/services/firebase_notifications.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ApiService {
  String baseUrl = "https://opti-sec.runasp.net";
  bool isRefreshing = false;
  Future<String?>? refreshFuture;
  Future<String?> login({
    required String email,
    required String password,
  }) async {
    final url = Uri.parse('$baseUrl/Auth/login');
    final body = jsonEncode({"email": email, "password": password});

    try {
      final response = await http.post(
        url,
        headers: {"Content-Type": "application/json"},
        body: body,
      );

      if (response.statusCode == 200 ||
          response.statusCode == 201 ||
          response.statusCode == 204) {
        final data = jsonDecode(response.body);

        final token = data['token'];
        print('Response body : $data');
        if (token != null) {
          final prefs = await SharedPreferences.getInstance();
          await prefs.setString('auth_token', token);
          await prefs.setString('refresh_token', data['refreshToken']);
          await prefs.setString('userId', data['id']);
          await prefs.setString('email', data['email']);
          await prefs.setString('fName', data['fName']);
          await prefs.setString('lName', data['lName']);
          await prefs.setString('roles', jsonEncode(data['roles']));
          await prefs.setBool('isLogged', true);
          final tokenExpiry = DateTime.now().add(
            Duration(seconds: data['expireIn']),
          );
          await prefs.setString('token_expiry', tokenExpiry.toIso8601String());
          await prefs.setString(
            'refresh_token_expiry',
            data['refreshTokenExpiration'],
          );

          return token;
        } else {
          print("Token is null, login failed");
        }
      } else {
        print("Login failed with status code: ${response.statusCode}");
      }
    } catch (e) {
      print("Exception during login: $e");
    }

    return null;
  }

  // ========== logout=======
  Future<bool> signOut() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final token = prefs.getString('auth_token');
      final refreshToken = prefs.getString('refresh_token');
      var response = await http.post(
        Uri.parse('$baseUrl/Auth/revoke-refresh-token'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'token': token, 'refreshToken': refreshToken}),
      );
      if (response.statusCode == 200 ||
          response.statusCode == 204 ||
          response.statusCode == 201) {
        await clearSession(prefs);
        return true;
      } else {
        print("Status: ${response.statusCode} , Body:${response.body}");
        return false;
      }
    } catch (e) {
      print('Error in Signout: $e');
      return false;
    }
  }

  // ====== forget password ==========
  Future<bool> forgetPassword({required String email}) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/Auth/forget-password'),
        body: jsonEncode({'email': email}),
        headers: {'Content-Type': 'application/json'},
      );
      if (response.statusCode == 200 || response.statusCode == 204) {
        print('successfully send email');
        return true;
      } else {
        print(
          'Error: status code:${response.statusCode}, body:${response.body}',
        );
        return false;
      }
    } catch (e) {
      throw Exception('Error during sending email: $e');
    }
  }

  // ======reset password ======
  Future<bool> resetPassword({required Map<String, dynamic> data}) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/Auth/reset-password'),
        body: jsonEncode({
          'email': data['email'],
          'code': data['code'],
          'newPassword': data['password'],
        }),
        headers: {'Content-Type': 'application/json'},
      );
      if (response.statusCode == 200 || response.statusCode == 204) {
        print('successfully reseting password');
        return true;
      } else {
        print(
          'Error: status code ${response.statusCode} ,body:${response.body}',
        );
        return false;
      }
    } catch (e) {
      throw Exception('Error in resetting password :$e');
    }
  }

  //========get clients==========
  Future<List<dynamic>> getClients(String token) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.get(
        Uri.parse('$baseUrl/api/Clients'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 201 ||
        response.statusCode == 204) {
      return jsonDecode(response.body);
    } else {
      throw Exception("Failed to load clients");
    }
  }

  // ======== Get Client by ID ========
  Future<Map<String, dynamic>> getClientById(String token, int clientId) async {
    final url = Uri.parse('$baseUrl/api/Clients/$clientId');

    final response = await http.get(
      url,
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      },
    );

    if (response.statusCode == 200) {
      return json.decode(response.body);
    } else {
      throw Exception('Failed to load client');
    }
  }

  // =========update client==========
  Future<bool> updateClient({
    required String token,
    required int clientId,
    required Map<String, String> body,
    required File? image,
  }) async {
    try {
      var uri = Uri.parse('$baseUrl/api/Clients/$clientId');

      var request = http.MultipartRequest('PUT', uri);

      request.headers['Authorization'] = 'Bearer $token';

      request.fields['Name'] = body['name']!;
      request.fields['Email'] = body['email']!;
      request.fields['UserName'] = body['userName']!;
      request.fields['PhoneNumber'] = body['phoneNumber']!;

      if (image != null) {
        request.files.add(
          await http.MultipartFile.fromPath('Image', image.path),
        );
      }
      var response = await request.send();

      if (response.statusCode == 200 ||
          response.statusCode == 204 ||
          response.statusCode == 201) {
        return true;
      } else {
        print("Status: ${response.statusCode}");
        final respStr = await response.stream.bytesToString();
        print("Body: $respStr");
        return false;
      }
    } catch (e) {
      print("Exception: $e");
      return false;
    }
  }

  // ====== Delete client =========
  Future<bool> deleteClient({
    required String token,
    required int clientId,
  }) async {
    final url = Uri.parse('$baseUrl/api/Clients/$clientId');
    final response = await http.delete(
      url,
      headers: {"Authorization": "Bearer $token"},
    );

    if (response.statusCode == 200 || response.statusCode == 204) {
      return true;
    } else {
      print('Failed to delete client: ${response.body}');
      return false;
    }
  }

  // ============= add Client ===========
  Future<String> addClient({
    required String token,
    required Map<String, dynamic> clientData,
    required File image,
  }) async {
    try {
      Future<http.StreamedResponse> sendRequest(String activeToken) async {
        var uri = Uri.parse('$baseUrl/api/Clients');
        var request = http.MultipartRequest('POST', uri);
        request.headers['Authorization'] = 'Bearer $activeToken';
        request.fields['FName'] = clientData['fname'];
        request.fields['LName'] = clientData['lname'];
        request.fields['Email'] = clientData['email'];
        request.fields['UserName'] = clientData['username'];
        request.fields['Password'] = clientData['password'];
        request.fields['PhoneNumber'] = clientData['phone'];
        request.files.add(
          await http.MultipartFile.fromPath('Image', image.path),
        );
        return await request.send();
      }

      var response = await sendRequest(token);
      if (response.statusCode == 200 || response.statusCode == 204) {
        return 'success';
      } else if (response.statusCode == 401) {
        print('Token expired, trying to refresh.');
        final newToken = await refreshToken();
        if (newToken != null) {
          print('Successfully Refreshing Token, sending request');
          response = await sendRequest(newToken);

          if (response.statusCode == 200 || response.statusCode == 204) {
            return 'success';
          } else {
            final respStr = await response.stream.bytesToString();
            print("Status: ${response.statusCode} , Body:$respStr");
            final error = jsonDecode(respStr)['errors'];
            return error[0]['description'];
          }
        } else {
          return 'session expired, please login again';
        }
      } else {
        final respStr = await response.stream.bytesToString();
        print("Status: ${response.statusCode} , Body:$respStr");
        final error = jsonDecode(respStr)['errors'];
        return error[0]['description'];
      }
    } catch (e) {
      return 'Something went wrong, please try again';
    }
  }

  // =============add user=============
  Future<bool> addUser({
    required String token,
    required Map<String, dynamic> userData,
    required File image,
  }) async {
    try {
      Future<http.StreamedResponse> sendRequest(String activeToken) async {
        var uri = Uri.parse('$baseUrl/api/Members');
        var request = http.MultipartRequest('POST', uri);
        print(userData);
        request.headers['Authorization'] = 'Bearer $activeToken';
        request.fields['FName'] = userData['fname'];
        request.fields['LName'] = userData['lname'];
        request.fields['UserName'] = userData['username'];
        request.fields['Phone'] = userData['phone'];
        request.files.add(
          await http.MultipartFile.fromPath('Image', image.path),
        );
        return await request.send();
      }

      var response = await sendRequest(token);

      if (response.statusCode == 200 ||
          response.statusCode == 204 ||
          response.statusCode == 201) {
        return true;
      } else if (response.statusCode == 401) {
        print('Token expired, trying to refresh.');
        final newToken = await refreshToken();

        if (newToken != null) {
          print('Successfully refreshing token, retrying request');
          response = await sendRequest(newToken);
          return response.statusCode == 200 ||
              response.statusCode == 204 ||
              response.statusCode == 201;
        } else {
          print('Failed refreshing token, session expired');
          return false;
        }
      } else {
        final respStr = await response.stream.bytesToString();
        print("Status: ${response.statusCode}, Body: $respStr");
        return false;
      }
    } catch (e) {
      print('Exception during adding user: $e');
      return false;
    }
  }

  // ========= get users ==========
  Future<List<dynamic>> getUsers(String token) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.get(
        Uri.parse('$baseUrl/api/Members'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 204 ||
        response.statusCode == 201) {
      return jsonDecode(response.body);
    } else {
      throw Exception("Failed to load Users");
    }
  }

  // ===========get Authorized Users==========
  Future<List<dynamic>> getAuthorizedUsers(String token) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.get(
        Uri.parse('$baseUrl/api/AccessLogs/authorized'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 204 ||
        response.statusCode == 201) {
      return jsonDecode(response.body);
    } else {
      throw Exception("Failed to load Authorized Users");
    }
  }

  // ===========get Unauthorized Users=========
  Future<List<dynamic>> getUnauthorizedUsers(String token) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.get(
        Uri.parse('$baseUrl/api/AccessLogs/unauthorized'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 204 ||
        response.statusCode == 201) {
      return jsonDecode(response.body);
    } else {
      throw Exception("Failed to load Unauthorized Users");
    }
  }

  // ========= get user by id ==========
  Future<Map<String, dynamic>> getUserById(String token, int userId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/Members/$userId'),
      headers: {'Authorization': 'Bearer $token'},
    );

    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    } else {
      throw Exception("Failed to load user");
    }
  }

  // ========= update user ==========
  Future<bool> updateUser({
    required String token,
    required int userId,
    required Map<String, String> body,
    required File? image,
  }) async {
    try {
      var request = http.MultipartRequest(
        'PUT',
        Uri.parse('$baseUrl/api/Members/$userId'),
      );

      request.headers['Authorization'] = 'Bearer $token';

      request.fields['Name'] = body['name']!;
      request.fields['UserName'] = body['userName']!;
      request.fields['Phone'] = body['phone']!;

      if (image != null) {
        request.files.add(
          await http.MultipartFile.fromPath('Image', image.path),
        );
      }

      var response = await request.send();

      return response.statusCode == 200 || response.statusCode == 204;
    } catch (e) {
      print("Error updating user: $e");
      return false;
    }
  }

  // ========= delete user ==========
  Future<bool> deleteUser({required String token, required int userId}) async {
    final response = await http.delete(
      Uri.parse('$baseUrl/api/Members/$userId'),
      headers: {'Authorization': 'Bearer $token'},
    );

    return response.statusCode == 200 || response.statusCode == 204;
  }

  // ====== get Gates =============
  Future<List<dynamic>> getGates({required String token}) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.get(
        Uri.parse('$baseUrl/api/Gates'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print(
          'Failed Refreshing token, session expired!, token:$token, newtoken:$newToken',
        );
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 201 ||
        response.statusCode == 204) {
      print('status code: ${response.statusCode}, body:${response.body}');
      final token = await getToken();
      print('current token:$token');
      return jsonDecode(response.body);
    } else {
      throw Exception("Failed to load Gates");
    }
  }

  // ====== get Gate by Id =============
  Future<Map<String, dynamic>> getGateById({
    required String token,
    required int gateId,
  }) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.get(
        Uri.parse('$baseUrl/api/Gates/$gateId'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 204 ||
        response.statusCode == 201) {
      return jsonDecode(response.body);
    } else {
      throw Exception("Failed to load Gate for id:$gateId");
    }
  }

  Future<String?> addGate({
    required String token,
    required Map<String, dynamic> gateData,
  }) async {
    Future<http.Response> sendRequest(String activeToken) async {
      var uri = Uri.parse('$baseUrl/api/Gates');
      var response = await http.post(
        uri,
        body: jsonEncode(gateData),
        headers: {
          'Authorization': 'Bearer $activeToken',
          'Content-Type': 'application/json',
        },
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 200 ||
        response.statusCode == 201 ||
        response.statusCode == 204) {
      // final decodedData = jsonDecode(response.body);
      return 'success';
    } else if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();

      if (newToken != null) {
        response = await sendRequest(newToken);
        if (response.statusCode == 200 ||
            response.statusCode == 201 ||
            response.statusCode == 204) {
          // final decodedData = jsonDecode(response.body);
          return 'success';
        }
      } else {
        return null;
      }
    } else {
      print("Status: ${response.statusCode} , Body:${response.body}");
      try {
        final decoded = jsonDecode(response.body);
        if (decoded != null &&
            decoded['errors'] != null &&
            (decoded['errors'] as List).isNotEmpty) {
          return decoded['errors'][0]['description'];
        } else {
          return null;
        }
      } catch (e) {
        print("Parsing error body failed: $e");
      }

      return "Server Error: ${response.statusCode}";
    }
    return null;
  }

  // ========= update gate ==========
  Future<bool> updateGate({
    required String token,
    required int gateId,
    required Map<String, String> body,
  }) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.put(
        Uri.parse('$baseUrl/api/Gates/$gateId'),
        body: jsonEncode(body),
        headers: {
          'Authorization': 'Bearer $activeToken',
          'Content-Type': 'application/json',
        },
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
        return false;
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 204 ||
        response.statusCode == 201) {
      return true;
    } else {
      print(
        "Failed to update Gate. Status: ${response.statusCode}, Body: ${response.body}",
      );
      return false;
    }
  }

  // ====== Delete gate =========
  Future<bool> deleteGate({required String token, required int gateId}) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.delete(
        Uri.parse('$baseUrl/api/Gates/$gateId'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
        return false;
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 204 ||
        response.statusCode == 201) {
      return true;
    } else {
      print(
        "Failed to update Gate. Status: ${response.statusCode}, Body: ${response.body}",
      );
      return false;
    }
  }

  // Gates status
  Future<List<dynamic>> geyGatesStatus({required String token}) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.get(
        Uri.parse('$baseUrl/api/MobileCommands/gates/status'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print(
          'Failed Refreshing token, session expired!, token:$token, newtoken:$newToken',
        );
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 201 ||
        response.statusCode == 204) {
      print('status code: ${response.statusCode}, body:${response.body}');
      // final token = await getToken();
      // print('current token:$token');
      return jsonDecode(response.body);
    } else {
      throw Exception("Failed to load Gates");
    }
  }

  // stop buzzer
  Future<bool> stopBuzzer({required String token, required int gateId}) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.post(
        Uri.parse('$baseUrl/api/MobileCommands/gates/$gateId/stop-buzzer'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
        return false;
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 204 ||
        response.statusCode == 201) {
      return true;
    } else {
      print(
        "Failed to stop buzzer. Status: ${response.statusCode}, Body: ${response.body}",
      );
      return false;
    }
  }

  // trigger gate
  Future<bool> triggerGate({required String token, required int gateId}) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.post(
        Uri.parse('$baseUrl/api/MobileCommands/gates/$gateId/open'),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
        return false;
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 204 ||
        response.statusCode == 201) {
      return true;
    } else {
      print(
        "Failed to open Gate. Status: ${response.statusCode}, Body: ${response.body}",
      );
      return false;
    }
  }

  Future<bool> addFCMToken({
    required String token,
    required Map<String, dynamic> data,
  }) async {
    Future<http.Response> sendRequest(String activeToken) async {
      var uri = Uri.parse('$baseUrl/api/MobileCommands/register-fcm-token');
      var response = await http.post(
        uri,
        body: jsonEncode(data),
        headers: {
          'Authorization': 'Bearer $activeToken',
          'Content-Type': 'application/json',
        },
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 200 ||
        response.statusCode == 201 ||
        response.statusCode == 204) {
      return true;
    } else if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();

      if (newToken != null) {
        response = await sendRequest(newToken);
        if (response.statusCode == 200 ||
            response.statusCode == 201 ||
            response.statusCode == 204) {
          return true;
        }
      } else {
        return false;
      }
    } else {
      print("Status: ${response.statusCode} , Body:${response.body}");
      final decoded = jsonDecode(response.body);
      return decoded['errors'][0]['description'];
    }
    return false;
  }

  // resolve emergency access
  Future<bool> resolveEmergency({
    required String token,
    required int emergencyId,
  }) async {
    Future<http.Response> sendRequest(String activeToken) async {
      final response = await http.post(
        Uri.parse(
          '$baseUrl/api/MobileCommands/emergencies/$emergencyId/resolve',
        ),
        headers: {'Authorization': 'Bearer $activeToken'},
      );
      return response;
    }

    var response = await sendRequest(token);
    if (response.statusCode == 401) {
      print('Token expired, trying to refresh.');
      final newToken = await refreshToken();
      if (newToken != null) {
        print('Successfully Refreshing Token, sending request');
        response = await sendRequest(newToken);
      } else {
        print('Failed Refreshing token, session expired!');
        return false;
      }
    }
    if (response.statusCode == 200 ||
        response.statusCode == 204 ||
        response.statusCode == 201) {
      return true;
    } else {
      print(
        "Failed to resolve emergency access. Status: ${response.statusCode}, Body: ${response.body}",
      );
      return false;
    }
  }

  // ================== refresh token =============
  Future<String?> refreshToken() async {
    if (isRefreshing && refreshFuture != null) {
      print(
        '🔄 Refresh is already in progress, waiting for the same future...',
      );
      return refreshFuture;
    }

    isRefreshing = true;
    refreshFuture = _executeRefreshPipeline();
    return refreshFuture;
  }

  Future<String?> _executeRefreshPipeline() async {
    try {
      final prefs = await SharedPreferences.getInstance();

      final String? currentToken = prefs.getString('auth_token');
      final String? tokenExpiryStr = prefs.getString('token_expiry');

      if (tokenExpiryStr != null) {
        final DateTime expiry = DateTime.parse(tokenExpiryStr);
        if (expiry.isAfter(DateTime.now())) {
          print(
            '💡 Parallel Request Logic: Token was already refreshed by another call. Salvaging session!',
          );
          return currentToken;
        }
      }

      var oldRefreshToken = prefs.getString('refresh_token');
      var uri = Uri.parse('$baseUrl/Auth/refresh');

      if (oldRefreshToken == null || oldRefreshToken.isEmpty) {
        print('❌ No refresh token found in SharedPreferences.');
        return null;
      }

      print('🚀 Sending Refresh Token Request to Server...');
      final response = await http.post(
        uri,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
        body: jsonEncode({
          'token': currentToken,
          'refreshToken': oldRefreshToken,
        }),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        final data = jsonDecode(response.body);

        String newToken = data['token'];
        String newRefreshToken = data['refreshToken'];

        await prefs.setString('auth_token', newToken);
        await prefs.setString('refresh_token', newRefreshToken);

        if (data['expireIn'] != null) {
          final tokenExpiry = DateTime.now().add(
            Duration(seconds: data['expireIn']),
          );
          await prefs.setString('token_expiry', tokenExpiry.toIso8601String());
        }
        if (data['refreshTokenExpiration'] != null) {
          await prefs.setString(
            'refresh_token_expiry',
            data['refreshTokenExpiration'],
          );
        }

        print('✅ Token refreshed successfully!');
        return newToken;
      }

      print('❌ Server rejected refresh token. Status: ${response.statusCode}');
      print('❌ Server Error Message: ${response.body}');

      final String? finalCheckToken = prefs.getString('auth_token');
      if (finalCheckToken != null && finalCheckToken != currentToken) {
        print(
          '✅ Tunnels Aligned: Fresh token detected in SharedPreferences written by a parallel request. Session salvaged!',
        );
        return finalCheckToken;
      }

      print(
        '🚨 Session completely expired or broken. Redirecting to login page...',
      );
      await clearSession(prefs);

      navigatorKey.currentState?.pushAndRemoveUntil(
        MaterialPageRoute(builder: (_) => const LoginPage()),
        (route) => false,
      );

      return null;
    } catch (e) {
      print('💥 Exception during refresh token: $e');
      return null;
    } finally {
      await Future.delayed(const Duration(milliseconds: 300));
      isRefreshing = false;
      refreshFuture = null;
    }
  }

  Future<void> clearSession(SharedPreferences prefs) async {
    await prefs.remove('auth_token');
    await prefs.remove('refresh_token');
    await prefs.remove('token_expiry');
    await prefs.remove('refresh_token_expiry');
    await prefs.remove('userId');
    await prefs.remove('email');
    await prefs.remove('fName');
    await prefs.remove('lName');
    await prefs.remove('roles');
    await prefs.setBool('isLogged', false);
  }
}

// ----------------------------------------
Future<String?> getToken() async {
  final prefs = await SharedPreferences.getInstance();
  return prefs.getString('auth_token');
}
