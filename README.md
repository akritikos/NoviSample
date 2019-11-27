# NoviSample

Sample assigment for assessment, described in detail [here][assigment],

- [X] Library
- [ ] API
- [ ] Batch Job

## Library

Given the ambiguous interface we're given, we assume Country/Continent are their equivalent names instead of two char codes.
Although we could use RestSharp, Refit or another of advanced libraries for rest calls, the example is simple enough that a simple http client is enough. As for why we are using a static field for HttpClient instead of spinning up new instances as required cf. [You're using HttpClient wrong and it is destabilizing your software][httpClient].

## API

### Persistence

Implemented as a seperate project to hold migrations indepenently of the API. To avoid needless complication we reuse the [model][modelDefinition] we defined in the library as a table definition by adding a few more fields as seen in [here][akritikos/NoviSample@5c0a35b]

[assigment]: assigment.pdf
[httpClient]: https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
[modelDefinition]: src/NoviSample.Services/Models/IpDetailResponse.cs
