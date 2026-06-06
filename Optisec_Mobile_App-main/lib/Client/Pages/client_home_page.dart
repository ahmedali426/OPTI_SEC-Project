import 'package:flutter/material.dart';
import 'package:optisecapp/Client/Pages/client_dashboard.dart';
import 'package:optisecapp/Client/Pages/gates_page.dart';
import 'package:optisecapp/Client/Pages/unauthorized_users.dart';
import 'package:optisecapp/Client/Pages/authorized_users.dart';
import 'package:optisecapp/Client/Pages/add_user.dart';

class ClientHomePage extends StatefulWidget {
  const ClientHomePage({super.key});

  @override
  State<ClientHomePage> createState() => _ClientHomePageState();
}

class _ClientHomePageState extends State<ClientHomePage> {
  int selectedidx = 0;
  List<Widget> pages = [
    ClientDashBoard(),
    GatesPage(showBackArrow: false),
    AddUser(),
    Authorizedusers(),
    UnauthorizedUsers(),
  ];
  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: AnimatedSwitcher(
        duration: const Duration(milliseconds: 300), 
        switchInCurve: Curves.easeInOut,
        switchOutCurve: Curves.easeInOut,
        child: IndexedStack(
          key: ValueKey<int>(selectedidx),
          index: selectedidx,
          children: pages,
        ),
      ),
      bottomNavigationBar: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 15.0),
        child: Theme(
          data: Theme.of(context).copyWith(
            highlightColor: Colors.transparent,
            splashColor: Colors.transparent,
          ),
          child: BottomNavigationBar(
            type: BottomNavigationBarType.fixed,
            enableFeedback: true,
            unselectedIconTheme: IconThemeData(size: 30),
            selectedIconTheme: IconThemeData(size: 33),
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
                icon: Icon(Icons.meeting_room_outlined),
                activeIcon: Icon(Icons.meeting_room),
                label: 'Gates',
                tooltip: 'Gates',
              ),
              BottomNavigationBarItem(
                icon: Icon(Icons.person_add_alt_1_outlined),
                activeIcon: Icon(Icons.person_add_alt_1),
                label: 'Add user',
                tooltip: 'Add user',
              ),
              BottomNavigationBarItem(
                icon: Icon(Icons.person_outline),
                activeIcon: Icon(Icons.person),
                label: 'Authorized users',
                tooltip: 'Authorized users',
              ),
              BottomNavigationBarItem(
                icon: Icon(Icons.person_off_outlined),
                activeIcon: Icon(Icons.person_off),
                label: 'Authorized users',
                tooltip: 'Authorized users',
              ),
            ],
          ),
        ),
      ),
    );
  }
}
