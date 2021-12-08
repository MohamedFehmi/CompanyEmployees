using AutoMapper;
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
        public IActionResult GetCompanies()
        {
            var companies = _repository.Company.GetAllCompanies();

            var companiesDto = _mapper.Map<IEnumerable<CompanyDTO>>(companies);

            return Ok(companiesDto);
        }

        [HttpGet("{id}", Name ="CompanyById")]
        public IActionResult GetCompany(Guid id)
        {
            var company = _repository.Company.GetCompany(id);
            
            if(company != null)
            {
                var companyDto = _mapper.Map<CompanyDTO>(company);
                return Ok(companyDto);
            }

            _logger.LogInfo($"Company with the specified id: {id} can not be found.");
            return NotFound();
        }

        [HttpPost]
        public IActionResult CreateCompany([FromBody] CompanyCreateDTO companyCreateDto)
        {
            if (companyCreateDto == null)
            {
                _logger.LogError($"The object companyCreateDto is not valid");
                return BadRequest($"Company can not be creted because of invalid data.");
            }

            var company = _mapper.Map<Company>(companyCreateDto);

            _repository.Company.Create(company);
            _repository.Save();

            var companyDtoToReturn = _mapper.Map<CompanyDTO>(company);

            return CreatedAtRoute("CompanyById", new { id = companyDtoToReturn.CompanyID }, companyDtoToReturn);
        }
    }
}
