using CodingBash.Interfaces;
using CodingBash.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CodingBash.Workflows
{
    public class ElfWorkflow
    {
        private readonly IToyMachine _toyMachine;
        private readonly IElf _elf;
        private readonly ISleigh _santasSleigh;

        public ElfWorkflow( IToyMachine toyMachine, IElf elf, ISleigh santasSleigh)
        {
            _toyMachine = toyMachine;
            _elf = elf;
            _santasSleigh = santasSleigh;
        }

        public async Task Work()
        {
            //TODO PM test return early should make the test fail
            // return;
            while (true)
            {
                Present nextPresent = await _toyMachine
                .GetNextPresent()
                .ConfigureAwait(false);

                if (nextPresent == null)
                {
                    Console.WriteLine("ELf Workflow - done");
                    break;
                }

                Console.WriteLine($"Workflow - procssed {nextPresent.Id}");

                await _elf
                    .Deliver(nextPresent, _santasSleigh)
                    .ConfigureAwait(false);
            }
        }
    }
}
