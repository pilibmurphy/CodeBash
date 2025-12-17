using CodingBash.Interfaces;
using CodingBash.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests.TestDoubles
{
    public class SpyElf : IElf
    {
        public List<Present> DeliveredPresents { get; }

        public SpyElf()
        {
            DeliveredPresents = new List<Present>();
        }

        public async Task Deliver(Present present, ISleigh sleigh)
        {
            Console.WriteLine($"Elf - Delivering present: {present.Id}");
            DeliveredPresents.Add(present);
            await sleigh.PackPresent(present);
        }
    }
}
