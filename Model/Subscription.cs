using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;

namespace graphql_web
{
    public class Subscription
    {
        [Subscribe]
        public Cat OnCatChanged([EventMessage] Cat cat) => cat;
    }
}
