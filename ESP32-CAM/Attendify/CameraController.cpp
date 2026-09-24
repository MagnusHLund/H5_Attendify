#include <Arduino.h>
#include "CameraController.h"
#include "img_converters.h"
#include "fd_forward.h"
#include "base64.h"
#include "config.h"

CameraController::CameraController(Camera* camera, HttpService* httpService)
  : _camera(camera), _httpService(httpService), _faceDetectorConfig(mtmn_init_config()) {}

void CameraController::main()
{
    camera_fb_t* picture = _camera->takePicture();

    if (picture == nullptr)
    {
        Serial.println("Picture is nullptr");
        return;
    }

    if (_isHighResolutionImage)
    {
        Serial.println("High resolution image captured");

        base64 encoder;

        String encodedPicture = encoder.encode(
            picture->buf,
            picture->len
        );

        String json = "{\"classroom\":\"";
        json += CLASSROOM;
        json += "\",\"picture\":\"";
        json += encodedPicture;
        json += "\"}";

        HttpResponse response = _httpService->request(
            "POST",
            "/api/attendance",
            json,
            "application/json"
        );

        esp_camera_fb_return(picture);

        _camera->setResolution(_camera->_lowResolution);
        _isHighResolutionImage = false;

        return;
    }

    bool isFacePresent = isFacePresentInPicture(picture);

    esp_camera_fb_return(picture);

    if (!isFacePresent)
    {
        return;
    }

    _camera->setResolution(_camera->_highResolution);
    _isHighResolutionImage = true;
}

bool CameraController::isFacePresentInPicture(camera_fb_t* picture)
{
    Serial.println("Checking for face");

    dl_matrix3du_t* imageMatrix =
        dl_matrix3du_alloc(
            1,
            picture->width,
            picture->height,
            3
        );

    if (imageMatrix == nullptr)
    {
        Serial.println("imageMatrix is nullptr");
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
        Serial.println("fmt2rgb888 failed");
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
        Serial.println("Face detected!");

        dl_lib_free(boxes->score);
        dl_lib_free(boxes->box);
        dl_lib_free(boxes->landmark);
        dl_lib_free(boxes);
    }

    dl_matrix3du_free(imageMatrix);

    return facePresent;
}