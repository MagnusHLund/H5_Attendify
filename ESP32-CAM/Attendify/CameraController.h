#pragma once

#include "Camera.h"
#include "fd_forward.h"

class CameraController {
  private:
    Camera* _camera;
    bool _isHighResolutionImage;
    mtmn_config_t _faceDetectorConfig;

    bool isFacePresentInPicture(camera_fb_t* picture);

  public:
    CameraController(Camera* camera /* Include HttpService*/);
    void main();
};