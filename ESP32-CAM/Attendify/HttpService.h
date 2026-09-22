#pragma once

#include <Arduino.h>

struct HttpResponse
{
    int statusCode;
    String body;

    bool isSuccessful() const;
};

class HttpService
{
  public:
    explicit HttpService(const char* baseUrl);

    HttpResponse request(
        const char* method,
        const char* endpoint,
        uint8_t* body = nullptr,
        size_t bodyLength = 0,
        const char* contentType = nullptr
    );

  private:
    String _baseUrl;

    String buildUrl(const char* endpoint) const;
};