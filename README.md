# NoviSample

Sample assigment for assessment, described in detail [here][assigment],

- [X] Library
- [X] API
- [ ] Batch Job

## Library

Given the ambiguous interface we're given, we assume Country/Continent are their equivalent names instead of two char codes.
Although we could use RestSharp, Refit or another of advanced libraries for rest calls, the example is simple enough that a simple http client is enough. As for why we are using a static field for HttpClient instead of spinning up new instances as required cf. [You're using HttpClient wrong and it is destabilizing your software][httpClient]. Also, the methods used by our implementation are marked as thread-safe by Microsoft.

An extra interface is provided, ```IpInfoProviderAsync``` to avoid repeated Task.Result calls. Sample console app uses the provided interface, API will use the async method.

## API

### Persistence

Implemented as a seperate project to hold migrations indepenently of the API.

### Web Service

A barebones API implementation in .Net Core 3.0 and EF Core 3.0 (although, ef-wise we're not using any core-specific features, we could just as easily swap it out for EF 6 by modifying the DI injection). Apart from the boostraped files and the required controller, a structured logging framework is used (outputs only to console and the debug window as a proof of concept) and Swagger (living under ```/swagger``` under the hostname the API is hosted under) to expose API calls without the need for Postman or client consumers.

[assigment]: assigment.pdf
[httpClient]: https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
[modelDefinition]: src/NoviSample.Services/Models/IpDetailResponse.cs
[ipController]: src/NoviSample.Api/Controller/IpController.cs
