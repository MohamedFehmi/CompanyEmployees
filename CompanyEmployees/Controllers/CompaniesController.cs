using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _repository.Company.GetAllCompaniesAsync();

            var companiesDto = _mapper.Map<IEnumerable<CompanyDTO>>(companies);

            return Ok(companiesDto);
        }

        [HttpGet("{id}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompanyAsync(Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(id);

            if (company != null)
            {
                var companyDto = _mapper.Map<CompanyDTO>(company);
                return Ok(companyDto);
            }

            _logger.LogInfo($"Company with the specified id: {id} can not be found.");
            return NotFound();
        }

        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                _logger.LogError("Parameter ids is null.");
                return BadRequest("Parameter ids is null");
            }

            var companyEntities = await _repository.Company.GetByIdsAsync(ids);

            if (companyEntities.Count() != ids.Count())
            {
                _logger.LogError("Some ids are not valid in the collection");
                return NotFound();
            }

            var companiesDto = _mapper.Map<IEnumerable<CompanyDTO>>(companyEntities);

            return Ok(companiesDto);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyCreateDTO companyCreateDto)
        {
            var company = _mapper.Map<Company>(companyCreateDto);

            _repository.Company.Create(company);
           await _repository.SaveAsync();

            var companyDtoToReturn = _mapper.Map<CompanyDTO>(company);

            return CreatedAtRoute("CompanyById", new { id = companyDtoToReturn.CompanyID }, companyDtoToReturn);
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompaniesCollection([FromBody] IEnumerable<CompanyCreateDTO> companyCreateDTOs)
        {
            if (companyCreateDTOs == null)
            {
                _logger.LogError("Company collection sent from the client is null.");
                return BadRequest("Company collection is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the companyCreateDTOs");
                return UnprocessableEntity(ModelState);
            }

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCreateDTOs);

            foreach (var company in companyEntities)
            {
                _repository.Company.Create(company);
            }
            await _repository.SaveAsync();

            var companyDTOs = _mapper.Map<IEnumerable<CompanyDTO>>(companyEntities);

            var ids = string.Join(",", companyDTOs.Select(c => c.CompanyID));

            return CreatedAtRoute("CompanyCollection", new { ids }, companyDTOs);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(id);
            if (company == null)
            {
                _logger.LogInfo($"A company with the given id: {id} cannot be found.");
                return NotFound();
            }

            _repository.Company.DeleteCompany(company);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyUpdateDTO companyUpdateDTO)
        {
            var company = await _repository.Company.GetCompanyAsync(id, trackChanges: true);
            if (company == null)
            {
                _logger.LogError($"A company with the given id: {id} cannot be found.");
                return NotFound();
            }

            _mapper.Map(companyUpdateDTO, company);
            await _repository.SaveAsync();

            return NoContent();
        }
    }
}
