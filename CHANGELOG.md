# Change Log

## Unreleased

- Request Caching
- Dashboard

## [0.5.1]

### Bug Fixes

Fixed a bug in `HystrixCommandGroup.AddCommandKey` where multiple keys didn't get added.

## [0.5.0]

### Beaking changes

Extracted HttpClient support into its own library.

## [0.4.0]

## Changes

Support for .Net Core 3.0.

Multi-Targeted for older supported frameworks and tooling:

- net452
- net461
- netStandard2.0
- netCoreApp3.0

## [0.3.0]

### Added

Asychronous execution support using the `IHystrixCommand.Queue()` method. This method is non-blocking uses a producer/consumer pattern
under the hood. A callback action can invoked when the command completes.

## [0.2.0]

There are some pretty major breaking changes in this release as we bring more of the Hystrix functionality and design
into the APIs. We've also deviated from the original Hystrix implementation a bit more and use typed exceptions rather
than a single HystrixRuntimeException and an enum.

We'll try to keep breaking changes to a minimum going forward unless it's to fix a major design flaw or to greatly improve
reliability, performance, and usability. No promises :)  If breaking it makes it better, we'll do it. At least until 
we reach a 1.0 release.

### Added

- A delegating message handler for HttpClient (`HystrixDelegatingHandler`) that implements
  - Circuit Breaker Pattern
  - Bulkhead Pattern
  - Metrics
- Expanded the `IHystrixCircuitBreaker` beyond the original Netflix version
- Typed Exceptions

### Changes (non-breaking)

- `Execute()` and `ExecuteAsync()` methods now accept an optional request parameter. This allows a `HystrixCommand` to
be a Singleton and the request passed into Execute() rather than the constructor.

### Breaking Changes

- The circuit breaker properties have been refactored in to their own class `CircuitBreakerOptions`. The 
`HystrixCommandProperties` now has a property of type `CircuitBreakerOptions`. This eliminates confusion as to 
exactly which configuration properties directly affect the circuit breaker and only those options are passed to 
the `HystrixCircuitBreaker` on construction.  We will likely do the same in metrics in a future release when metrics 
is built out further.

- Timeouts now throw a `HystrixTimeoutException` instead of a `System.TimoutException`. `HystrixTimeoutException` 
derives from `HystrixRuntimeException` and ensures the `HystrixCommand` doesn't double count when exceptions are rethrown.

- When fallback is disabled, the original exception is wrapped in a HystrixRuntimeExtension or subclass rather than just
throwing the original exception.

- `HystrixCommand` has an additional generic type parameter for the request.

## [0.1.0] - Initial release

- HystrixCommand with:
  - Circuit Breaker Pattern (https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
  - Bulkhead Pattern (https://docs.microsoft.com/en-us/azure/architecture/patterns/bulkhead)
  - Timeouts
- Minimum metrics required to support the Circuit Breaker
