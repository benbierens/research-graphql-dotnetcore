
using System.Linq;

namespace graphql_web
{
    public class Data
    {
        private static Data instance;
        public static Data Instance
        {
            get
            {
                if (instance == null) instance = new Data();
                return instance;
            }
        }

        public Model Model { get; private set; }

        public Cat MoveCat(int catIndex, int couchIndex)
        {
            var cat = Model.Cats[catIndex];
            var oldCouch = cat.Couch;
            var newCouch = Model.Couches[couchIndex];

            cat.Couch = newCouch;
            oldCouch.Cats = oldCouch.Cats.Where(c => c != cat).ToArray();
            newCouch.Cats = newCouch.Cats.Concat(new[] { cat }).ToArray();
            return cat;
        }

        public Data()
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

            Model = new Model
            {
                Cats = cats,
                Couches = couches
            };
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

    public class MoveCatInput
    {
        public int CatIndex { get; set; }
        public int CouchIndex { get; set; }
    }
}
