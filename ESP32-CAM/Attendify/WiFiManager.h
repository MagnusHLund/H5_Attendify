#pragma once

#include <Arduino.h>
#include <WiFi.h>

class WiFiManager {
  private:
    const char* _ssid;
    const char* _password;

    IPAddress _localIp;
    IPAddress _dnsServer;
    IPAddress _gateway;
    IPAddress _subnet;

  public:
    WiFiManager(const char* ssid, const char* password, String localIp, String gateway, String subnet, String dnsServer = "1.1.1.1");
    void connect();
    void ensureConnectivity();
};