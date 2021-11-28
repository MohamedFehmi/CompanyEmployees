using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface ICompanyRepository : IRepositoryBase<Company>
    {
        //new method to get all companies
        IEnumerable<Company> GetAllCompanies(bool trackChanges);
    }
}
