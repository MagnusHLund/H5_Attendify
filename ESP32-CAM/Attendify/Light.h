#pragma once

#include <Arduino.h>

enum class LightState
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
    byte _pin;
    byte _brightness;

    volatile LightState _state;

    static void taskEntry(void* parameter);
    void run();

public:
    explicit Light(byte pin, byte brightness = 30);

    void setState(LightState state);
};