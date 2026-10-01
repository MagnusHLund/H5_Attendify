
#include "HttpService.h"

HttpService::HttpService(
    const char *hostname,
    uint16_t port)
    : _hostname(hostname),
      _port(port)
{
    Serial.println("[HTTP] HttpService created");
    Serial.print("[HTTP] Host: ");
    Serial.println(_hostname);
    Serial.print("[HTTP] Port: ");
    Serial.println(_port);

    _client.setInsecure(); // Disable certificate verification
}

bool HttpService::uploadImage(
    const char *urlPath,
    const uint8_t *image,
    size_t imageLength,
    const char *classroom)
{
    Serial.println();
    Serial.println("========== HTTP UPLOAD ==========");

    Serial.print("[HTTP] Host: ");
    Serial.println(_hostname);

    Serial.print("[HTTP] Port: ");
    Serial.println(_port);

    Serial.print("[HTTP] Path: ");
    Serial.println(urlPath);

    Serial.print("[HTTP] Image size: ");
    Serial.print(imageLength);
    Serial.println(" bytes");

    Serial.print("[HTTP] Classroom: ");
    Serial.println(classroom);

    // --------------------------------------------------
    // TCP connection
    // --------------------------------------------------

    Serial.println("[HTTP] Connecting...");

    unsigned long connectStart = millis();

    if (!_client.connect(_hostname, _port))
    {
        Serial.print("[HTTP] Connection failed after ");
        Serial.print(millis() - connectStart);
        Serial.println(" ms");

        Serial.print("[HTTP] Client connected: ");
        Serial.println(_client.connected());

        Serial.print("[HTTP] Client available: ");
        Serial.println(_client.available());

        _client.stop();

        Serial.println("=================================");
        return false;
    }

    Serial.print("[HTTP] Connected in ");
    Serial.print(millis() - connectStart);
    Serial.println(" ms");

    // --------------------------------------------------
    // Build multipart body
    // --------------------------------------------------

    constexpr char boundary[] =
        "AttendifyBoundary7MA4YWxkTrZu0gW";

    String multipartHeader;

    multipartHeader += "--";
    multipartHeader += boundary;
    multipartHeader += "\r\n";

    multipartHeader +=
        "Content-Disposition: form-data; name=\"classroom\"\r\n\r\n";

    multipartHeader += classroom;
    multipartHeader += "\r\n";

    multipartHeader += "--";
    multipartHeader += boundary;
    multipartHeader += "\r\n";

    multipartHeader +=
        "Content-Disposition: form-data; "
        "name=\"picture\"; "
        "filename=\"capture.jpg\"\r\n";

    multipartHeader +=
        "Content-Type: image/jpeg\r\n\r\n";

    String multipartFooter =
        "\r\n--" +
        String(boundary) +
        "--\r\n";

    const size_t contentLength =
        multipartHeader.length() +
        imageLength +
        multipartFooter.length();

    Serial.print("[HTTP] Multipart header: ");
    Serial.print(multipartHeader.length());
    Serial.println(" bytes");

    Serial.print("[HTTP] Image: ");
    Serial.print(imageLength);
    Serial.println(" bytes");

    Serial.print("[HTTP] Multipart footer: ");
    Serial.print(multipartFooter.length());
    Serial.println(" bytes");

    Serial.print("[HTTP] Content-Length: ");
    Serial.print(contentLength);
    Serial.println(" bytes");

    // --------------------------------------------------
    // Request
    // --------------------------------------------------

    Serial.println("[HTTP] Sending request headers...");

    _client.print("POST ");
    _client.print(urlPath);
    _client.println(" HTTP/1.1");

    _client.print("Host: ");
    _client.println(_hostname);

    _client.print("Content-Type: multipart/form-data; boundary=");
    _client.println(boundary);

    _client.print("Content-Length: ");
    _client.println(contentLength);

    _client.println("Connection: close");
    _client.println();

    Serial.println("[HTTP] Request headers sent");

    // --------------------------------------------------
    // Multipart header
    // --------------------------------------------------

    _client.print(multipartHeader);

    Serial.println("[HTTP] Multipart header sent");

    // --------------------------------------------------
    // Image
    // --------------------------------------------------

    Serial.println("[HTTP] Uploading image...");

    unsigned long uploadStart = millis();

    size_t sent = 0;

    while (sent < imageLength)
    {
        size_t written = _client.write(
            image + sent,
            imageLength - sent);

        if (written == 0)
        {
            Serial.println("[HTTP] Failed to write image");

            _client.stop();

            return false;
        }

        sent += written;

        Serial.print("[HTTP] ");
        Serial.print(sent);
        Serial.print("/");
        Serial.print(imageLength);
        Serial.println(" bytes");
    }

    Serial.print("[HTTP] Image uploaded in ");
    Serial.print(millis() - uploadStart);
    Serial.println(" ms");

    // --------------------------------------------------
    // Multipart footer
    // --------------------------------------------------

    _client.print(multipartFooter);

    Serial.println("[HTTP] Multipart footer sent");

    // --------------------------------------------------
    // Response
    // --------------------------------------------------

    Serial.println("[HTTP] Waiting for response...");

    constexpr unsigned long RESPONSE_TIMEOUT = 10000;

    unsigned long responseStart = millis();

    String statusLine;
    int statusCode = -1;

    // --------------------------------------------------
    // Read status line
    // --------------------------------------------------

    while (millis() - responseStart < RESPONSE_TIMEOUT)
    {
        if (_client.available())
        {
            statusLine = _client.readStringUntil('\n');

            statusLine.trim();

            Serial.print("[HTTP] < ");
            Serial.println(statusLine);

            // Example:
            // HTTP/1.1 200 OK

            int firstSpace = statusLine.indexOf(' ');

            if (firstSpace >= 0)
            {
                int secondSpace =
                    statusLine.indexOf(' ', firstSpace + 1);

                if (secondSpace >= 0)
                {
                    statusCode = statusLine
                        .substring(firstSpace + 1, secondSpace)
                        .toInt();
                }
            }

            break;
        }

        if (!_client.connected())
        {
            Serial.println("[HTTP] Connection closed before response");

            break;
        }

        delay(1);
    }

    if (statusLine.length() == 0)
    {
        Serial.println("[HTTP] Response timeout");

        _client.stop();

        Serial.println("========== HTTP COMPLETE ==========");

        return false;
    }

    // --------------------------------------------------
    // Read response headers
    // --------------------------------------------------

    bool headersComplete = false;

    while (millis() - responseStart < RESPONSE_TIMEOUT)
    {
        if (_client.available())
        {
            String line = _client.readStringUntil('\n');

            line.trim();

            Serial.print("[HTTP] < ");
            Serial.println(line);

            if (line.length() == 0)
            {
                headersComplete = true;
                break;
            }
        }
        else
        {
            if (!_client.connected())
            {
                break;
            }

            delay(1);
        }
    }

    Serial.print("[HTTP] Response received in ");
    Serial.print(millis() - responseStart);
    Serial.println(" ms");

    // --------------------------------------------------
    // Response body
    // --------------------------------------------------

    if (headersComplete)
    {
        unsigned long bodyStart = millis();

        while (millis() - bodyStart < RESPONSE_TIMEOUT)
        {
            if (_client.available())
            {
                String response = _client.readString();

                Serial.println("[HTTP] Response body:");
                Serial.println(response);

                break;
            }

            if (!_client.connected())
            {
                break;
            }

            delay(1);
        }
    }

    _client.stop();

    Serial.print("[HTTP] Status code: ");
    Serial.println(statusCode);

    Serial.println("========== HTTP COMPLETE ==========");

    return statusCode >= 200 && statusCode < 300;
}