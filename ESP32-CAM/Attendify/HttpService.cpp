#include "HttpService.h"

#include <WiFi.h>
#include <HTTPClient.h>

HttpService::HttpService(const char* baseUrl)
    : _baseUrl(baseUrl)
{
}

bool HttpResponse::isSuccessful() const
{
    Serial.println("Request succeded with a response of: " + String(statusCode));
    return statusCode >= 200 && statusCode < 300;
}

String HttpService::buildUrl(const char* endpoint) const
{
    String url = _baseUrl;

    if (!url.endsWith("/") && endpoint[0] != '/')
    {
        url += "/";
    }

    if (url.endsWith("/") && endpoint[0] == '/')
    {
        url.remove(url.length() - 1);
    }

    url += endpoint;

    return url;
}

HttpResponse HttpService::request(
    const char* method,
    const char* endpoint,
    uint8_t* body,
    size_t bodyLength,
    const char* contentType
)
{
    HttpResponse response{
        .statusCode = -1,
        .body = ""
    };

    if (WiFi.status() != WL_CONNECTED)
    {
        response.body = "WiFi is not connected";
        return response;
    }

    WiFiClient client;
    HTTPClient http;

    String url = buildUrl(endpoint);

    if (!http.begin(client, url))
    {
        response.body = "Failed to initialize HTTP client";
        return response;
    }

    if (contentType != nullptr)
    {
        http.addHeader("Content-Type", contentType);
    }

    int statusCode;

    if (body != nullptr && bodyLength > 0)
    {
        statusCode = http.sendRequest(method, body, bodyLength);
    }
    else
    {
        statusCode = http.sendRequest(method);
    }

    response.statusCode = statusCode;

    if (statusCode > 0)
    {
        response.body = http.getString();
        Serial.println("Request returned status code: " + String(statusCode) + " and body:" + response.body);
    }
    else
    {
        response.body = http.errorToString(statusCode);
        Serial.println("Request failed with status code: " + String(statusCode) + " and body:" + response.body);
    }

    http.end();

    return response;
}