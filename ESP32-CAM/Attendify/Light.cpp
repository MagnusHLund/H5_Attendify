#include "Light.h"

namespace
{
    constexpr uint8_t PWM_CHANNEL = 2;
    constexpr uint32_t PWM_FREQUENCY = 5000;
    constexpr uint8_t PWM_RESOLUTION = 8;
}

Light::Light(
    uint8_t pin,
    uint8_t brightness)
    : _pin(pin),
      _brightness(brightness),
      _taskHandle(nullptr)
{
    ledcSetup(
        PWM_CHANNEL,
        PWM_FREQUENCY,
        PWM_RESOLUTION);

    ledcAttachPin(
        _pin,
        PWM_CHANNEL);

    ledcWrite(
        PWM_CHANNEL,
        0);

    xTaskCreate(
        taskEntry,
        "LightTask",
        2048,
        this,
        1,
        &_taskHandle);
}

void Light::setState(LightState state)
{
    if (_taskHandle == nullptr)
    {
        return;
    }

    xTaskNotify(
        _taskHandle,
        static_cast<uint32_t>(state),
        eSetValueWithOverwrite);
}

void Light::taskEntry(void *parameter)
{
    auto *light =
        static_cast<Light *>(parameter);

    light->run();

    vTaskDelete(nullptr);
}

void Light::run()
{
    LightState state = LightState::Off;

    while (true)
    {
        uint32_t notificationValue;

        // Check for a new state without blocking.
        if (xTaskNotifyWait(
                0,
                UINT32_MAX,
                &notificationValue,
                0) == pdTRUE)
        {
            state = static_cast<LightState>(
                notificationValue);
        }

        switch (state)
        {
        case LightState::Off:
        {
            ledcWrite(PWM_CHANNEL, 0);

            // Stay here until another state arrives.
            if (xTaskNotifyWait(
                    0,
                    UINT32_MAX,
                    &notificationValue,
                    portMAX_DELAY) == pdTRUE)
            {
                state = static_cast<LightState>(
                    notificationValue);
            }

            break;
        }

        case LightState::Detected:
        {
            ledcWrite(
                PWM_CHANNEL,
                _brightness);

            if (xTaskNotifyWait(
                    0,
                    UINT32_MAX,
                    &notificationValue,
                    pdMS_TO_TICKS(100)) == pdTRUE)
            {
                state = static_cast<LightState>(
                    notificationValue);

                ledcWrite(PWM_CHANNEL, 0);
                break;
            }

            ledcWrite(
                PWM_CHANNEL,
                0);

            state = LightState::Off;

            break;
        }

        case LightState::Uploading:
        {
            ledcWrite(
                PWM_CHANNEL,
                _brightness);

            if (xTaskNotifyWait(
                    0,
                    UINT32_MAX,
                    &notificationValue,
                    pdMS_TO_TICKS(300)) == pdTRUE)
            {
                state = static_cast<LightState>(
                    notificationValue);

                ledcWrite(PWM_CHANNEL, 0);
                break;
            }

            ledcWrite(
                PWM_CHANNEL,
                0);

            if (xTaskNotifyWait(
                    0,
                    UINT32_MAX,
                    &notificationValue,
                    pdMS_TO_TICKS(500)) == pdTRUE)
            {
                state = static_cast<LightState>(
                    notificationValue);
            }

            break;
        }

        case LightState::Success:
        {
            bool interrupted = false;

            for (int i = 0; i < 2; i++)
            {
                ledcWrite(
                    PWM_CHANNEL,
                    _brightness);

                if (xTaskNotifyWait(
                        0,
                        UINT32_MAX,
                        &notificationValue,
                        pdMS_TO_TICKS(100)) == pdTRUE)
                {
                    state = static_cast<LightState>(
                        notificationValue);

                    interrupted = true;
                    break;
                }

                ledcWrite(
                    PWM_CHANNEL,
                    0);

                if (xTaskNotifyWait(
                        0,
                        UINT32_MAX,
                        &notificationValue,
                        pdMS_TO_TICKS(100)) == pdTRUE)
                {
                    state = static_cast<LightState>(
                        notificationValue);

                    interrupted = true;
                    break;
                }
            }

            ledcWrite(
                PWM_CHANNEL,
                0);

            if (!interrupted)
            {
                state = LightState::Off;
            }

            break;
        }

        case LightState::Failure:
        {
            bool interrupted = false;

            for (int i = 0; i < 3; i++)
            {
                ledcWrite(
                    PWM_CHANNEL,
                    _brightness);

                if (xTaskNotifyWait(
                        0,
                        UINT32_MAX,
                        &notificationValue,
                        pdMS_TO_TICKS(150)) == pdTRUE)
                {
                    state = static_cast<LightState>(
                        notificationValue);

                    interrupted = true;
                    break;
                }

                ledcWrite(
                    PWM_CHANNEL,
                    0);

                if (xTaskNotifyWait(
                        0,
                        UINT32_MAX,
                        &notificationValue,
                        pdMS_TO_TICKS(150)) == pdTRUE)
                {
                    state = static_cast<LightState>(
                        notificationValue);

                    interrupted = true;
                    break;
                }
            }

            ledcWrite(
                PWM_CHANNEL,
                0);

            if (!interrupted)
            {
                state = LightState::Off;
            }

            break;
        }
        }
    }
}