using CodingBash.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingBash.Interfaces
{
    public interface IElf
    {
        Task Deliver(Present present, ISleigh santasSleigh);
    }
}
