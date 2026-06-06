import 'package:flutter/material.dart';
import 'package:optisecapp/Admin/Pages/clients_list.dart';
import 'package:optisecapp/Admin/provider/admin_provider.dart';
import 'package:optisecapp/Auth/provider/auth_provider.dart';
import 'package:optisecapp/CustomWidgets/custom_dashboard.dart';
import 'package:provider/provider.dart';

class AdminDashBoard extends StatefulWidget {
  const AdminDashBoard({super.key});

  @override
  State<AdminDashBoard> createState() => _AdminDashBoardState();
}

class _AdminDashBoardState extends State<AdminDashBoard> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<AdminProvider>().fetchClients();
    });
  }

  @override
  Widget build(BuildContext context) {
    final user = context.watch<AuthProvider>().user;
    return Customdashboard(
      username: '${user['fname'] ?? ''}',
      page: ClientsList(showBackArrow: true),
      isClient: false,
    );
  }
}
