#pragma once

#include "Camera.h"
#include "Light.h"
#include "HttpService.h"
#include "fd_forward.h"

class CameraController
{
private:
  Camera *_camera;
  Light *_light;
  mtmn_config_t _faceDetectorConfig;
  bool _isHighResolutionImage = false;
  HttpService *_httpService;

  bool isFacePresentInPicture(camera_fb_t *picture);
  void sendImage(camera_fb_t *picture);

public:
  CameraController(Camera *camera, Light *light, HttpService *httpService);
  void main();
};