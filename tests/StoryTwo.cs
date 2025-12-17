using CodingBash.Interfaces;
using CodingBash.Models;
using CodingBash.Workflows;
using tests.TestDoubles;

namespace CodingBash.Tests
{
    public class StoryTwo
    {
        [Fact]
        public async Task SingleToyMachineDeliveredToSleigh()
        {
            Console.WriteLine("---------- Story 2 - TEST 1: Single machine, many elves ----------");

            List<Present> presents = new List<Present>
            {
                new Present("p1", "family1", "Toy1"),
                new Present("p2", "family2", "Toy2"),
                new Present("p3", "family3", "Toy3"),
                new Present("p4", "family4", "Toy4")
            };

            MockToyMachine toyMachine = new MockToyMachine(presents);

            SpyElf elfOne = new SpyElf();
            SpyElf elfTwo = new SpyElf();

            SpySleigh spySleigh = new SpySleigh();
            List<IElf> elves = new List<IElf> { elfOne, elfTwo };
            List<IToyMachine> toyMachines = new List<IToyMachine> { toyMachine };

            MrsClausWorkFlow workflow = new MrsClausWorkFlow( toyMachines, elves, spySleigh);

            await workflow.ConcurrentElvesAndToyMachines();
                        
            Assert.Equal(4, spySleigh.PackedPresents.Count);

            // reorder it as it a bit wack while running concurrently
            List<string> packedIds = spySleigh.PackedPresents
                .Select(p => p.Id)
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(new[] { "p1", "p2", "p3", "p4" }, packedIds);

            // and also a count test
            int totalDelivered = elfOne.DeliveredPresents.Count + elfTwo.DeliveredPresents.Count;
            Assert.Equal(spySleigh.PackedPresents.Count, totalDelivered);
        }

        [Fact]
        public async Task MultipleToyMachinesDeliveredToSleigh()
        {
            Console.WriteLine("---------- Story 2 - TEST 2: manby machines, many elves ----------");

            List<Present> machineOnePresents = new List<Present>
            {
                new Present("m1-p1", "family1", "Toy1"),
                new Present("m1-p2", "family2", "Toy2"),
            };

            List<Present> machineTwoPresents = new List<Present>
            {
                new Present("m2-p1", "family3", "Toy3"),
                new Present("m2-p2", "family4", "Toy4"),
            };

            MockToyMachine machineOne = new MockToyMachine(machineOnePresents);
            MockToyMachine machineTwo = new MockToyMachine(machineTwoPresents);

            SpyElf elfOne = new SpyElf();
            SpyElf elfTwo = new SpyElf();
            SpyElf elfThree = new SpyElf();

            SpySleigh spySleigh = new SpySleigh();

            MrsClausWorkFlow workflow = new MrsClausWorkFlow(
                new List<IToyMachine> { machineOne, machineTwo },
                new List<IElf> { elfOne, elfTwo, elfThree },
                spySleigh);

            await workflow.ConcurrentElvesAndToyMachines();

            Assert.Equal(4, spySleigh.PackedPresents.Count);

            List<string> packedIds = spySleigh.PackedPresents
                .Select(p => p.Id)
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(
                new[] { "m1-p1", "m1-p2", "m2-p1", "m2-p2" },
                packedIds);

            int totalDelivered = elfOne.DeliveredPresents.Count +  elfTwo.DeliveredPresents.Count + elfThree.DeliveredPresents.Count;

            Assert.Equal(spySleigh.PackedPresents.Count, totalDelivered);
        }

        [Fact]
        public async Task NoPresentsTest()
        {
            Console.WriteLine("---------- Story 2 - TEST 3: No presents ----------");

            // Toy machine that immediately returns null
            MockToyMachine emptyMachine = new MockToyMachine(Array.Empty<Present>());

            SpyElf elfOne = new SpyElf();
            SpyElf elfTwo = new SpyElf();

            SpySleigh spySleigh = new SpySleigh();

            MrsClausWorkFlow workflow = new MrsClausWorkFlow(
                new List<IToyMachine> { emptyMachine },
                new List<IElf> { elfOne, elfTwo },
                spySleigh);

            await workflow.ConcurrentElvesAndToyMachines();

            Assert.Empty(spySleigh.PackedPresents);
            Assert.Empty(elfOne.DeliveredPresents);
            Assert.Empty(elfTwo.DeliveredPresents);
        }

        [Fact]
        public async Task NoPresentsAreLostOrDuplicated()
        {
            Console.WriteLine("---------- Story 2 - TEST 4: Concurrency testing----------");

            List<Present> presents = new List<Present>();
            for (int i = 1; i <= 50; i++)
            {
                string id = "p" + i.ToString();
                presents.Add(new Present(id, "family" + i, "Toy " + i));
            }

            MockToyMachine toyMachine = new MockToyMachine(presents);

            SpyElf elfOne = new SpyElf();
            SpyElf elfTwo = new SpyElf();
            SpyElf elfThree = new SpyElf();

            SpySleigh spySleigh = new SpySleigh();

            MrsClausWorkFlow workflow = new MrsClausWorkFlow(
                new List<IToyMachine> { toyMachine },
                new List<IElf> { elfOne, elfTwo, elfThree },
                spySleigh);

            await workflow.ConcurrentElvesAndToyMachines();

            //Count must match
            Assert.Equal(50, spySleigh.PackedPresents.Count);

            // concurrecny wrecks the order so sort them too
            List<string> packedIds = spySleigh.PackedPresents
                .Select(p => p.Id)
                .OrderBy(id => id)
                .ToList();

            List<string> expectedIds = presents
                .Select(p => p.Id)
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(expectedIds, packedIds);
        }
    }
}
