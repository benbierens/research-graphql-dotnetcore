using System.Threading.Tasks;
using HotChocolate.Types;

namespace graphql_web
{
    public class Mutation
    {
        public Cat MoveCat(MoveCatInput input)
        {
            return Data.Instance.MoveCat(input.CatIndex, input.CouchIndex);
        }
    }
}