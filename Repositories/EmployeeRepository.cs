using Contracts;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public Employee GetEmployee(Guid companyId, Guid employeeId, bool trackChanges = false) =>
            FindByCondition(e => e.CompanyID.Equals(companyId) && e.EmployeeID.Equals(employeeId))
            .FirstOrDefault();

        public IEnumerable<Employee> GetEmployees(Guid companyId, bool trackChanges) =>
            FindByCondition(e => e.CompanyID.Equals(companyId), trackChanges)
            .OrderBy(e => e.Name);
    }
}
