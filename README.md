Mindscape.Lightspeed.Extensions
=============================

Extensions to help with using Mindscapes' [Lightspeed ORM](http://www.mindscapehq.com/products/lightspeed)


In particular, it enables Full text search using Lucene.Net using Azure blob storage.

The out of the box Mindacape Lightpseed Lucene search engine, requires being able to write to disk.
Azure websites however do not give disk write access.

So this implementation of the Mindscape.LightSpeed.Search.ISearchEngine inteface,
bridges that senario.

It also upgrade the version of Lucene.net to 3.0.3
