using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Meetings;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ICombinationFactory
    {
        public IEnumerable<Combination> CreateCombinations(IEnumerable<string> input);
    }

    public class CombinationFactory : ICombinationFactory
    {
        public IEnumerable<Combination> CreateCombinations(IEnumerable<string> input)
        {
            var result = new List<Combination>();
            var elementsCount = input.Count();

            for (int i = 0; i < elementsCount; i++)
            {
                for (int j = i + 1; j < elementsCount; j++)
                {
                    result.Add(new Combination(input.ElementAt(i), input.ElementAt(j)));
                    result.Add(new Combination(input.ElementAt(j), input.ElementAt(i)));
                }
            }

            return result;
        }
    }
}