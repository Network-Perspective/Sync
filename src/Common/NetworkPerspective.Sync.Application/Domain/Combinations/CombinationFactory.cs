using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Application.Domain.Combinations
{
    public class CombinationFactory<T>
    {
        private CombinationFactory()
        { }

        public IEnumerable<Combination<T>> CreateCombinations(IEnumerable<T> input)
        {
            var result = new List<Combination<T>>();
            var elementsCount = input.Count();

            for (int i = 0; i < elementsCount; i++)
            {
                for (int j = i + 1; j < elementsCount; j++)
                {
                    result.Add(new Combination<T>(input.ElementAt(i), input.ElementAt(j)));
                    result.Add(new Combination<T>(input.ElementAt(j), input.ElementAt(i)));
                }
            }

            return result;
        }

        public static IEnumerable<Combination<TT>> CreateCombinations<TT>(IEnumerable<TT> input)
            => new CombinationFactory<TT>().CreateCombinations(input);
    }
}