using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var company = _repository.Company.GetCompany(companyId);
            if (company == null)
            {
                _logger.LogInfo($"Company with the specified id: {companyId} can not be found.");
                return NotFound();
            }

            var employees = _repository.Employee.GetEmployees(companyId);
            
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDTO>>(employees);

            return Ok(employeesDto);
        }

        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = _repository.Company.GetCompany(companyId);
            if (company == null)
            {
                _logger.LogInfo($"Company with the specified id: {companyId} can not be found.");
                return NotFound();
            }

            var employee = _repository.Employee.GetEmployee(companyId, id);
            if (employee == null)
            {
                _logger.LogInfo($"Employee with the specified id: {id} can not be found.");
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDTO>(employee);
            
            return Ok(employeeDto);
        }

        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeCreateDTO employeeCreateDTO)
        {
            if (employeeCreateDTO == null)
            {
                _logger.LogError($"The object employeeCreateDTO is not valid");
                return BadRequest($"Employee for company id {companyId} can not be creted because of invalid employee data.");
            }

            //check if company actually exists
            var company = _repository.Company.GetCompany(companyId);
            if (company == null)
            {
                _logger.LogInfo($"Company with the id: {companyId} can not be found.");
                return NotFound();
            }

            var employee = _mapper.Map<Employee>(employeeCreateDTO);
            
            _repository.Employee.CreateEmployeeForCompany(companyId, employee);
            _repository.Save();

            var employeeToReturnDto = _mapper.Map<EmployeeDTO>(employee);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturnDto.EmployeeID }, employeeToReturnDto);
        }
    }
}
