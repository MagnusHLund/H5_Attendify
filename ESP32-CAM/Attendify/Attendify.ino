#include "Config.h"
#include "Camera.h"
#include "WiFiManager.h"
#include "CameraController.h"

WiFiManager* _wifiManager;
Camera* _camera;

CameraController* _cameraController;

unsigned long _lastWiFiKeepAlive = 0;

void setup() {
  Serial.begin(115200);

  _wifiManager = new WiFiManager(WIFI_SSID, WIFI_PASSWORD, WIFI_STATIC_IP, WIFI_GATEWAY, WIFI_SUBNET_MASK);
  _camera = new Camera();

  _cameraController = new CameraController(_camera);

  _wifiManager->connect();
  _camera->init();

  Serial.println("Setup complete");
}

void loop() {
  _cameraController->main();

  if(millis() - _lastWiFiKeepAlive > 5000) {
    _wifiManager->ensureConnectivity();
    _lastWiFiKeepAlive = millis();
  }
}
