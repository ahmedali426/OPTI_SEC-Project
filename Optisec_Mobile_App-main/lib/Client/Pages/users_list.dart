import 'package:flutter/material.dart';
import 'package:optisecapp/Client/Pages/user_info.dart';
import 'package:optisecapp/CustomWidgets/custom_card.dart';
import 'package:optisecapp/CustomWidgets/delete_dialog.dart';
import 'package:provider/provider.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:optisecapp/Api/api_service.dart';

class UsersList extends StatefulWidget {
  const UsersList({super.key});

  @override
  State<UsersList> createState() => _UserslistState();
}

class _UserslistState extends State<UsersList> {
  List filteredUsers = [];
  List selectedUsers = [];

  String searchText = "";

  bool get isSelect => selectedUsers.isNotEmpty;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<UserProvider>().fetchUsers();
    });
  }

  void handleSearch(String input, List users) {
    setState(() {
      searchText = input;

      if (input.isEmpty) {
        filteredUsers = [];
      } else {
        filteredUsers = users.where((user) {
          final name = user['name'] ?? '';
          return name.toLowerCase().contains(input.toLowerCase());
        }).toList();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final userProvider = context.watch<UserProvider>();
    final users = userProvider.users;

    final displayedUsers = searchText.isEmpty ? users : filteredUsers;

    return Scaffold(
      appBar: AppBar(
        leading: Container(
          margin: const EdgeInsets.only(left: 15.0),
          child: IconButton(
            onPressed: () {
              if (isSelect) {
                setState(() {
                  selectedUsers.clear();
                });
              } else {
                Navigator.pop(context);
              }
            },
            icon: Icon(isSelect ? Icons.close : Icons.arrow_back_ios),
          ),
        ),
        title: Text(
          isSelect ? '${selectedUsers.length}' : 'USERS',
          style: Theme.of(context).textTheme.bodyLarge,
        ),
        centerTitle: isSelect ? false : true,
        actions: isSelect
            ? [
                Container(
                  margin: EdgeInsets.only(right: 10),
                  child: IconButton(
                    onPressed: () async {
                      final confirm = await deleteDialog(context);
                      if (!confirm) return;

                      final token = await getToken();
                      if (token == null) return;

                      for (var user in selectedUsers) {
                        await ApiService().deleteUser(
                          token: token,
                          userId: user['id'],
                        );
                      }

                      setState(() {
                        selectedUsers.clear();
                      });

                      userProvider.fetchUsers();
                    },
                    icon: Icon(Icons.delete_outline, color: Colors.grey[200]!),
                  ),
                ),
              ]
            : null,
      ),
      body: Padding(
        padding: const EdgeInsets.all(12),
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
              child: userProvider.isUsersLoading
                  ? Center(child: CircularProgressIndicator())
                  : displayedUsers.isEmpty && searchText.isNotEmpty
                  ? Center(child: Text("No users found"))
                  : ListView.builder(
                      itemCount: displayedUsers.length,
                      itemBuilder: (context, idx) {
                        final user = displayedUsers[idx];

                        return GestureDetector(
                          onLongPress: () {
                            if (!selectedUsers.contains(user)) {
                              setState(() {
                                selectedUsers.add(user);
                              });
                            }
                          },
                          onTap: () async {
                            if (isSelect) {
                              setState(() {
                                if (selectedUsers.contains(user)) {
                                  selectedUsers.remove(user);
                                } else {
                                  selectedUsers.add(user);
                                }
                              });
                            } else {
                              final result = await Navigator.push(
                                context,
                                MaterialPageRoute(
                                  builder: (context) =>
                                      UserInfo(userId: user['id']),
                                ),
                              );

                              if (result == true) {
                                userProvider.fetchUsers();
                              }
                            }
                          },
                          child: CustomCard(
                            name: user['name'] ?? 'No Name',
                            img: user['imageUrl'] ?? '',
                            selected: selectedUsers.contains(user),
                          ),
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
