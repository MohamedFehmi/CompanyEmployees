using Contracts;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public void CreateCompany(Company company) => Create(company);

        public IEnumerable<Company> GetAllCompanies(bool trackChanges) =>
            FindAll(trackChanges)
            .OrderBy(c => c.Name)
            .ToList();

        public IEnumerable<Company> GetByIds(IEnumerable<Guid> ids, bool trackChanges = false) =>
            FindByCondition(c => ids.Contains(c.CompanyID), trackChanges)
            .ToList();

        public Company GetCompany(Guid companyId, bool trackChanges) =>
            FindByCondition(c => c.CompanyID.Equals(companyId))
            .FirstOrDefault();
    }
}
