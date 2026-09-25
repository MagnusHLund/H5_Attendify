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
        uint8_t* body,
        size_t bodyLength,
        const char* contentType
    );

    HttpResponse request(
        const char* method,
        const char* endpoint,
        const String& body,
        const char* contentType
    );

  private:
    String _baseUrl;

    String buildUrl(const char* endpoint) const;
};