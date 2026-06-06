import 'dart:io';
import 'package:flutter/material.dart';
import 'package:optisecapp/Admin/provider/admin_provider.dart';
import 'package:optisecapp/CustomWidgets/bottom_modal.dart';
import 'package:optisecapp/CustomWidgets/image_modal_widget.dart';
import 'package:provider/provider.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:optisecapp/CustomWidgets/ok_dialog.dart';
import 'package:optisecapp/Api/api_service.dart';

class EditClient extends StatefulWidget {
  final int clientId;

  const EditClient({super.key, required this.clientId});

  @override
  State<EditClient> createState() => _EditClientState();
}

class _EditClientState extends State<EditClient> {
  late TextEditingController name;
  late TextEditingController username;
  late TextEditingController email;
  late TextEditingController phone;
  late AdminProvider provider;

  final GlobalKey<FormState> formState = GlobalKey<FormState>();

  String initialname = '';
  String initialusername = '';
  String initialemail = '';
  String initialphone = '';
  String imageUrl = '';

  bool changeColor = false;
  bool isLoading = true;

  @override
  void initState() {
    super.initState();

    name = TextEditingController();
    username = TextEditingController();
    email = TextEditingController();
    phone = TextEditingController();

    provider = context.read<AdminProvider>();

    name.addListener(checkIfChange);
    username.addListener(checkIfChange);
    email.addListener(checkIfChange);
    phone.addListener(checkIfChange);

    provider.addListener(checkIfChange);

    fetchClientData();
  }

  Future<void> fetchClientData() async {
    try {
      final token = await getToken();
      if (token == null) return;

      final client = await ApiService().getClientById(token, widget.clientId);

      setState(() {
        initialname = client['name'] ?? '';
        initialusername = client['userName'] ?? '';
        initialemail = client['email'] ?? '';
        initialphone = client['phoneNumber'] ?? '';
        imageUrl = client['imageUrl'] ?? '';

        name.text = initialname;
        username.text = initialusername;
        email.text = initialemail;
        phone.text = initialphone;

        isLoading = false;
      });
    } catch (e) {
      print("Error fetching client: $e");
      setState(() => isLoading = false);
    }
  }

  void checkIfChange() {
    setState(() {
      changeColor =
          name.text != initialname ||
          username.text != initialusername ||
          email.text != initialemail ||
          phone.text != initialphone ||
          provider.picture != null;
    });
  }

  @override
  void dispose() {
    provider.removeListener(checkIfChange);

    name.dispose();
    username.dispose();
    email.dispose();
    phone.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final provider = context.read<AdminProvider>();
    return Scaffold(
      appBar: AppBar(
        title: Text('EDIT CLIENT'),
        leading: Container(
          margin: const EdgeInsets.only(left: 15.0),
          child: IconButton(
            onPressed: () => Navigator.pop(context, true),
            icon: Icon(Icons.arrow_back_ios),
          ),
        ),
        titleTextStyle: TextStyle(fontSize: 25),
        actions: [
          Container(
            margin: EdgeInsets.only(right: 20),
            width: 83,
            height: 40,
            child: CustomButton(
              text: 'Save',
              bgColor: changeColor ? Color(0xff0176EA) : Color(0xff161A22),
              textSize: 15,
              textColor: changeColor ? Colors.white : Colors.grey[400]!,
              onPressed: () async {
                if (!changeColor) return;
                final token = await getToken();
                if (token == null) {
                  return;
                }

                final success = await ApiService().updateClient(
                  token: token,
                  clientId: widget.clientId,
                  body: {
                    "name": name.text,
                    "userName": username.text,
                    "email": email.text,
                    "phoneNumber": phone.text,
                  },
                  image: provider.picture != null
                      ? File(provider.picture!.path)
                      : null,
                );
                if (success) {
                  provider.clearImage();

                  await okDialog(
                    context,
                    content: 'Updated successfully',
                    contentColor: Colors.green,
                  );
                  Navigator.pop(context, true);
                } else {
                  okDialog(
                    context,
                    content: 'Failed to update user',
                    contentColor: Colors.red,
                  );
                }
              },
            ),
          ),
        ],
      ),

      body: isLoading
          ? Center(child: CircularProgressIndicator())
          : Padding(
              padding: const EdgeInsets.all(20),
              child: Form(
                key: formState,
                child: SingleChildScrollView(
                  child: Column(
                    children: [
                      Consumer<AdminProvider>(
                        builder: (context, provider, _) {
                          return CircleAvatar(
                            radius: 65,
                            backgroundImage: provider.picture != null
                                ? FileImage(File(provider.picture!.path))
                                : (imageUrl.isNotEmpty
                                      ? NetworkImage(imageUrl)
                                      : AssetImage('images/profile.jfif')
                                            as ImageProvider),
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
                                icon: Icon(
                                  Icons.add,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ),
                          );
                        },
                      ),
                      SizedBox(height: 30),

                      CustomTextField(
                        controller: name,
                        text: 'Name',
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
                        controller: phone,
                        text: 'Phone',
                        hasborder: true,
                        prefixIconName: 'phone',
                      ),
                    ],
                  ),
                ),
              ),
            ),

      // ),
    );
  }
}
