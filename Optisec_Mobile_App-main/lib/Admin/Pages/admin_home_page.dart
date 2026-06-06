import 'package:flutter/material.dart';
import 'package:optisecapp/Admin/Pages/add_client.dart';
import 'package:optisecapp/Admin/Pages/clients_list.dart';
import 'package:optisecapp/Admin/Pages/admin_dashboard.dart';

class AdminHomePage extends StatefulWidget {
  const AdminHomePage({super.key});

  @override
  State<AdminHomePage> createState() => _AdminHomePageState();
}

class _AdminHomePageState extends State<AdminHomePage> {
  int selectedidx = 0;
  List<Widget> pages = [AdminDashBoard(), AddClient(), ClientsList()];
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: AnimatedSwitcher(
        duration: const Duration(milliseconds: 300), // سرعة الانتقال
        switchInCurve: Curves.easeInOut,
        switchOutCurve: Curves.easeInOut,
        child: IndexedStack(
          key: ValueKey<int>(selectedidx),
          index: selectedidx,
          children: pages,
        ),
      ),
      bottomNavigationBar: Theme(
        data: Theme.of(context).copyWith(
          splashColor: Colors.transparent,
          highlightColor: Colors.transparent,
        ),
        child: Theme(
          data: Theme.of(context).copyWith(
            splashColor: Colors.transparent,
            highlightColor: Colors.transparent,
          ),
          child: BottomNavigationBar(
            selectedIconTheme: IconThemeData(size: 33),
            unselectedIconTheme: IconThemeData(size: 30),
            enableFeedback: true,
            showSelectedLabels: false,
            showUnselectedLabels: false,
            onTap: (value) {
              setState(() {
                selectedidx = value;
              });
            },
            currentIndex: selectedidx,
            items: [
              BottomNavigationBarItem(
                icon: Icon(Icons.dashboard_outlined),
                activeIcon: Icon(Icons.dashboard),
                label: 'Dashboard',
                tooltip: 'Dashboard',
              ),
              BottomNavigationBarItem(
                icon: Icon(Icons.person_add_alt_1_outlined),
                activeIcon: Icon(Icons.person_add_alt_1),
                label: 'Add client',
                tooltip: 'Add client',
              ),
              BottomNavigationBarItem(
                icon: Icon(Icons.people_alt_outlined),
                activeIcon: Icon(Icons.people_alt),
                label: 'Clients',
                tooltip: 'Clients',
              ),
            ],
          ),
        ),
      ),
    );
  }
}
