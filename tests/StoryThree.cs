using CodingBash.Interfaces;
using CodingBash.Models;
using CodingBash.Workflows;
using System.Linq;
using tests.TestDoubles;

namespace CodingBash.Tests
{
    public class StoryThree
    {
        [Fact]
        public async Task MultipleFamiliesTest()
        {
            Console.WriteLine("---------- Story 3 - TEST 1: families ----------");

            List<Present> presents = new List<Present>();

            for (int i = 1; i <= 10; i++)
            {
                presents.Add(new Present("A-" + i, "FamilyA", "Toy A " + i));
                presents.Add(new Present("B-" + i, "FamilyB", "Toy B " + i));
                presents.Add(new Present("C-" + i, "FamilyC", "Toy C " + i));
            }

            IToyMachine toyMachine = new MockToyMachine(presents);

            IElf elfOne = new SpyElf();
            IElf elfTwo = new SpyElf();
            IElf elfThree = new SpyElf();

            SpySleigh sleigh = new SpySleigh();
            List<IElf> elves = new List<IElf> { elfOne, elfTwo, elfThree };

            MrsClausWorkFlow mrsClauseWorkflow = new MrsClausWorkFlow( new List<IToyMachine> { toyMachine }, elves , sleigh);

            await mrsClauseWorkflow.ConcurrentElvesAndToyMachines();

            Assert.Equal(presents.Count, sleigh.PackedPresents.Count);

            List<string> packedIdsSorted = sleigh.PackedPresents
                .Select(p => p.Id)
                .OrderBy(id => id)
                .ToList();

            List<string> expectedIdsSorted = presents
                .Select(p => p.Id)
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(expectedIdsSorted, packedIdsSorted);

            int deliveredCount = ((SpyElf)elfOne).DeliveredPresents.Count 
                + ((SpyElf)elfTwo).DeliveredPresents.Count + ((SpyElf)elfThree).DeliveredPresents.Count;

            Assert.Equal(sleigh.PackedPresents.Count, deliveredCount);
        }

        [Fact]
        public async Task CheckFamilyPresentsOrderPreserved()
        {
            Console.WriteLine("---------- Story 3 - TEST 1: present order preserverd per family ----------");

            // One toy machine so order should be known
            List<Present> presents = new List<Present>
            {
                new Present("A1", "FamilyA", "Toy A1"),
                new Present("B1", "FamilyB", "Toy B1"),
                new Present("A2", "FamilyA", "Toy A2"),
                new Present("C1", "FamilyC", "Toy C1"),
                new Present("B2", "FamilyB", "Toy B2"),
                new Present("A3", "FamilyA", "Toy A3"),
                new Present("C2", "FamilyC", "Toy C2"),
                new Present("B3", "FamilyB", "Toy B3")
            };

            IToyMachine toyMachine = new MockToyMachine(presents);

            SpyElf elfOne = new SpyElf();
            SpyElf elfTwo = new SpyElf();

            SpySleigh sleigh = new SpySleigh();
            List<IElf> elves =  new List<IElf> { elfOne, elfTwo };

            MrsClausWorkFlow workflow = new MrsClausWorkFlow( new List<IToyMachine> { toyMachine }, elves, sleigh);

            await workflow.ConcurrentElvesAndToyMachines();

            Assert.Equal(presents.Count, sleigh.PackedPresents.Count);

            List<string> expectedIdsSorted = presents
                .Select(p => p.Id)
                .OrderBy(id => id)
                .ToList();

            List<string> packedIdsSorted = sleigh.PackedPresents
                .Select(p => p.Id)
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(expectedIdsSorted, packedIdsSorted);
            
            Dictionary<string, List<string>> inputOrderByFamily = presents
                .GroupBy(p => p.FamilyId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => p.Id).ToList());

            Dictionary<string, List<string>> outputOrderByFamily = sleigh.PackedPresents
                .GroupBy(p => p.FamilyId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => p.Id).ToList());

            foreach (KeyValuePair<string, List<string>> family in inputOrderByFamily)
            {
                string familyId = family.Key;
                List<string> expectedFamilyOrder = family.Value;

                Assert.True(outputOrderByFamily.ContainsKey(familyId));

                List<string> actualFamilyOrder = outputOrderByFamily[familyId];

                Assert.Equal(expectedFamilyOrder, actualFamilyOrder);
            }
        }
    }
}
