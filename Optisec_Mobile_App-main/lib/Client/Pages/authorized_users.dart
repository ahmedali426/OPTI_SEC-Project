import 'package:flutter/material.dart';
import 'package:optisecapp/CustomWidgets/custom_users.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:provider/provider.dart';

class Authorizedusers extends StatefulWidget {
  const Authorizedusers({super.key});

  @override
  State<Authorizedusers> createState() => _AuthorizedusersState();
}

class _AuthorizedusersState extends State<Authorizedusers> {
  List<dynamic> filteredUsers = [];
  String searchText = "";

  @override
  void initState() {
    super.initState();

    Future.microtask(() {
      context.read<UserProvider>().fetchAuthorizedUsers();
    });
  }

  void handleSearch(String value, List<dynamic> users) {
    setState(() {
      searchText = value;

      if (value.isEmpty) {
        filteredUsers = [];
      } else {
        filteredUsers = users.where((user) {
          final name = user['name'] ?? '';
          return name.toLowerCase().contains(value.toLowerCase());
        }).toList();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final userprovider = context.watch<UserProvider>();
    final users = userprovider.authorizedUsers;

    final displayedUsers = searchText.isEmpty ? users : filteredUsers;
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'AUTHORIZED USERS',
          style: Theme.of(context).textTheme.bodyLarge,
        ),
        centerTitle: true,
      ),
      body: userprovider.isAuthUsersLoading
          ? Center(child: CircularProgressIndicator())
          : users.isEmpty
          ? Center(child: Text('No users found'))
          : Padding(
              padding: const EdgeInsets.fromLTRB(12, 12, 12, 0),
              child: Column(
                children: [
                  TextField(
                    onChanged: (value) => handleSearch(value, users),
                    style: TextStyle(fontSize: 18),
                    decoration: InputDecoration(
                      filled: true,
                      fillColor: Theme.of(context).cardTheme.color,
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: BorderSide.none,
                      ),
                      enabledBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: BorderSide.none,
                      ),
                      focusedBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: BorderSide(color: Colors.grey),
                      ),
                      hintText: "Search",
                      prefixIcon: Icon(Icons.search),
                    ),
                  ),

                  SizedBox(height: 10),
                  Expanded(
                    child: userprovider.isAuthUsersLoading
                        ? Center(child: CircularProgressIndicator())
                        : displayedUsers.isEmpty && searchText.isNotEmpty
                        ? Center(child: Text("No users found"))
                        : ListView.builder(
                            itemCount: displayedUsers.length,
                            itemBuilder: (context, idx) {
                              final data = displayedUsers[idx];

                              return Customusers(
                                name: data['name'] ?? '',
                                img: data['imageUrl'] ?? '',
                                date: data['dateOnly'] ?? '',
                                time: data['timeOnly'] ?? '',
                                isAuthorized: true,
                              );
                            },
                          ),
                  ),
                ],
              ),
            ),
    );
  }
}
