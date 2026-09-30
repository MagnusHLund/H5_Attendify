#pragma once

#include <Arduino.h>

enum class LightState : uint8_t
{
    Off,
    Detected,
    Uploading,
    Success,
    Failure
};

class Light
{
private:
    uint8_t _pin;
    uint8_t _brightness;

    TaskHandle_t _taskHandle;

    static void taskEntry(void *parameter);

    void run();

    bool waitForState(
        TickType_t timeout);

public:
    explicit Light(
        uint8_t pin,
        uint8_t brightness = 30);

    void setState(LightState state);
};