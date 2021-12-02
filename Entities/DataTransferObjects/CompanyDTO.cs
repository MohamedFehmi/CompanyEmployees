using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DataTransferObjects
{
    public class CompanyDTO
    {
        public Guid CompanyID { get; set; }
        public string Name { get; set; }
        public string FullAddress { get; set; }
    }
}
