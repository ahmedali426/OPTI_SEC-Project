import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:flutter/material.dart';
import 'package:optisecapp/Admin/Pages/client_info.dart';
import 'package:optisecapp/Admin/provider/admin_provider.dart';
import 'package:optisecapp/CustomWidgets/custom_card.dart';
import 'package:optisecapp/Api/api_service.dart';
import 'package:optisecapp/CustomWidgets/delete_dialog.dart';
import 'package:provider/provider.dart';

class ClientsList extends StatefulWidget {
  final bool showBackArrow;
  const ClientsList({super.key, this.showBackArrow = false});

  @override
  State<ClientsList> createState() => _ClientsListState();
}

class _ClientsListState extends State<ClientsList> {
  List<dynamic> filteredClients = [];
  List<dynamic> selectedClients = [];

  String searchText = "";

  bool get isSelect => selectedClients.isNotEmpty;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<AdminProvider>().fetchClients();
    });
  }

  void handleSearch(String input, List<dynamic> clients) {
    setState(() {
      searchText = input;

      if (input.isEmpty) {
        filteredClients = [];
      } else {
        filteredClients = clients.where((client) {
          final name = client['name'] ?? '';
          return name.toLowerCase().contains(input.toLowerCase());
        }).toList();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final adminProvider = context.watch<AdminProvider>();
    final clients = adminProvider.clients;
    final displayedUsers = searchText.isEmpty ? clients : filteredClients;
    return Scaffold(
      appBar: AppBar(
        leading: widget.showBackArrow
            ? Container(
                margin: const EdgeInsets.only(left: 15.0),
                child: IconButton(
                  onPressed: () {
                    if (isSelect) {
                      setState(() {
                        selectedClients.clear();
                      });
                    } else {
                      Navigator.pop(context);
                    }
                  },
                  icon: Icon(isSelect ? Icons.close : Icons.arrow_back_ios),
                ),
              )
            : null,
        title: Text(
          isSelect ? '${selectedClients.length}' : 'CLIENTS',
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
                      List<Future<void>> deleteTasks = [];

                      for (var client in selectedClients) {
                        final clientId = client['id'];
                        deleteTasks.add(() async {
                          try {
                            final deleted = await ApiService().deleteClient(
                              token: token,
                              clientId: clientId,
                            );
                            if (deleted) {
                              await FirebaseFirestore.instance
                                  .collection('device-tokens')
                                  .doc('$clientId')
                                  .delete();
                              print(
                                'Successfully deleted FCM token for userId: $clientId',
                              );
                            }
                          } catch (e) {
                            print('Error deleting client $clientId: $e');
                          }
                        }());
                      }
                      await Future.wait(deleteTasks);
                      if (!mounted) return;
                      setState(() {
                        selectedClients.clear();
                      });
                      // updating after delete
                      adminProvider.fetchClients();
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
              onChanged: (value) => handleSearch(value, clients),
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
              child: adminProvider.isloading
                  ? Center(child: CircularProgressIndicator())
                  : displayedUsers.isEmpty && searchText.isNotEmpty
                  ? Center(child: Text("No clients found"))
                  : ListView.builder(
                      itemCount: displayedUsers.length,
                      itemBuilder: (context, idx) {
                        final client = displayedUsers[idx];
                        return GestureDetector(
                          onLongPress: () {
                            if (!selectedClients.contains(client)) {
                              setState(() {
                                selectedClients.add(client);
                              });
                            }
                          },
                          onTap: () async {
                            if (isSelect) {
                              setState(() {
                                if (selectedClients.contains(client)) {
                                  selectedClients.remove(client);
                                } else {
                                  selectedClients.add(client);
                                }
                              });
                            } else {
                              final result = await Navigator.push(
                                context,
                                MaterialPageRoute(
                                  builder: (context) =>
                                      ClientInfo(clientId: client['id']),
                                ),
                              );
                              if (result == true) {
                                adminProvider.fetchClients();
                              }
                            }
                          },
                          child: CustomCard(
                            name: client['name'] ?? 'No Name',
                            img: client['imageUrl'] ?? '',
                            selected: selectedClients.contains(client),
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
