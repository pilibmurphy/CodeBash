using CodingBash.Interfaces;
using CodingBash.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CodingBash.Workflows
{
    public class MrsClausWorkFlow
    {
        private List<IToyMachine> _toyMachines;
        private List<IElf> _elves;
        private  ISleigh _sleigh;
        private ConcurrentDictionary<string, ConcurrentQueue<Present>> familyQueues;
        private ConcurrentDictionary<string, byte> cancelledFamilies;
        private SemaphoreSlim presentAvailable;
        private int activeToyMachines;

        public MrsClausWorkFlow( List<IToyMachine> toyMachines, List<IElf> elevs, ISleigh sleigh)
        {
            _toyMachines = toyMachines;
            _elves = elevs;
            _sleigh = sleigh;

            familyQueues = new ConcurrentDictionary<string, ConcurrentQueue<Present>>();
            cancelledFamilies = new ConcurrentDictionary<string, byte>();
            presentAvailable = new SemaphoreSlim(0);

            activeToyMachines = _toyMachines.Count;
        }

        public async Task ConcurrentElvesAndToyMachines()
        {
            List<Task> tasks = new List<Task>();

            foreach (IToyMachine toyMachine in _toyMachines)
            {
                Task ToyMachineTask = RunToyMachines(toyMachine);
                tasks.Add(ToyMachineTask);
            }

            foreach (IElf elf in _elves)
            {
                Task elfTask = RunElfs(elf);
                tasks.Add(elfTask);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }


        public void CancelFamily(string familyId)
        {
            cancelledFamilies.TryAdd(familyId, 0);

            ConcurrentQueue<Present> removedQueue;
            if (familyQueues.TryRemove(familyId, out removedQueue))
            {
                int discardedCount = removedQueue.Count;
                Console.WriteLine( "MrsClausWorkFlow - cancelled family {0}, discarded {1} queued presents.", familyId, discardedCount);
            }
            else
            {
                Console.WriteLine( "MrsClausWorkFlow - cancelled family {0}, nothing discarded.", familyId);
            }
        }

        private bool IsFamilyCancelled(string familyId)
        {
            if (string.IsNullOrEmpty(familyId)) return false;

            return cancelledFamilies.ContainsKey(familyId);
        }

        private async Task RunToyMachines(IToyMachine toyMachine)
        {
            try
            {
                while (true)
                {
                    Present present = await toyMachine
                        .GetNextPresent()
                        .ConfigureAwait(false);

                    if (present == null)
                    {
                        // finished
                        break;
                    }

                    EnqueuePresent(present);
                }
            }
            finally
            {
                int remaining = Interlocked.Decrement(ref activeToyMachines);

                if (remaining == 0)
                {
                    for (int index = 0; index < _elves.Count; index++)
                    {
                        presentAvailable.Release();
                    }
                }
            }
        }

        private void EnqueuePresent(Present present)
        {
            if (IsFamilyCancelled(present.FamilyId))
            {
                Console.WriteLine("MrsClausWorkFlow - discarding {0} for family {1}.", present.Id, present.FamilyId);
                return;
            }

            ConcurrentQueue<Present> familyQueue = familyQueues.GetOrAdd(
                present.FamilyId,
                _ => new ConcurrentQueue<Present>());

            familyQueue.Enqueue(present);

            // call it here to so we dont' get stuck again
            presentAvailable.Release();
        }

        private async Task RunElfs(IElf elf)
        {
            string preferredFamilyId = null;

            while (true)
            {
                await presentAvailable
                    .WaitAsync()
                    .ConfigureAwait(false);

                Present present;
                bool gotPresent = TryGetNextPresentForElf(ref preferredFamilyId, out present);

                if (!gotPresent)
                {
                    if (Volatile.Read(ref activeToyMachines) == 0)
                    {
                        break;
                    }

                    // This gets reviwed as not covered but I think another thread could grab it in that time, maybe write a test to spam it to get better coverage
                    continue;
                }

                await elf
                    .Deliver(present, _sleigh)
                    .ConfigureAwait(false);
            }
        }

        //making public for coverage lol
        public bool TryGetNextPresentForElf(ref string preferredFamilyId, out Present present)
        {
            if (!string.IsNullOrEmpty(preferredFamilyId))
            {
                if (TryDequeueForFamily(preferredFamilyId, out present))
                {
                    return true;
                }
            }

            //grab any to cover the "do not let Elves idle if avoidable"
            string selectedFamilyId;
            bool gotAny = TryDequeueAny(out present, out selectedFamilyId);

            if (gotAny)
            {
                preferredFamilyId = selectedFamilyId;
                return true;
            }

            preferredFamilyId = null;
            present = null;
            return false;
        }

        //making public for coverage lol
        public bool TryDequeueForFamily(string familyId, out Present present)
        {
            present = null;

            ConcurrentQueue<Present> queue;
            if (!familyQueues.TryGetValue(familyId, out queue))
            {
                return false;
            }

            return queue.TryDequeue(out present);
        }

        public bool TryDequeueAny(out Present present, out string familyId)
        {
            foreach (KeyValuePair<string, ConcurrentQueue<Present>> entry in familyQueues)
            {
                string currentFamilyId = entry.Key;

                if (entry.Value.TryDequeue(out present))
                {
                    familyId = currentFamilyId;
                    return true;
                }
            }

            present = null;
            familyId = null;
            return false;
        }
    }
}
