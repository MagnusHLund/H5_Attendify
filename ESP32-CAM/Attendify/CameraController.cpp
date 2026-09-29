#include <Arduino.h>

#include "CameraController.h"
#include "img_converters.h"
#include "Config.h"

CameraController::CameraController(
    Camera *camera,
    Light *light,
    HttpService *httpService)
    : _camera(camera),
      _light(light),
      _httpService(httpService),
      _faceDetectorConfig(mtmn_init_config())
{
}

void CameraController::main()
{
    camera_fb_t *picture = _camera->takePicture();

    if (picture == nullptr)
    {
        Serial.println("Picture is nullptr");
        return;
    }

    if (_isHighResolutionImage)
    {
        Serial.println("High resolution image captured");

        sendImage(picture);

        _camera->setResolution(_camera->_lowResolution);
        _isHighResolutionImage = false;

        return;
    }

    bool facePresent = isFacePresentInPicture(picture);

    esp_camera_fb_return(picture);

    if (!facePresent)
    {
        return;
    }

    _light->setState(LightState::Detected);

    _camera->setResolution(_camera->_highResolution);
    _isHighResolutionImage = true;
}

bool CameraController::isFacePresentInPicture(
    camera_fb_t *picture)
{
    Serial.println("Checking for face");

    dl_matrix3du_t *imageMatrix =
        dl_matrix3du_alloc(
            1,
            picture->width,
            picture->height,
            3);

    if (imageMatrix == nullptr)
    {
        Serial.println("imageMatrix is nullptr");
        return false;
    }

    bool conversionSuccessful = fmt2rgb888(
        picture->buf,
        picture->len,
        picture->format,
        imageMatrix->item);

    if (!conversionSuccessful)
    {
        Serial.println("fmt2rgb888 failed");

        dl_matrix3du_free(imageMatrix);

        return false;
    }

    box_array_t *boxes = face_detect(
        imageMatrix,
        &_faceDetectorConfig);

    bool facePresent = boxes != nullptr;

    if (boxes != nullptr)
    {
        Serial.println("Face detected!");

        dl_lib_free(boxes->score);
        dl_lib_free(boxes->box);
        dl_lib_free(boxes->landmark);
        dl_lib_free(boxes);
    }

    dl_matrix3du_free(imageMatrix);

    return facePresent;
}

void CameraController::sendImage(camera_fb_t *picture)
{
    Serial.print("Sending image: ");
    Serial.print(picture->len);
    Serial.println(" bytes");

    _light->setState(LightState::Uploading);

    bool success = _httpService->uploadImage(
        "/api/attendance",
        picture->buf,
        picture->len,
        CLASSROOM);

    if (success)
    {
        _light->setState(LightState::Success);
        Serial.println("Image uploaded successfully");
    }
    else
    {
        _light->setState(LightState::Failure);
        Serial.println("Image upload failed");
    }

    esp_camera_fb_return(picture);
}