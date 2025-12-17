using CodingBash.Interfaces;
using CodingBash.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests.TestDoubles
{
    public class SpySleigh : ISleigh
    {

		public List<Present> PackedPresents { get; }

		public SpySleigh()
        {
            PackedPresents = new List<Present>();
        }

        public Task PackPresent(Present present)
        {
            Console.WriteLine($"Sleigh - Packing present: {present.Id}");
            PackedPresents.Add(present);
            return Task.CompletedTask;
        }
    }
}
