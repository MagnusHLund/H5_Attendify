#pragma once

#include <Arduino.h>
#include <WiFiClientSecure.h>

class HttpService
{
private:
    const char *_hostname;
    uint16_t _port;

    WiFiClientSecure _client;

public:
    explicit HttpService(
        const char *hostname,
        uint16_t port = 80);

    bool uploadImage(
        const char *urlPath,
        const uint8_t *image,
        size_t imageLength,
        const char *classroom);
};