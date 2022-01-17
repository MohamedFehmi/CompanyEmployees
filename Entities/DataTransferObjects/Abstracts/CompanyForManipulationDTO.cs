using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DataTransferObjects
{
    public abstract class CompanyForManipulationDTO
    {
        [Required(ErrorMessage = "Company name is a required field.")]
        [MaxLength(30, ErrorMessage = "Maximum length for the name is 30 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Company address is a required field.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Country is a required field.")]
        public string Country { get; set; }

        public IEnumerable<EmployeeCreateDTO> Employees { get; set; }
    }
}
