using CodingBash.Interfaces;
using CodingBash.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests.TestDoubles
{
    public class MockToyMachine : IToyMachine
    {
        private readonly Queue<Present> _presentQueue;

        public MockToyMachine(IEnumerable<Present> presents)
        {
            _presentQueue = new Queue<Present>(presents);
        }

        public Task<Present> GetNextPresent()
        {
            if (_presentQueue.Count == 0)
            {
                Console.WriteLine("MockToyMachine - Out of presents!");
                return Task.FromResult<Present>(null);
            }

            Present next = _presentQueue.Dequeue();
            Console.WriteLine($"ToyMachine - Present: {next.Id}");
            return Task.FromResult(next);
        }
    }
}
