
using System.Linq;

namespace graphql_web
{
    public class Query
    {
        private Model model;

        public Query()
        {
            var cats = new[]{
                 new Cat { Name = "Biggie" },
                 new Cat { Name = "Pusheen" },
                 new Cat { Name = "Baltasar" },
            };

            var couches = new[]{
                new Couch { Location = "LivingRoom" },
                new Couch { Location = "WorkRoom" },
            };

            cats[0].Couch = couches[0];
            cats[1].Couch = couches[0];
            cats[2].Couch = couches[1];

            couches[0].Cats = cats.Take(2).ToArray();
            couches[1].Cats = cats.Skip(2).Take(1).ToArray();

            model = new Model
            {
                Cats = cats,
                Couches = couches
            };
        }

        public Cat[] Cats()
        {
            return model.Cats;
        }

        public Couch[] Couches()
        {
            return model.Couches;
        }
    }

    public class Model
    {
        public Cat[] Cats { get; set; }
        public Couch[] Couches { get; set; }
    }

    public class Cat
    {
        public string Name { get; set; }
        public Couch Couch { get; set; }
    }

    public class Couch
    {
        public string Location { get; set; }
        public Cat[] Cats { get; set; }
    }
}