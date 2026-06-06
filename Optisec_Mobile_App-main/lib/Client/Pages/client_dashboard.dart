import 'package:flutter/material.dart';
import 'package:optisecapp/Auth/provider/auth_provider.dart';
import 'package:optisecapp/Client/Pages/users_list.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:optisecapp/CustomWidgets/custom_dashboard.dart';
import 'package:optisecapp/services/firebase_notifications.dart';
import 'package:provider/provider.dart';

class ClientDashBoard extends StatefulWidget {
  const ClientDashBoard({super.key});

  @override
  State<ClientDashBoard> createState() => _ClientDashBoardState();
}

class _ClientDashBoardState extends State<ClientDashBoard> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<UserProvider>().fetchUsers();
      context.read<UserProvider>().fetchGates();
      FirebaseNotifications().checkAndRequestPermissions(context);
    });
  }

  @override
  Widget build(BuildContext context) {
    final user = context.watch<AuthProvider>().user;
    return Customdashboard(
      username: user['fname'] ?? '',
      page: UsersList(),
      isClient: true,
    );
  }
}
