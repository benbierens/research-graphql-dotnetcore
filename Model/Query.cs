using System.Threading.Tasks;
using HotChocolate.Types;

namespace graphql_web
{
    public class Query
    {
        public async Task<Cat[]> Cats()
        {
            await Task.Run(() => { });
            return Data.Instance.Model.Cats;
        }

        public Couch[] Couches()
        {
            return Data.Instance.Model.Couches;
        }
    }
}
