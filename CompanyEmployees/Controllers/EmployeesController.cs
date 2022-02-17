using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Utility;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private readonly EmployeeLinks _employeeLinks;

        public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, EmployeeLinks employeeLinks)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _employeeLinks = employeeLinks;
        }

        [HttpGet, Authorize]
        [HttpHead]
        [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.IsValidAgeRange)
                return BadRequest($"Max age can not be less than min age.");

            var company = await _repository.Company.GetCompanyAsync(companyId);
            if (company == null)
            {
                _logger.LogInfo($"Company with the specified id: {companyId} can not be found.");
                return NotFound();
            }

            var employees = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters);

            Response.Headers.Add("X-pagination", JsonSerializer.Serialize(employees.MetaData));

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDTO>>(employees);
            var links = _employeeLinks.TryGenerateLinks(employeesDto, employeeParameters.Fields, companyId, HttpContext);

            return links.HasLinks ? Ok(links.LinkedEntities) : Ok(links.ShapedEntities);
        }

        [HttpGet("{id}", Name = "GetEmployeeForCompany"), Authorize]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId);
            if (company == null)
            {
                _logger.LogInfo($"Company with the specified id: {companyId} can not be found.");
                return NotFound();
            }

            var employee = await _repository.Employee.GetEmployeeAsync(companyId, id);
            if (employee == null)
            {
                _logger.LogInfo($"Employee with the specified id: {id} can not be found.");
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDTO>(employee);
            
            return Ok(employeeDto);
        }

        [HttpPost, Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<ActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeCreateDTO employeeCreateDTO)
        {
            //check if company actually exists
            var company = await _repository.Company.GetCompanyAsync(companyId);
            if (company == null)
            {
                _logger.LogInfo($"Company with the id: {companyId} can not be found.");
                return NotFound();
            }

            var employee = _mapper.Map<Employee>(employeeCreateDTO);
            
            _repository.Employee.CreateEmployeeForCompany(companyId, employee);
            await _repository.SaveAsync();

            var employeeToReturnDto = _mapper.Map<EmployeeDTO>(employee);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturnDto.EmployeeID }, employeeToReturnDto);
        }

        [HttpDelete("{id}"), Authorize]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var employee = HttpContext.Items["employee"] as Employee;

            _repository.Employee.DeleteEmployee(employee);
            await _repository.SaveAsync();
            
            return NoContent();
        }

        [HttpPut("{id}"), Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeUpdateDTO employeeUpdateDTO)
        {
            var employee = HttpContext.Items["employee"] as Employee;

            _mapper.Map(employeeUpdateDTO, employee);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{id}"), Authorize]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeUpdateDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                _logger.LogError("The data object 'patchDocument' sent from the client is null.");
                return BadRequest("patchDocument object is null");
            }

            var employee = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeUpdateDTO>(employee);

            patchDocument.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patchDocument");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employee);
            await _repository.SaveAsync();

            return NoContent();
        }
    }
}
