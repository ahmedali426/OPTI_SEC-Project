import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Client/Pages/client_home_page.dart';
import 'package:optisecapp/CustomWidgets/bottom_modal.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:optisecapp/CustomWidgets/image_modal_widget.dart';
import 'package:provider/provider.dart';

class AddUser extends StatefulWidget {
  const AddUser({super.key});

  @override
  State<AddUser> createState() => _AddUserState();
}

class _AddUserState extends State<AddUser> {
  late TextEditingController fname;
  late TextEditingController lname;
  late TextEditingController username;
  late TextEditingController phone;

  final GlobalKey<FormState> formState = GlobalKey<FormState>();
  bool isLoading = false;

  @override
  void initState() {
    fname = TextEditingController();
    lname = TextEditingController();
    username = TextEditingController();
    phone = TextEditingController();
    super.initState();
  }

  @override
  void dispose() {
    fname.dispose();
    lname.dispose();
    username.dispose();
    phone.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final provider = context.watch<UserProvider>();

    return Scaffold(
      appBar: AppBar(title: Text('ADD NEW USER'), centerTitle: true),

      body: Padding(
        padding: const EdgeInsets.only(top: 30),
        child: ListView(
          children: [
            Column(
              children: [
                CircleAvatar(
                  radius: 65,
                  backgroundImage: provider.picture == null
                      ? AssetImage('images/profile.jfif')
                      : FileImage(provider.picture!),

                  child: Align(
                    alignment: Alignment.bottomRight,
                    child: IconButton(
                      onPressed: () {
                        bottomModal(
                          context,
                          childWidget: ImageModalWidget(),
                        );
                      },
                      padding: EdgeInsets.all(5),
                      style: IconButton.styleFrom(
                        backgroundColor: Colors.white,
                        foregroundColor: Colors.black,
                      ),
                      iconSize: 18,
                      icon: Icon(Icons.add, fontWeight: FontWeight.w600),
                    ),
                  ),
                ),

                SizedBox(height: 5),
                Text('Add Photo'),
              ],
            ),

            Padding(
              padding: const EdgeInsets.all(20),
              child: Form(
                key: formState,
                child: Column(
                  children: [
                    CustomTextField(
                      controller: fname,
                      text: 'First Name',
                      prefixIconName: 'name',
                      hasborder: true,
                    ),
                    SizedBox(height: 15),

                    CustomTextField(
                      controller: lname,
                      text: 'Last Name',
                      prefixIconName: 'name',
                      hasborder: true,
                    ),
                    SizedBox(height: 15),

                    CustomTextField(
                      controller: username,
                      text: 'Username',
                      prefixIconName: 'username',
                      hasborder: true,
                    ),
                    SizedBox(height: 15),

                    CustomTextField(
                      controller: phone,
                      text: 'Phone',
                      prefixIconName: 'phone',
                      hasborder: true,
                    ),

                    SizedBox(height: 30),

                    CustomButton(
                      text: 'Add User',
                      isLoading: isLoading,
                      bgColor: Color(0xff0176EA),
                      textColor: Colors.white,

                      onPressed: () async {
                        if (!formState.currentState!.validate()) return;

                        if (provider.picture == null) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(content: Text('Please select a photo')),
                          );
                          return;
                        }

                        setState(() => isLoading = true);

                        final token = await getToken();

                        final success = await ApiService().addUser(
                          token: token!,
                          userData: {
                            "fname": fname.text,
                            "lname": lname.text,
                            "username": username.text,
                            "phone": phone.text,
                          },
                          image: provider.picture!,
                        );

                        if (!mounted) return;

                        setState(() => isLoading = false);

                        if (success) {
                          context.read<UserProvider>().clearImage();
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text('Successfully Added user'),
                              duration: Duration(seconds: 2),
                            ),
                          );
                          Navigator.pushAndRemoveUntil(
                            context,
                            MaterialPageRoute(
                              builder: (context) => ClientHomePage(),
                            ),
                            (route) => false,
                          );
                        } else {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(content: Text('Failed to Add user')),
                          );
                        }
                      },
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
