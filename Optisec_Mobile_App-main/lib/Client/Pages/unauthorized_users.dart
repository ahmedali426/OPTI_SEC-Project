import 'package:flutter/material.dart';
import 'package:optisecapp/CustomWidgets/custom_users.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:provider/provider.dart';

class UnauthorizedUsers extends StatefulWidget {
  const UnauthorizedUsers({super.key});

  @override
  State<UnauthorizedUsers> createState() => _UnauthorizedUsersState();
}

class _UnauthorizedUsersState extends State<UnauthorizedUsers> {
  @override
  void initState() {
    super.initState();

    Future.microtask(() {
      context.read<UserProvider>().fetchUnAuthorizedUsers();
    });
  }

  @override
  Widget build(BuildContext context) {
    final userProvider = context.watch<UserProvider>();
    final users = userProvider.unauthorizedUsers;

    return Scaffold(
      appBar: AppBar(
        title: Text(
          'UNAUTHORIZED USERS',
          style: Theme.of(context).textTheme.bodyLarge,
        ),
        centerTitle: true,
      ),
      body: Padding(
        padding: const EdgeInsets.fromLTRB(12, 12, 12, 0),
        child: userProvider.isUnAuthUsersLoading
            ? Center(child: CircularProgressIndicator())
            : users.isEmpty
            ? Center(child: Text("No users found"))
            : ListView.builder(
                itemCount: users.length,
                itemBuilder: (context, idx) {
                  final data = users[idx];

                  return Customusers(
                    name: data['name'] ?? '',
                    img: data['imageUrl'] ?? '',
                    date: data['dateOnly'] ?? '',
                    time: data['timeOnly'] ?? '',
                    isAuthorized: false,
                  );
                },
              ),
      ),
    );
  }
}
