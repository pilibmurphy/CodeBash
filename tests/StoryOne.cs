using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodingBash.Models;
using CodingBash.Interfaces;
using CodingBash.Workflows;
using Xunit;
using System.Diagnostics;
using tests.TestDoubles;

namespace CodingBash.Tests
{
    public class StoryOne
    {
        //TODO PM rename tests with the "What When Then" formatting 
        [Fact]
        public async Task PresentDeliveredToSleigh()
        {
            Console.WriteLine("-------------------- Story 1 - TEST 1 --------------------------------");

            Present present = new Present("p1", "family1", "Toy");

            MockToyMachine toyMachine = new MockToyMachine(new List<Present> { present });
            SpyElf spyElf = new SpyElf();
            SpySleigh spySleigh = new SpySleigh();

            ElfWorkflow workflow = new ElfWorkflow(toyMachine, spyElf, spySleigh);

            await workflow.Work();

            Assert.Single(spyElf.DeliveredPresents);
            Assert.Equal("p1", spyElf.DeliveredPresents[0].Id);

            Assert.Single(spySleigh.PackedPresents);
            Assert.Equal("p1", spySleigh.PackedPresents[0].Id);
        }

        [Fact]
        public async Task MultiplePresentsDeliveredInOrder()
        {
            List<Present> presents = new List<Present>
            {
                new Present("p1", "family1", "Toy1"),
                new Present("p2", "family2", "Toy2"),
                new Present("p3", "family3", "Toy3")
            };

            MockToyMachine mockToyMachine = new MockToyMachine(presents);
            SpyElf spyElf = new SpyElf();
            SpySleigh spySleigh = new SpySleigh();

            ElfWorkflow workflow = new ElfWorkflow(mockToyMachine, spyElf, spySleigh);

            await workflow.Work();

            Assert.Equal(3, spyElf.DeliveredPresents.Count);
            Assert.Equal("p1", spyElf.DeliveredPresents[0].Id);
            Assert.Equal("p2", spyElf.DeliveredPresents[1].Id);
            Assert.Equal("p3", spyElf.DeliveredPresents[2].Id);

            Assert.Equal(3, spySleigh.PackedPresents.Count);
            Assert.Equal("p1", spySleigh.PackedPresents[0].Id);
            Assert.Equal("p2", spySleigh.PackedPresents[1].Id);
            Assert.Equal("p3", spySleigh.PackedPresents[2].Id);
        }

        [Fact]
        public async Task WorkflowEndsWhenToyMachineReturnsNull()
        {
            List<Present> presents = new List<Present>
            {
                new Present("p1", "family1", "Toy1")
            };

            MockToyMachine mockToyMachine = new MockToyMachine(presents);
            SpyElf spyElf = new SpyElf();
            SpySleigh spySleigh = new SpySleigh();

            ElfWorkflow workflow = new ElfWorkflow(mockToyMachine, spyElf, spySleigh);

            await workflow.Work();

            Assert.Single(spyElf.DeliveredPresents);
            Assert.Single(spySleigh.PackedPresents);
        }
    }
}
