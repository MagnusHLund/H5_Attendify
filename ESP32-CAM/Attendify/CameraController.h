#pragma once

#include "Camera.h"
#include "HttpService.h"
#include "fd_forward.h"

class CameraController {
  private:
    Camera* _camera;
    mtmn_config_t _faceDetectorConfig;
    bool _isHighResolutionImage = false;
    HttpService* _httpService;

    bool isFacePresentInPicture(camera_fb_t* picture);

  public:
    CameraController(Camera* camera, HttpService* httpService);
    void main();
};