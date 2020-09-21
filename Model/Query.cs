using System.Threading.Tasks;
using HotChocolate.Types;

namespace graphql_web
{
    public class Query
    {
        public Cat[] Cats()
        {
            return Data.Instance.Model.Cats;
        }

        public Couch[] Couches()
        {
            return Data.Instance.Model.Couches;
        }
    }
}
