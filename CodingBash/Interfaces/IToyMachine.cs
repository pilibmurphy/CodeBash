using CodingBash.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingBash.Interfaces
{
    public interface IToyMachine
    {
        Task<Present> GetNextPresent();
    }
}
