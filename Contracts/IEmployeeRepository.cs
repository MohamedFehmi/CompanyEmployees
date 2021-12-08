using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IEmployeeRepository : IRepositoryBase<Employee>
    {
        IEnumerable<Employee> GetEmployees(Guid companyId, bool trackChanges = false);
        Employee GetEmployee(Guid companyId, Guid employeeId, bool trackChanges = false);

        void CreateEmployeeForCompany(Guid companyId, Employee employee);
    }
}
