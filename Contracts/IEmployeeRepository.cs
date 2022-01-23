﻿using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IEmployeeRepository : IRepositoryBase<Employee>
    {
        Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId, bool trackChanges = false);
        Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId, bool trackChanges = false);
        void CreateEmployeeForCompany(Guid companyId, Employee employee);
        void DeleteEmployee(Employee employee);
    }
}
