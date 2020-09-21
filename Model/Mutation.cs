using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Subscriptions;

namespace graphql_web
{
    public class Mutation
    {
        public async Task<Cat> MoveCat(MoveCatInput input, [Service] ITopicEventSender sender)
        {
            var result = Data.Instance.MoveCat(input.CatIndex, input.CouchIndex);

            await sender.SendAsync("OnCatChanged", result);

            return result;
        }
    }
}