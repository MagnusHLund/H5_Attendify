#include "Light.h"

namespace
{
    constexpr uint8_t PWM_CHANNEL = 2;
    constexpr uint32_t PWM_FREQUENCY = 5000;
    constexpr uint8_t PWM_RESOLUTION = 8;
}

Light::Light(byte pin, byte brightness)
    : _pin(pin),
      _brightness(brightness),
      _state(LightState::Off)
{
    ledcSetup(
        PWM_CHANNEL,
        PWM_FREQUENCY,
        PWM_RESOLUTION
    );

    ledcAttachPin(
        _pin,
        PWM_CHANNEL
    );

    ledcWrite(
        PWM_CHANNEL,
        0
    );

    xTaskCreate(
        taskEntry,
        "LightTask",
        2048,
        this,
        1,
        nullptr
    );
}

void Light::setState(LightState state)
{
    _state = state;
}

void Light::taskEntry(void* parameter)
{
    auto* light = static_cast<Light*>(parameter);

    light->run();

    vTaskDelete(nullptr);
}

void Light::run()
{
    while (true)
    {
        switch (_state)
        {
            case LightState::Off:
            {
                ledcWrite(PWM_CHANNEL, 0);

                vTaskDelay(pdMS_TO_TICKS(100));
                break;
            }

            case LightState::Detected:
            {
                ledcWrite(PWM_CHANNEL, _brightness);
                vTaskDelay(pdMS_TO_TICKS(100));

                ledcWrite(PWM_CHANNEL, 0);

                _state = LightState::Off;

                vTaskDelay(pdMS_TO_TICKS(100));
                break;
            }

            case LightState::Uploading:
            {
                ledcWrite(PWM_CHANNEL, _brightness);
                vTaskDelay(pdMS_TO_TICKS(300));

                ledcWrite(PWM_CHANNEL, 0);
                vTaskDelay(pdMS_TO_TICKS(500));

                break;
            }

            case LightState::Success:
            {
                for (int i = 0; i < 2; i++)
                {
                    ledcWrite(PWM_CHANNEL, _brightness);
                    vTaskDelay(pdMS_TO_TICKS(100));

                    ledcWrite(PWM_CHANNEL, 0);
                    vTaskDelay(pdMS_TO_TICKS(100));
                }

                _state = LightState::Off;

                break;
            }

            case LightState::Failure:
            {
                for (int i = 0; i < 3; i++)
                {
                    ledcWrite(PWM_CHANNEL, _brightness);
                    vTaskDelay(pdMS_TO_TICKS(150));

                    ledcWrite(PWM_CHANNEL, 0);
                    vTaskDelay(pdMS_TO_TICKS(150));
                }

                _state = LightState::Off;

                break;
            }
        }
    }
}