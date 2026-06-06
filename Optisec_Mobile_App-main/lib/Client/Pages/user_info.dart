import 'package:flutter/material.dart';
import 'package:optisecapp/Client/Pages/edit_user.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:optisecapp/CustomWidgets/delete_dialog.dart';
import 'package:optisecapp/CustomWidgets/ok_dialog.dart';
import 'package:optisecapp/Api/api_service.dart';

class UserInfo extends StatefulWidget {
  final int userId;
  const UserInfo({super.key, required this.userId});

  @override
  State<UserInfo> createState() => _UserInfoState();
}

class _UserInfoState extends State<UserInfo> {
  late TextEditingController name;
  late TextEditingController username;
  late TextEditingController phone;

  final GlobalKey<FormState> formState = GlobalKey<FormState>();
  bool isLoading = true;

  String imageUrl = '';

  @override
  void initState() {
    super.initState();
    name = TextEditingController();
    username = TextEditingController();
    phone = TextEditingController();

    fetchUserData();
  }

  Future<void> fetchUserData() async {
    try {
      final token = await getToken();
      if (token == null) throw Exception("No token found");

      final user = await ApiService().getUserById(token, widget.userId);

      setState(() {
        name.text = user['name'] ?? '';
        username.text = user['userName'] ?? '';
        phone.text = user['phone'] ?? '';
        imageUrl = user['imageUrl'] ?? '';
        isLoading = false;
      });
    } catch (e) {
      print("Error fetching user: $e");
      setState(() => isLoading = false);
    }
  }

  @override
  void dispose() {
    name.dispose();
    username.dispose();
    phone.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        leading: Container(
          margin: const EdgeInsets.only(left: 15.0),
          child: IconButton(
            onPressed: () => Navigator.pop(context, true),
            icon: Icon(Icons.arrow_back_ios),
          ),
        ),
        actions: [
          IconButton(
            onPressed: () async {
              final result = await Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => EditUser(userId: widget.userId),
                ),
              );

              if (result == true) {
                fetchUserData();
              }
            },
            icon: Icon(Icons.edit),
          ),

          PopupMenuButton(
            icon: Icon(Icons.more_vert),
            padding: EdgeInsets.symmetric(horizontal: 15),
            color: CardTheme.of(context).color,
            onSelected: (value) async {
              if (value == 'delete') {
                final confirm = await deleteDialog(context);
                if (!confirm) return;

                final token = await getToken();
                if (token == null) return;

                final deleted = await ApiService().deleteUser(
                  token: token,
                  userId: widget.userId,
                );

                if (deleted) {
                  Navigator.pop(context, true);
                } else {
                  okDialog(
                    context,
                    content: 'Failed to delete user',
                    contentColor: Colors.red,
                  );
                }
              }
            },
            itemBuilder: (context) {
              return [
                PopupMenuItem(
                  value: 'delete',
                  child: ListTile(
                    iconColor: Colors.red,
                    leading: Icon(Icons.person_remove_alt_1_outlined),
                    title: Text('Delete User'),
                  ),
                ),
              ];
            },
          ),
        ],
        title: Text('USER INFORMATION'),
        titleTextStyle: TextStyle(fontSize: 25),
      ),

      body: isLoading
          ? Center(child: CircularProgressIndicator())
          : Padding(
              padding: const EdgeInsets.only(top: 80),
              child: ListView(
                children: [
                  Column(
                    children: [
                      CircleAvatar(
                        radius: 65,
                        backgroundImage: imageUrl.isNotEmpty
                            ? NetworkImage(imageUrl)
                            : AssetImage('images/img1.jpg') as ImageProvider,
                      ),
                      SizedBox(height: 10),
                      Text(
                        name.text,
                        style: TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                      SizedBox(height: 15),
                    ],
                  ),

                  Padding(
                    padding: const EdgeInsets.all(20),
                    child: Form(
                      key: formState,
                      child: Column(
                        children: [
                          CustomTextField(
                            controller: username,
                            text: 'Username',
                            hasborder: true,
                            prefixIconName: 'username',
                            isenable: false,
                          ),
                          SizedBox(height: 15),

                          CustomTextField(
                            controller: phone,
                            text: 'Phone',
                            hasborder: true,
                            prefixIconName: 'phone',
                            isenable: false,
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}
