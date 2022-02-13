using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Marvin.Cache.Headers;
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
    //[ResponseCache(CacheProfileName = "120SecondsDuration")]
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

        [HttpGet(Name = "GetCompanies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _repository.Company.GetAllCompaniesAsync();

            var companiesDto = _mapper.Map<IEnumerable<CompanyDTO>>(companies);

            return Ok(companiesDto);
        }

        [HttpOptions]
        public IActionResult GetCompanyOptions() 
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");
            return Ok();
        }

        [HttpGet("{id}", Name = "CompanyById")]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)]
        [HttpCacheValidation(MustRevalidate = false)]
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

        [HttpPost(Name = "CreateCompany")]
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
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = HttpContext.Items["company"] as Company;

            _repository.Company.DeleteCompany(company);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyUpdateDTO companyUpdateDTO)
        {
            var company = HttpContext.Items["company"] as Company;

            _mapper.Map(companyUpdateDTO, company);
            await _repository.SaveAsync();

            return NoContent();
        }
    }
}
