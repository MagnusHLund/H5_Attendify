#include <Arduino.h>
#include "Camera.h"

bool Camera::init()
{
    _config.ledc_channel = LEDC_CHANNEL_0;
    _config.ledc_timer = LEDC_TIMER_0;

    _config.pin_d0 = Y2_GPIO_NUM;
    _config.pin_d1 = Y3_GPIO_NUM;
    _config.pin_d2 = Y4_GPIO_NUM;
    _config.pin_d3 = Y5_GPIO_NUM;
    _config.pin_d4 = Y6_GPIO_NUM;
    _config.pin_d5 = Y7_GPIO_NUM;
    _config.pin_d6 = Y8_GPIO_NUM;
    _config.pin_d7 = Y9_GPIO_NUM;

    _config.pin_xclk = XCLK_GPIO_NUM;
    _config.pin_pclk = PCLK_GPIO_NUM;
    _config.pin_vsync = VSYNC_GPIO_NUM;
    _config.pin_href = HREF_GPIO_NUM;

    _config.pin_sscb_sda = SIOD_GPIO_NUM;
    _config.pin_sscb_scl = SIOC_GPIO_NUM;

    _config.pin_pwdn = PWDN_GPIO_NUM;
    _config.pin_reset = RESET_GPIO_NUM;

    _config.xclk_freq_hz = 20000000;
    _config.pixel_format = PIXFORMAT_JPEG;

    _config.frame_size = _lowResolution;
    _config.jpeg_quality = 12;
    _config.fb_count = 1;

    return esp_camera_init(&_config) == ESP_OK;
}

camera_fb_t* Camera::takePicture()
{
    Serial.println("Taking picture");
    return esp_camera_fb_get();
}

void Camera::setResolution(framesize_t resolution)
{
    Serial.println("Setting resolution to " + String(resolution));
    sensor_t* sensor = esp_camera_sensor_get();

    if (sensor != nullptr)
    {
        sensor->set_framesize(sensor, resolution);
    }
}
