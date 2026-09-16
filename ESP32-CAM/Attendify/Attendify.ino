#include "Config.h"
#include "WiFiManager.h"

WiFiManager _wifiManager(WIFI_SSID, WIFI_PASSWORD, WIFI_STATIC_IP, WIFI_GATEWAY, WIFI_SUBNET_MASK, WIFI_DNS_SERVER);

unsigned long _lastWiFiKeepAlive = 0;

void setup() {
  Serial.begin(115200);

  _wifiManager.connect();
}

void loop() {
  if(millis() - _lastWiFiKeepAlive > 5000) {
    _wifiManager.ensureConnectivity();
    _lastWiFiKeepAlive = millis();
  }
}
