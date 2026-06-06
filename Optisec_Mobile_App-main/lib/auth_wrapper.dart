import 'package:flutter/material.dart';
import 'package:optisecapp/Admin/Pages/admin_home_page.dart';
import 'package:optisecapp/Auth/pages/login_page.dart';
import 'package:optisecapp/Auth/provider/auth_provider.dart';
import 'package:optisecapp/Client/Pages/client_home_page.dart';
import 'package:provider/provider.dart';

class AuthWrapper extends StatelessWidget {
  const AuthWrapper({super.key});

  @override
  Widget build(BuildContext context) {
    return Consumer<AuthProvider>(
      builder: (context, auth, child) {
        final user = auth.user;
        if (user['isLogged'] != true) {
          return LoginPage();
        }
        List roles = user['roles'] ?? [];
        if (roles.contains('Admin')) {
          return AdminHomePage();
        } else {
          return ClientHomePage();
        }
      },
    );
  }
}
