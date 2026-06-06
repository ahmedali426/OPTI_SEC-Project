import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:provider/provider.dart';

class AddGate extends StatefulWidget {
  const AddGate({super.key});

  @override
  State<AddGate> createState() => _AddGateState();
}

class _AddGateState extends State<AddGate> {
  late TextEditingController name;
  late TextEditingController location;
  late TextEditingController deviceId;
  late TextEditingController password;
  late TextEditingController alarm;
  GlobalKey<FormState> formState = GlobalKey<FormState>();
  bool isLoading = false;
  // Future<void> registerNewGate(String gateId) async {
  //   await FirebaseFirestore.instance.collection('Gates').doc(gateId).set({
  //     'is_open': false,
  //     'buzzer_on': false,
  //   });
  // }

  @override
  void initState() {
    super.initState();
    name = TextEditingController();
    location = TextEditingController();
    deviceId = TextEditingController();
    password = TextEditingController();
    alarm = TextEditingController();
  }

  @override
  void dispose() {
    super.dispose();
    name.dispose();
    location.dispose();
    deviceId.dispose();
    password.dispose();
    alarm.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('ADD NEW GATE'),
        centerTitle: true,
        titleTextStyle: Theme.of(context).textTheme.bodyLarge,
        leading: Container(
          margin: const EdgeInsets.only(left: 15.0),
          child: IconButton(
            onPressed: () => Navigator.pop(context, true),
            icon: Icon(Icons.arrow_back_ios),
          ),
        ),
      ),
      body: Padding(
        padding: const EdgeInsets.only(top: 30),
        child: ListView(
          scrollDirection: Axis.vertical,
          shrinkWrap: true,
          children: [
            Padding(
              padding: const EdgeInsets.all(20),
              child: Form(
                key: formState,
                child: Column(
                  children: [
                    CustomTextField(
                      controller: name,
                      text: 'Gate Name',
                      hasborder: true,
                      prefixIconName: 'gate',
                    ),
                    SizedBox(height: 15),
                    CustomTextField(
                      controller: location,
                      text: 'Location',
                      hasborder: true,
                      prefixIconName: 'location',
                    ),
                    SizedBox(height: 15),
                    CustomTextField(
                      controller: deviceId,
                      text: 'Devise Id',
                      hasborder: true,
                      prefixIconName: 'device',
                    ),
                    SizedBox(height: 15),
                    CustomTextField(
                      controller: password,
                      text: 'Gate Password',
                      hasborder: true,
                      hasSuffix: true,
                      prefixIconName: 'password',
                    ),
                    SizedBox(height: 15),
                    CustomTextField(
                      controller: alarm,
                      text: 'Silent Alarm Password',
                      hasborder: true,
                      hasSuffix: true,
                      prefixIconName: 'alarm',
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
                          final navigator = Navigator.of(context);
                          final scaffoldMessenger = ScaffoldMessenger.of(
                            context,
                          );
                          setState(() {
                            isLoading = true;
                          });
                          try {
                            String? token = await getToken();

                            if (token != null) {
                              String? data = await ApiService().addGate(
                                token: token,
                                gateData: {
                                  'name': name.text,
                                  'location': location.text,
                                  'deviceId': deviceId.text,
                                  'password': password.text,
                                  'silentAlarm': alarm.text,
                                },
                              );

                              if (!mounted) return;

                              if (data == 'success') {
                                scaffoldMessenger.showSnackBar(
                                  const SnackBar(
                                    content: Text('Successfully Added Gate'),
                                  ),
                                );
                                navigator.pop();
                                context.read<UserProvider>().fetchGates();
                              } else {
                                print(data);
                                scaffoldMessenger.showSnackBar(
                                  SnackBar(
                                    content: Text(data ?? 'Failed to Add Gate'),
                                  ),
                                );
                              }
                            } else {
                              scaffoldMessenger.showSnackBar(
                                const SnackBar(
                                  content: Text(
                                    'Authentication Token not found.',
                                  ),
                                ),
                              );
                            }
                          } catch (e) {
                            print('Error catch in Add Gate : $e');
                            scaffoldMessenger.showSnackBar(
                              SnackBar(
                                content: Text(
                                  'An unexpected error occurred: $e',
                                ),
                              ),
                            );
                          } finally {
                            if (mounted) {
                              setState(() {
                                isLoading = false;
                              });
                            }
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
