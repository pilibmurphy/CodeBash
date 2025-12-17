using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodingBash.Interfaces;
using CodingBash.Models;
using CodingBash.Workflows;
using tests.TestDoubles;
using Xunit;

namespace CodingBash.Tests
{
    public class StoryFour
    {
        [Fact]
        public async Task CancelFamily()
        {
            Console.WriteLine("---------- Story 4 - TEST 1: Cancel Family ----------");

            List<Present> presents = new List<Present>
            {
                new Present("p1", "family1", "Toy 1"),
                new Present("p2", "family2", "Toy 2"),
                new Present("p3", "family1", "Toy 3"),
                new Present("p4", "family3", "Toy 4")
            };

            IToyMachine toyMachine = new MockToyMachine(presents);

            SpyElf elfOne = new SpyElf();
            SpyElf elfTwo = new SpyElf();
            List<IElf> elves = new List<IElf> { elfOne, elfTwo };

            SpySleigh sleigh = new SpySleigh();

            MrsClausWorkFlow workflow = new MrsClausWorkFlow( new List<IToyMachine> { toyMachine }, elves, sleigh);

            // santa hates family one
            workflow.CancelFamily("family1");

            await workflow.ConcurrentElvesAndToyMachines();

            // so we should not end up with any for family1
            Assert.DoesNotContain(
                sleigh.PackedPresents,
                present => present.FamilyId == "family1");

            //can break this off and get more tests? cba probably
            List<string> packedIds = sleigh.PackedPresents
                .Select(present => present.Id)
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(new[] { "p2", "p4" }, packedIds);
        }

        [Fact]
        public async Task CancellingUnknownFamily_DoesNotBreakWorkflow()
        {
            Console.WriteLine("---------- Story 4 - TEST 3: Cancel unknown family ----------");

            List<Present> presents = new List<Present>
            {
                new Present("p1", "family1", "Toy 1"),
                new Present("p2", "family2", "Toy 2")
            };

            IToyMachine toyMachine = new MockToyMachine(presents);

            SpyElf elf = new SpyElf();
            SpySleigh sleigh = new SpySleigh();

            MrsClausWorkFlow workflow = new MrsClausWorkFlow( new List<IToyMachine> { toyMachine }, new List<IElf> { elf }, sleigh);

            // Cancel a family that never appears
            workflow.CancelFamily("fakeFam");

            await workflow.ConcurrentElvesAndToyMachines();

            // all og presents should still be packed
            Assert.Equal(2, sleigh.PackedPresents.Count);

            List<string> packedIds = sleigh.PackedPresents
                .Select(present => present.Id)
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(new[] { "p1", "p2" }, packedIds);
        }

        [Fact]
        public void DequeueForFakeFamilyReturnsFalse()
        {
            Console.WriteLine("---------- Story 4 - TEST 3: unknown family returns false ----------");
            MrsClausWorkFlow workflow = new MrsClausWorkFlow( new List<IToyMachine>(), new List<IElf>(), new SpySleigh());

            Present present;
            bool result = workflow.TryDequeueForFamily("missing-family", out present);

            Assert.False(result);
            Assert.Null(present);
        }

        [Fact]
        public void DequeueAnyWithNoFamilyQue()
        {
            Console.WriteLine("---------- Story 4 - TEST 3: no family queues returns false ----------");
            MrsClausWorkFlow workflow = new MrsClausWorkFlow( new List<IToyMachine>(), new List<IElf>(), new SpySleigh());

            Present present;
            string familyId;

            bool result = workflow.TryDequeueAny(out present, out familyId);

            Assert.False(result);
            Assert.Null(present);
        }
    }
}
