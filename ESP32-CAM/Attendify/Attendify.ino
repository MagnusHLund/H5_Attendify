#include "Config.h"
#include "Camera.h"
#include "Light.h"
#include "HttpService.h"
#include "WiFiManager.h"
#include "CameraController.h"
#include <WiFiClientSecure.h>

WiFiManager* _wifiManager;
HttpService* _httpService;
Camera* _camera;
Light* _light;

CameraController* _cameraController;

unsigned long _lastWiFiKeepAlive = 0;

void setup() {
  Serial.begin(115200);

  _wifiManager = new WiFiManager(WIFI_SSID, WIFI_PASSWORD, WIFI_STATIC_IP, WIFI_GATEWAY, WIFI_SUBNET_MASK);
  _camera = new Camera();
  _light = new Light(LIGHT_PIN, 1);

  _httpService = new HttpService(API_BASE, API_PORT);
  _cameraController = new CameraController(_camera, _light, _httpService);

  _wifiManager->connect();
  
  while (!_camera->init())
  {
      Serial.println("Camera initialization failed. Retrying...");
      ESP.restart();
  }

  Serial.println("Setup complete");
}

void loop() {
  _cameraController->main();

  if(millis() - _lastWiFiKeepAlive > 5000) {
    _wifiManager->ensureConnectivity();
    _lastWiFiKeepAlive = millis();
  }
}
