# NoviSample

Sample assigment for assessment, described in detail [here][assigment],

- [X] Library
- [X] API
- [X] Batch Job

## Library

Given the ambiguous interface we're given, we assume Country/Continent are their equivalent names instead of two char codes.
Although we could use RestSharp, Refit or another of advanced libraries for rest calls, the example is simple enough that a simple http client is enough. As for why we are using a static field for HttpClient instead of spinning up new instances as required cf. [You're using HttpClient wrong and it is destabilizing your software][httpClient]. Also, the methods used by our implementation are marked as thread-safe by Microsoft.

An extra interface is provided, ```IpInfoProviderAsync``` to avoid repeated Task.Result calls. Sample console app uses the provided interface, API will use the async method.

Additionally, we shall be using the concrete implementation of IpDetails since we need access to the IP address and don't want to change the specifications of a public interface.

## API

### Persistence

Implemented as a seperate project to hold migrations indepenently of the API.

### Web Service

A barebones API implementation in .Net Core 3.0 and EF Core 3.0 (although, ef-wise we're not using any core-specific features, we could just as easily swap it out for EF 6 by modifying the DI injection). Apart from the boostraped files and the required controller, a structured logging framework is used (outputs only to console and the debug window as a proof of concept) and Swagger (living under ```/swagger``` under the hostname/adress and port of the API) to expose API calls without the need for Postman or client consumers.

### Batch Job

We will implement the batch request job as a BackgroundService, a type of IHostedService used for long running tasks. Given the complexity of a background queue though, and wanting to avoid external message brokers in lieu of a queue, a simple demonstration is provided using an in memory dictionary. Obviously, in a real world scenario, a proper queue with persistence should be used.
Also, since no information is provided on the assigment, we assume that any completed job (with all parsed ip adresses persisted) is discarded, and as such should return a 404 result.

To see the results of our batch job, two additional calls are exposed under the IpController, one accepting an [array of Ip adresses][ipList] in mixed IPv4 and IPv6 format and returning a GUID, and a second call accepting a GUID and returning the current progress of the job.

[assigment]: assigment.pdf
[httpClient]: https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
[modelDefinition]: src/NoviSample.Services/Models/IpDetailResponse.cs
[ipController]: src/NoviSample.Api/Controller/IpController.cs
[ipList]: IpList.txt
