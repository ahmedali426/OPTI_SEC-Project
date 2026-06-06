import 'dart:io';

import 'package:flutter/material.dart';
import 'package:optisecapp/Admin/Pages/admin_home_page.dart';
import 'package:optisecapp/Admin/provider/admin_provider.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/CustomWidgets/bottom_modal.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:optisecapp/CustomWidgets/image_modal_widget.dart';
import 'package:provider/provider.dart';

class AddClient extends StatefulWidget {
  const AddClient({super.key});

  @override
  State<AddClient> createState() => _AddClientState();
}

class _AddClientState extends State<AddClient> {
  late TextEditingController firstName;
  late TextEditingController lastName;
  late TextEditingController username;
  late TextEditingController email;
  late TextEditingController password;
  late TextEditingController phone;
  final GlobalKey<FormState> formState = GlobalKey<FormState>();
  File? photo;
  bool isLoading = false;
  @override
  void initState() {
    firstName = TextEditingController();
    lastName = TextEditingController();
    username = TextEditingController();
    email = TextEditingController();
    password = TextEditingController();
    phone = TextEditingController();
    super.initState();
  }

  @override
  void dispose() {
    firstName.dispose();
    lastName.dispose();
    username.dispose();
    email.dispose();
    password.dispose();
    phone.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('ADD NEW CLIENT'),
        centerTitle: true,
        titleTextStyle: Theme.of(context).textTheme.bodyLarge,
      ),
      body: Padding(
        padding: const EdgeInsets.only(top: 30),
        child: ListView(
          scrollDirection: Axis.vertical,
          shrinkWrap: true,
          children: [
            Consumer<AdminProvider>(
              builder: (context, model, child) {
                photo = model.picture;
                return Column(
                  children: [
                    CircleAvatar(
                      radius: 65,
                      backgroundImage: model.picture == null
                          ? AssetImage('images/profile.jfif')
                          : FileImage(model.picture!),
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
                    Text(
                      'Add Photo',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                );
              },
            ),
            Padding(
              padding: const EdgeInsets.all(20),
              child: Form(
                key: formState,
                child: Column(
                  children: [
                    CustomTextField(
                      controller: firstName,
                      text: 'First Name',
                      hasborder: true,
                      prefixIconName: 'name',
                    ),
                    SizedBox(height: 15),
                    CustomTextField(
                      controller: lastName,
                      text: 'Last Name',
                      hasborder: true,
                      prefixIconName: 'name',
                    ),
                    SizedBox(height: 15),
                    CustomTextField(
                      controller: username,
                      text: 'Username',
                      hasborder: true,
                      prefixIconName: 'username',
                    ),
                    SizedBox(height: 15),
                    CustomTextField(
                      controller: email,
                      text: 'Email',
                      hasborder: true,
                      prefixIconName: 'email',
                    ),
                    SizedBox(height: 15),
                    CustomTextField(
                      controller: password,
                      text: 'Password',
                      hasSuffix: true,
                      hasborder: true,
                      prefixIconName: 'password',
                    ),
                    SizedBox(height: 15),
                    CustomTextField(
                      controller: phone,
                      text: 'Phone',
                      hasborder: true,
                      prefixIconName: 'phone',
                    ),
                    SizedBox(height: 30),
                    CustomButton(
                      bgColor: Color(0xff0176EA),
                      textColor: Colors.white,
                      text: 'Add',
                      isLoading: isLoading,
                      onPressed: () async {
                        if (formState.currentState!.validate()) {
                          print('fields are valid');
                          if (photo == null) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(content: Text('Please select a photo')),
                            );
                            return;
                          }
                          setState(() {
                            isLoading = true;
                          });
                          String? token = await getToken();
                          if (token != null) {
                            final message = await ApiService().addClient(
                              token: token,
                              clientData: {
                                'fname': firstName.text,
                                'lname': lastName.text,
                                'email': email.text,
                                'username': username.text,
                                'phone': phone.text,
                                'password': password.text,
                              },
                              image: photo!,
                            );
                            if (!mounted) return;
                            if (message == 'success') {
                              context.read<AdminProvider>().clearImage();
                              ScaffoldMessenger.of(context).showSnackBar(
                                SnackBar(
                                  content: Text('Successfully Added Client'),
                                  duration: Duration(seconds: 2),
                                ),
                              );
                              Navigator.pushAndRemoveUntil(
                                context,
                                MaterialPageRoute(
                                  builder: (context) => AdminHomePage(),
                                ),
                                (route) => false,
                              );
                            } else {
                              ScaffoldMessenger.of(
                                context,
                              ).showSnackBar(SnackBar(content: Text(message)));
                            }
                          }
                          if (mounted) {
                            setState(() {
                              isLoading = false;
                            });
                          }
                        } else {
                          print('fields are not valid');
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
