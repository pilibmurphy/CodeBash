using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingBash.Models
{
    public class Present
    {     
        public string Id { get; }

        public string FamilyId { get; }

        public string Description { get; }


        public Present(string id, string familyId, string description)
        {
            Id = id;
            FamilyId = familyId;
            Description = description;
        }
    }
}

