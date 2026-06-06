import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:flutter/material.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/Client/Pages/add_gate.dart';
import 'package:optisecapp/Client/Pages/edit_gate.dart';
import 'package:optisecapp/Client/provider/user_provider.dart';
import 'package:optisecapp/CustomWidgets/custom_card.dart';
import 'package:optisecapp/CustomWidgets/delete_dialog.dart';
import 'package:provider/provider.dart';

class GatesPage extends StatefulWidget {
  final bool showBackArrow;
  const GatesPage({super.key, required this.showBackArrow});

  @override
  State<GatesPage> createState() => _GatesPageState();
}

class _GatesPageState extends State<GatesPage> {
  List filteredGates = [];
  List selectedGates = [];
  bool get isSelect => selectedGates.isNotEmpty;
  String searchText = "";
  void handleSearch(String input, List gates) {
    setState(() {
      searchText = input;
      if (input.isEmpty) {
        filteredGates = [];
      } else {
        filteredGates = gates.where((gate) {
          final name = gate['name'] ?? '';
          return name.toLowerCase().contains(input.toLowerCase());
        }).toList();
      }
    });
  }

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<UserProvider>().fetchGates();
    });
  }

  @override
  Widget build(BuildContext context) {
    final userProvider = context.watch<UserProvider>();
    final gates = userProvider.gates;
    final displayedGates = searchText.isEmpty ? gates : filteredGates;
    return Scaffold(
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(builder: (context) => AddGate()),
          );
        },
        backgroundColor: Color(0xff0176EA),
        shape: CircleBorder(),
        child: Icon(Icons.add, color: Colors.white),
      ),
      floatingActionButtonLocation: FloatingActionButtonLocation.endFloat,
      appBar: AppBar(
        title: Text(
          isSelect ? '${selectedGates.length}' : 'GATES',
          style: Theme.of(context).textTheme.bodyLarge,
        ),
        centerTitle: isSelect ? false : true,
        leading: widget.showBackArrow
            ? Container(
                margin: const EdgeInsets.only(left: 15.0),
                child: IconButton(
                  onPressed: () {
                    if (isSelect) {
                      setState(() {
                        selectedGates.clear();
                      });
                    } else {
                      Navigator.pop(context);
                    }
                  },
                  icon: Icon(isSelect ? Icons.close : Icons.arrow_back_ios),
                ),
              )
            : null,
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

                      for (var gate in selectedGates) {
                        await ApiService().deleteGate(
                          token: token,
                          gateId: gate['id'],
                        );
                        await FirebaseFirestore.instance
                            .collection('Gates')
                            .doc(gate['id'].toString()).delete();
                      }
                      setState(() {
                        selectedGates.clear();
                      });
                      userProvider.fetchGates();
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
              onChanged: (value) {
                handleSearch(value, gates);
              },
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
              child: userProvider.isGatesLoading
                  ? Center(child: CircularProgressIndicator())
                  : displayedGates.isEmpty && searchText.isEmpty
                  ? Center(child: Text('No Gates Found'))
                  : ListView.builder(
                      itemCount: displayedGates.length,
                      itemBuilder: (context, idx) {
                        final gate = displayedGates[idx];
                        return GestureDetector(
                          onLongPress: () {
                            setState(() {
                              if (!selectedGates.contains(gate)) {
                                setState(() {
                                  selectedGates.add(gate);
                                });
                              }
                            });
                          },
                          onTap: () async {
                            if (isSelect) {
                              if (selectedGates.contains(gate)) {
                                setState(() {
                                  selectedGates.remove(gate);
                                });
                              } else {
                                setState(() {
                                  selectedGates.add(gate);
                                });
                              }
                            } else {
                              final result = await Navigator.push(
                                context,
                                MaterialPageRoute(
                                  builder: (context) =>
                                      EditGate(gateId: gate['id']),
                                ),
                              );
                              if (result == true) {
                                userProvider.fetchGates();
                              }
                            }
                          },
                          child: CustomCard(
                            name: gate['name'],
                            img: '',
                            location: gate['location'],
                            isGate: true,
                            selected: selectedGates.contains(gate),
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
