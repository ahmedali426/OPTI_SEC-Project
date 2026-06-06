package com.example.optisecapp

import android.content.Intent
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.provider.Settings
import android.view.WindowManager
import io.flutter.embedding.android.FlutterActivity
import io.flutter.embedding.engine.FlutterEngine
import io.flutter.plugin.common.MethodChannel

class MainActivity: FlutterActivity() {
    private val SETTINGS_CHANNEL = "com.example.optisecapp/settings"
    private val STATE_CHANNEL = "com.example.optisecapp/device_state"

    // 1. إضافة دالة onCreate لاختراق شاشة القفل وإضاءة الهاتف فوراً عند الطوارئ
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O_MR1) {
            setShowWhenLocked(true)
            setTurnScreenOn(true)
            val keyguardManager = getSystemService(android.content.Context.KEYGUARD_SERVICE) as android.app.KeyguardManager
            keyguardManager.requestDismissKeyguard(this, null)
        } else {
            @Suppress("DEPRECATION")
            window.addFlags(
                WindowManager.LayoutParams.FLAG_SHOW_WHEN_LOCKED or
                WindowManager.LayoutParams.FLAG_TURN_SCREEN_ON or
                WindowManager.LayoutParams.FLAG_DISMISS_KEYGUARD or
                WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON
            )
        }
    }

    // 2. الحفاظ على الـ MethodChannels الخاصة بكِ كاملة دون أي تغيير لضمان استقرار التطبيق
    override fun configureFlutterEngine(flutterEngine: FlutterEngine) {
        super.configureFlutterEngine(flutterEngine)

        // القناة الخاصة بفتح إعدادات النظام
        MethodChannel(flutterEngine.dartExecutor.binaryMessenger, SETTINGS_CHANNEL).setMethodCallHandler { call, result ->
            if (call.method == "openWriteSettings") {
                try {
                    val intent = Intent(Settings.ACTION_MANAGE_WRITE_SETTINGS).apply {
                        data = Uri.parse("package:$packageName")
                        addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
                    }
                    startActivity(intent)
                    result.success(true)
                } catch (e: Exception) {
                    result.error("UNAVAILABLE", e.message, null)
                }
            } else {
                result.notImplemented()
            }
        }

        // القناة الخاصة بالتحقق من حالة قفل الشاشة
        MethodChannel(flutterEngine.dartExecutor.binaryMessenger, STATE_CHANNEL).setMethodCallHandler { call, result ->
            if (call.method == "isLocked") {
                val myKM = getSystemService(android.content.Context.KEYGUARD_SERVICE) as android.app.KeyguardManager
                result.success(myKM.isKeyguardLocked)
            } else {
                result.notImplemented()
            }
        }
    }
}