using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Contracts;
using Entities.DataTransferObjects;
using AutoMapper;
using Entities.Models;

namespace ASP.NetCore3_Web_APIs.Controllers
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
            var companies = _repository.Company.GetAllCompanies(trackChanges: false);

            //var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            var companiesDto = companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                FullAddress = string.Join(' ', c.Address, c.Country)
            }).ToList();

            //throw new Exception("Exception");

            return Ok(companiesDto);
        }

        [HttpGet("{id}", Name = "CompanyById")]
        public IActionResult GetCompany(Guid id)
        {
            var company = _repository.Company.GetCompany(id, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {id} doesn't exist in the database.");

                return NotFound();
            }
            else
            {
                var companiesDto = new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    FullAddress = string.Join(' ', company.Address, company.Country)
                };

                return Ok(companiesDto);
            }
        }

        [HttpPost]
        public IActionResult CreateCompany([FromBody]CompanyForCreationDto company)
        {
            if (company == null)
            {
                _logger.LogError("CompanyForCreationDto object sent from client is null.");
                return BadRequest("CompanyForCreationDto object is null");
            }


            //Get child Employees resource if they exist
            List<Employee> employees = null;
            if(company.Employees != null)
            {
                employees = _repository.Company.GetEmployees(company.Employees);
            }

            //var companyEntity = _mapper.Map<Company>(company);
            var companyEntity = new Company
            {
                Name = company.Name,
                Address = company.Address,
                Country = company.Country,
                Employees = employees
                
            };


            _repository.Company.CreateCompany(companyEntity);
            _repository.Save();

            //var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);
            var companyToReturn = new CompanyDto
            {
                Id = companyEntity.Id,
                Name = companyEntity.Name,
                FullAddress = string.Join(' ', companyEntity.Address, companyEntity.Country)
            };

            return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
        }
    }
}