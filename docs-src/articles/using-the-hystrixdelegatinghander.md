# Using the HystrixDelegatingHandler

## Overview

The `HystrixDelegatingHandler` provides circuit breaker, bulkhead, and metrics support to HttpClient.

This handler would typically be the first in the pipeline:

``` cs
RetryDelegatingHandler retryHandler = new RetryDelegatingHandler
{
    InnerHandler = new HttpClientHandler()
};

HystrixDelegatingHandler hystrixHandler = new HystrixDelegatingHandler(commandKey, properties, retryHandler); 

HttpClient httpClient = new HttpClient(hystrixHandler);
```

## Timeouts

The `HystrixDelegatingHandler` does not manage timeouts directly. Instead, use the [HttpClient.Timeout](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.timeout) property. When
the defined timeout expires, `HystrixDelegatingHandler` will intercept the timeout and handle it appropriately.

``` cs
httpClient.Timeout = TimeSpan.FromMilliseconds(properties.ExecutionTimeoutInMilliseconds);
```

## HTTP Status Codes

Failures are counted and emitted for:
 
Status | Reason            | HystrixEvent             | Comments
-------|-------------------|--------------------------|----------
408    | Request Timeout   | HystrixEventType.TIMEOUT |
504    | Gateway Timeout   | HystrixEventType.TIMEOUT |
403    | Forbidden         | HystrixEventType.FAILURE | Some APIs, such as GitHub, return a 403 when a rate limit is reached
429    | Too Many Requests | HystrixEventType.FAILURE | Proposed rate-limit status code. [See RFC 6585](https://tools.ietf.org/html/rfc6585)
>=500  | Server Errors     | HystrixEventType.FAILURE | All server side errors

Http 400 Bad Requests are not counted against failures but do emit a `HystrixEventType.BAD_REQUEST`.

All other Http Status codes are ignored by NHystrix.

## Exceptions and Fallback
 
Like the `HystrixCommand`, the `HystrixDelegatingHandler` does not
throw an exception on error nor does is allow exceptions to propagate up the stack. Rather, it relies
on the fallback implementation to handle error situations.
 
By default, when fallback is enabled, an HttpResponseMessage with a status code of [204 No Content] 
will be returned and `HystrixEventType.FALLBACK_MISSING` emitted if no fallback function is provided.

If fallback is disabled, exceptions will be bubbled up as in a typical message handler. Short-circuits and Semaphore-Rejections will return a [204 No Content].
