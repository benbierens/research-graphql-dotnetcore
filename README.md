I'm figuring out GraphQL from DotNetCore.
This proof of concept must show nested data structure resolution, mutations, and subscriptions in order to be successful.

Use postman or insomnia to do a POST to localhost:5002 with JSON body:
{
"query": "{cats { name couch { location cats { name } } } }"
}
