#include "CameraController.h"
#include "img_converters.h"
#include "fd_forward.h"

CameraController::CameraController(Camera* camera)
  : _camera(camera), _faceDetectorConfig(mtmn_init_config()) {}

void CameraController::main() {
  camera_fb_t* picture = _camera->takePicture();

  if (picture == nullptr) {
    return;
  }

  bool isFacePresent = isFacePresentInPicture(picture);

  if (!isFacePresent) {
    _camera->setResolution(_camera->_lowResolution);
    _isHighResolutionImage = false;
    return;
  }

  if(isFacePresent && !_isHighResolutionImage)
  {
    _camera->setResolution(_camera->_highResolution);
    _isHighResolutionImage = true;
    return;
  }

  // TODO: Send image via HTTP
}

bool CameraController::isFacePresentInPicture(camera_fb_t* picture)
{
    dl_matrix3du_t* imageMatrix =
        dl_matrix3du_alloc(
            1,
            picture->width,
            picture->height,
            3
        );

    if (imageMatrix == nullptr)
    {
        return false;
    }

    bool conversionSuccessful = fmt2rgb888(
        picture->buf,
        picture->len,
        picture->format,
        imageMatrix->item
    );

    if (!conversionSuccessful)
    {
        dl_matrix3du_free(imageMatrix);
        return false;
    }

    box_array_t* boxes = face_detect(
        imageMatrix,
        &_faceDetectorConfig
    );

    bool facePresent = boxes != nullptr;

    if (boxes != nullptr)
    {
        free(boxes->box);
        free(boxes->landmark);
        free(boxes);
    }

    dl_matrix3du_free(imageMatrix);

    return facePresent;
}