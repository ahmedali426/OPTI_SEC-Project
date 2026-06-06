import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_database/firebase_database.dart';
import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:optisecapp/CustomWidgets/bottom_modal.dart';
import 'package:optisecapp/CustomWidgets/constants.dart';
import 'package:optisecapp/CustomWidgets/custom_button.dart';
import 'package:optisecapp/CustomWidgets/custom_text_field.dart';
import 'package:optisecapp/CustomWidgets/delete_dialog.dart';
import 'package:optisecapp/CustomWidgets/ok_dialog.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

class EditGate extends StatefulWidget {
  final int gateId;
  const EditGate({super.key, required this.gateId});

  @override
  State<EditGate> createState() => _EditGateState();
}

class _EditGateState extends State<EditGate> {
  late TextEditingController name;
  late TextEditingController location;
  late TextEditingController deviceId;
  late TextEditingController password;
  late TextEditingController alarm;
  GlobalKey<FormState> formState = GlobalKey<FormState>();
  bool changeColor = false;
  bool isSaving = false;
  bool isloading = true;
  bool isDeleting = false;
  String initialname = '';
  String initiallocation = '';
  String initialdeviceId = '';
  String initialPassword = '';
  String initialAlarm = '';
  final DatabaseReference _dbRef = FirebaseDatabase.instanceFor(
    app: Firebase.app(),
    databaseURL:
        'https://opti-sec-default-rtdb.europe-west1.firebasedatabase.app/',
  ).ref();
  // open gate
  Future<void> updateGateStatus(bool isOpen) async {
    try {
      await _dbRef.update({'GateStatus': isOpen});
      print("GateStatus updated to $isOpen");
    } catch (e) {
      print("Error updating Realtime Database: $e");
    }
  }

  Future<void> triggerGate(int gateId) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final token = prefs.getString('auth_token');
      if (token != null) {
        final isSuccess = await ApiService().triggerGate(
          token: token,
          gateId: gateId,
        );
        if (isSuccess) {
          if (mounted) context.read<UserProvider>().fetchGatesStatus();
          print('successfully trigger gate');
        }
      }
    } catch (e) {
      print('error during updating gate: $e');
    }
  }

  // turn off bazzer
  Future<void> updateBuzzerStatus(bool isActive) async {
    try {
      await _dbRef.update({'BuzzerStatus': isActive});
      print("BuzzerStatus updated to $isActive");
    } catch (e) {
      print("Error updating Realtime Database: $e");
    }
  }

  Future<void> turnOffBazzer(int gateId) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final token = prefs.getString('auth_token');
      if (token != null) {
        final isSuccess = await ApiService().stopBuzzer(
          token: token,
          gateId: gateId,
        );
        if (isSuccess) {
          if (mounted) context.read<UserProvider>().fetchGatesStatus();
          print('successfully stop buzzer');
        }
      }
    } catch (e) {
      print('error in stopping buzzer: $e');
    }
  }

  @override
  void initState() {
    super.initState();
    name = TextEditingController();
    location = TextEditingController();
    deviceId = TextEditingController();
    password = TextEditingController();
    alarm = TextEditingController();
    fetchGate();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<UserProvider>().fetchGatesStatus();
    });
  }

  void checkIfChange() {
    setState(() {
      changeColor =
          name.text != initialname ||
          location.text != initiallocation ||
          deviceId.text != initialdeviceId ||
          password.text != initialPassword ||
          alarm.text != initialAlarm;
    });
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

  Future<void> fetchGate() async {
    try {
      final token = await getToken();
      if (token == null) throw Exception("No token found");

      final gate = await ApiService().getGateById(
        token: token,
        gateId: widget.gateId,
      );
      setState(() {
        name.text = gate['name'] ?? '';
        location.text = gate['location'] ?? '';
        deviceId.text = gate['deviceId'] ?? '';
        password.text = gate['password'] ?? '';
        alarm.text = gate['silentAlarm'] ?? '';
        initialname = name.text;
        initiallocation = location.text;
        initialdeviceId = deviceId.text;
        initialPassword = password.text;
        initialAlarm = alarm.text;
        isloading = false;
      });
      name.addListener(checkIfChange);
      location.addListener(checkIfChange);
      deviceId.addListener(checkIfChange);
      password.addListener(checkIfChange);
      alarm.addListener(checkIfChange);
    } catch (e) {
      print("Error fetching user: $e");
      setState(() {
        isloading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final userProvider = context.watch<UserProvider>();
    List gatesStatus = userProvider.gatesStatus;
    Map<String, dynamic>? currentGateStatus;
    for (var gate in gatesStatus) {
      if (gate['gateId'] == widget.gateId) {
        currentGateStatus = gate;
      }
    }

    bool isBuzzerActive = currentGateStatus?['buzzerActive'] ?? false;
    String gateStatus = currentGateStatus?['status'] ?? 'Offline';

    if (isBuzzerActive == true) {
      Future.delayed(const Duration(seconds: 30), () async {
        if (mounted) {
          await updateBuzzerStatus(false);
          turnOffBazzer(widget.gateId);
          print(
            'Buzzer auto-resolved after 30 seconds on both Firebase and Backend.',
          );
        }
      });
    }
    return Scaffold(
      appBar: AppBar(
        title: Text('EDIT GATE', style: Theme.of(context).textTheme.bodyLarge),
        centerTitle: false,
        leading: Container(
          margin: const EdgeInsets.only(left: 15.0),
          child: IconButton(
            onPressed: () {
              Navigator.pop(context);
            },
            icon: Icon(Icons.arrow_back_ios),
          ),
        ),
        actions: [
          Container(
            margin: EdgeInsets.only(right: 20),
            width: 120,
            height: 40,
            child: CustomButton(
              text: isSaving ? 'Loading...' : 'Save',
              bgColor: changeColor ? Constants.mainColor : Constants.cardColor,
              textSize: 15,
              textColor: changeColor ? Colors.white : Colors.grey[400]!,
              onPressed: () async {
                if (!changeColor || isSaving) return;

                setState(() => isSaving = true);

                final token = await getToken();
                if (token == null) {
                  setState(() => isSaving = false);
                  return;
                }

                final success = await ApiService().updateGate(
                  token: token,
                  gateId: widget.gateId,
                  body: {
                    "name": name.text,
                    "location": location.text,
                    'deviceId': deviceId.text,
                    'password': password.text,
                    'silentAlarm': alarm.text,
                  },
                );

                setState(() => isSaving = false);

                if (success) {
                  await okDialog(
                    context,
                    content: 'Updated successfully',
                    contentColor: Colors.green,
                  );

                  Navigator.pop(context, true);
                } else {
                  okDialog(
                    context,
                    content: 'Failed to update gate',
                    contentColor: Colors.red,
                  );
                }
              },
            ),
          ),
        ],
      ),
      body: Stack(
        children: [
          isloading
              ? Center(child: CircularProgressIndicator())
              : Padding(
                  padding: const EdgeInsets.all(20),
                  child: Form(
                    key: formState,
                    child: SingleChildScrollView(
                      child: Column(
                        children: [
                          SizedBox(height: 30),
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
                          // SizedBox(height: 15),
                          // CustomTextField(
                          //   controller: password,
                          //   text: 'Gate Password',
                          //   hasborder: true,
                          //   hasSuffix: true,
                          //   prefixIconName: 'password',
                          // ),
                          // SizedBox(height: 15),
                          // CustomTextField(
                          //   controller: alarm,
                          //   text: 'Silent Alarm Password',
                          //   hasborder: true,
                          //   hasSuffix: true,
                          //   prefixIconName: 'alarm',
                          // ),
                          SizedBox(height: 60),

                          Column(
                            children: [
                              Container(
                                alignment: Alignment.topLeft,
                                child: const Text(
                                  'GATE CONTROL',
                                  style: TextStyle(
                                    fontSize: 22,
                                    fontWeight: FontWeight.w300,
                                  ),
                                ),
                              ),
                              const SizedBox(height: 8),

                              StreamBuilder<DatabaseEvent>(
                                stream: _dbRef.onValue,
                                builder: (context, snapshot) {
                                  if (!snapshot.hasData ||
                                      snapshot.data?.snapshot.value == null) {
                                    return const Center(
                                      child: CircularProgressIndicator(),
                                    );
                                  }

                                  final Map<dynamic, dynamic> dbValues =
                                      snapshot.data!.snapshot.value
                                          as Map<dynamic, dynamic>;

                                  var gateVal = dbValues['GateStatus'];
                                  bool firebaseGateOpen = gateVal is bool
                                      ? gateVal
                                      : (gateVal.toString().toLowerCase() ==
                                            'true');

                                  var buzzerVal = dbValues['BuzzerStatus'];
                                  bool firebaseBuzzerActive = buzzerVal is bool
                                      ? buzzerVal
                                      : (buzzerVal.toString().toLowerCase() ==
                                            'true');
                                  if (firebaseBuzzerActive == true) {
                                    Future.delayed(
                                      const Duration(seconds: 30),
                                      () async {
                                        if (mounted) {
                                          await updateBuzzerStatus(false);
                                          turnOffBazzer(widget.gateId);
                                          print(
                                            'Buzzer automatically turned OFF after 3 seconds in both Firebase and Backend.',
                                          );
                                        }
                                      },
                                    );
                                  }

                                  return Column(
                                    children: [
                                      ListTile(
                                        onTap: () async {
                                          if (firebaseGateOpen == false) {
                                            await updateGateStatus(true);
                                            triggerGate(widget.gateId);
                                            print(
                                              'Gate was closed. Sent OPEN command to both Firebase and Backend.',
                                            );
                                          } else {
                                            await updateGateStatus(false);
                                            print(
                                              'Gate was open. Sent CLOSE command to Firebase only.',
                                            );
                                          }
                                        },
                                        contentPadding: EdgeInsets.zero,
                                        leading: Container(
                                          padding: const EdgeInsets.all(15),
                                          decoration: BoxDecoration(
                                            color: Constants.cardColor,
                                            borderRadius: BorderRadius.circular(
                                              12,
                                            ),
                                          ),
                                          child: const Icon(
                                            Icons.meeting_room_outlined,
                                            size: 30,
                                          ),
                                        ),
                                        title: Text(
                                          firebaseGateOpen
                                              ? 'Close Gate'
                                              : 'Open Gate',
                                          style: const TextStyle(
                                            fontSize: 18,
                                            fontWeight: FontWeight.w500,
                                          ),
                                        ),
                                        subtitle: Text(
                                          firebaseGateOpen
                                              ? 'Tap to close ${name.text}'
                                              : 'Tap to open ${name.text}',
                                          style: const TextStyle(
                                            fontSize: 12.5,
                                            fontWeight: FontWeight.w300,
                                          ),
                                        ),
                                        trailing: Padding(
                                          padding: const EdgeInsets.all(10.0),
                                          child: SizedBox(
                                            width: 105,
                                            height: 45,
                                            child: CustomButton(
                                              borderRadius: 20,
                                              textSize: 15,
                                              text: firebaseGateOpen
                                                  ? 'Opened'
                                                  : 'Closed',
                                              onPressed: () async {
                                                if (firebaseGateOpen == false) {
                                                  await updateGateStatus(true);
                                                  triggerGate(widget.gateId);
                                                } else {
                                                  await updateGateStatus(false);
                                                }
                                              },
                                              bgColor: firebaseGateOpen
                                                  ? Constants.mainColor
                                                  : Colors.red,
                                              textColor: Colors.white,
                                            ),
                                          ),
                                        ),
                                      ),
                                      const SizedBox(height: 10),

                                      ListTile(
                                        onTap: firebaseBuzzerActive
                                            ? () async {
                                                await updateBuzzerStatus(false);
                                                turnOffBazzer(widget.gateId);
                                              }
                                            : null,
                                        contentPadding: EdgeInsets.zero,
                                        leading: Container(
                                          padding: const EdgeInsets.all(15),
                                          decoration: BoxDecoration(
                                            color: Constants.cardColor,
                                            borderRadius: BorderRadius.circular(
                                              12,
                                            ),
                                          ),
                                          child: const Icon(
                                            Icons.notifications_outlined,
                                            size: 30,
                                          ),
                                        ),
                                        title: Text(
                                          firebaseBuzzerActive
                                              ? 'Buzzer Alarmed!'
                                              : 'Buzzer Safe',
                                          style: const TextStyle(
                                            fontSize: 18,
                                            fontWeight: FontWeight.w500,
                                          ),
                                        ),
                                        subtitle: Text(
                                          firebaseBuzzerActive
                                              ? 'Active! Auto-clearing in 30s...'
                                              : 'Buzzer is currently quiet',
                                          style: const TextStyle(
                                            fontSize: 12.5,
                                            fontWeight: FontWeight.w300,
                                          ),
                                        ),
                                        trailing: Padding(
                                          padding: const EdgeInsets.all(10.0),
                                          child: SizedBox(
                                            width: 105,
                                            height: 45,
                                            child: CustomButton(
                                              borderRadius: 20,
                                              textSize: 15,
                                              text: firebaseBuzzerActive
                                                  ? 'STOP'
                                                  : 'OFF',
                                              onPressed: firebaseBuzzerActive
                                                  ? () async {
                                                      await updateBuzzerStatus(
                                                        false,
                                                      );
                                                      turnOffBazzer(
                                                        widget.gateId,
                                                      );
                                                    }
                                                  : null,
                                              bgColor: firebaseBuzzerActive
                                                  ? Colors.red
                                                  : Colors.grey,
                                              textColor: Colors.white,
                                            ),
                                          ),
                                        ),
                                      ),
                                    ],
                                  );
                                },
                              ),
                              SizedBox(height: 10),
                              ListTile(
                                onTap: () {
                                  bottomModal(
                                    context,
                                    childWidget: Container(
                                      padding: const EdgeInsets.all(10),
                                      child: Column(
                                        children: [
                                          Container(
                                            padding: EdgeInsets.all(15),
                                            decoration: BoxDecoration(
                                              color: Colors.red,
                                              borderRadius:
                                                  BorderRadius.circular(12),
                                            ),
                                            child: Icon(
                                              Icons.delete_forever_outlined,
                                              size: 30,
                                            ),
                                          ),
                                          SizedBox(height: 8),
                                          Text(
                                            'Delete ${name.text}',
                                            style: TextStyle(
                                              fontSize: 20,
                                              fontWeight: FontWeight.w600,
                                            ),
                                          ),
                                          SizedBox(height: 8),
                                          Text(
                                            'This will permanently remove the gate and all its settings. This is action cannot be undone',
                                            textAlign: TextAlign.center,
                                            style: TextStyle(
                                              fontSize: 15,
                                              fontWeight: FontWeight.w400,
                                            ),
                                          ),
                                          SizedBox(height: 10),
                                          Container(
                                            margin: EdgeInsets.symmetric(
                                              horizontal: 20,
                                            ),
                                            child: Column(
                                              children: [
                                                CustomButton(
                                                  text: 'Yes, Delete Gate',
                                                  onPressed: () async {
                                                    Navigator.pop(context);
                                                    final confirm =
                                                        await deleteDialog(
                                                          context,
                                                        );
                                                    if (!confirm) return;
                                                    final token =
                                                        await getToken();
                                                    if (token == null) return;
                                                    setState(() {
                                                      isDeleting = true;
                                                    });
                                                    bool isSuccess =
                                                        await ApiService()
                                                            .deleteGate(
                                                              token: token,
                                                              gateId:
                                                                  widget.gateId,
                                                            );
                                                    setState(() {
                                                      isDeleting = false;
                                                    });
                                                    if (isSuccess) {
                                                      try {
                                                        await FirebaseFirestore
                                                            .instance
                                                            .collection('Gates')
                                                            .doc(
                                                              widget.gateId
                                                                  .toString(),
                                                            )
                                                            .delete();
                                                        print(
                                                          'Successfully gate is deleted from firebase',
                                                        );
                                                      } catch (e) {
                                                        print(
                                                          'error in deleting gate :$e',
                                                        );
                                                      }
                                                      if (!mounted) return;
                                                      Navigator.pop(context);
                                                      context
                                                          .read<UserProvider>()
                                                          .fetchGatesStatus();
                                                      context
                                                          .read<UserProvider>()
                                                          .fetchGates();
                                                    }
                                                  },
                                                  bgColor: Color(0xFF0B0F14),
                                                  textColor: Colors.white,
                                                  borderRadius: 20,
                                                  borderColor: Colors.grey[200],
                                                ),
                                                SizedBox(height: 5),
                                                CustomButton(
                                                  text: 'Cancel',
                                                  onPressed: () {
                                                    Navigator.pop(context);
                                                  },
                                                  bgColor: Color(0xFF0B0F14),
                                                  textColor: Colors.white,
                                                  borderRadius: 20,
                                                  borderColor: Colors.grey[200],
                                                ),
                                              ],
                                            ),
                                          ),
                                        ],
                                      ),
                                    ),
                                  );
                                },
                                contentPadding: EdgeInsets.zero,
                                leading: Container(
                                  padding: EdgeInsets.all(15),
                                  decoration: BoxDecoration(
                                    color: Colors.red,
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Icon(
                                    Icons.delete_forever_outlined,
                                    size: 30,
                                  ),
                                ),
                                title: Text(
                                  'Delete Gate',
                                  style: TextStyle(
                                    fontSize: 18,
                                    fontWeight: FontWeight.w500,
                                  ),
                                ),
                                subtitle: Text(
                                  'Permanently remove ${name.text}',
                                  style: TextStyle(
                                    fontSize: 12.5,
                                    fontWeight: FontWeight.w300,
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ],
                        // ======
                      ),
                    ),
                  ),
                ),
          if (isDeleting) ...[Center(child: CircularProgressIndicator())],
        ],
      ),
    );
  }
}
