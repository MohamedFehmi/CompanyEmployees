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
        IEnumerable<Company> GetAllCompanies(bool trackChanges = false);
        Company GetCompany(Guid companyId, bool trackChanges = false);

        void CreateCompany(Company company);
    }
}
