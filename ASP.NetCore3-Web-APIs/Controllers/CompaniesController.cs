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
using ASP.NetCore3_Web_APIs.ModelBinders;

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


        //getting all companies
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


        //getting company by id
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


        //creating new company
        [HttpPost]
        public IActionResult CreateCompany([FromBody]CompanyForCreationDto company)
        {
            if (company == null)
            {
                _logger.LogError("CompanyForCreationDto object sent from client is null.");
                return BadRequest("CompanyForCreationDto object is null");
            }

            //Suppress the BadRequest error when the ModelState is invalid
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the CompanyForCreationDto object");
                return UnprocessableEntity(ModelState);
            }


            //Get child Employees resource if they exist
            List<Employee> employees = null;
            if (company.Employees != null)
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


        //get companies with ID contained in list of ids
        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public IActionResult GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                _logger.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }

            var companyEntities = _repository.Company.GetByIds(ids, trackChanges: false);
            if (ids.Count() != companyEntities.Count())
            {
                _logger.LogError("Some ids are not valid in a collection");
                return NotFound();
            }

            //var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            List<Company> companyList = companyEntities.ToList();
            List<CompanyDto> companiesToReturn = new List<CompanyDto>();

            foreach (Company company in companyList)
            {
                companiesToReturn.Add(new CompanyDto {Id = company.Id, Name =  company.Name, FullAddress = string.Join(' ', company.Address, company.Country) });
            }

            return Ok(companiesToReturn);
        }


        //create more than one company at a time
        [HttpPost("collection")]
        public IActionResult CreateCompanyCollection([FromBody]IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if (companyCollection == null)
            {
                _logger.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            }
            

            //var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            List<CompanyForCreationDto> companyForCreationDtoList = companyCollection.ToList();
            List<Company> companyEntities = new List<Company>();
            foreach (CompanyForCreationDto companyForCreationDto in companyForCreationDtoList)
            {
                List<Employee> employees = null;
                if (companyForCreationDto.Employees != null)
                {
                    employees = _repository.Company.GetEmployees(companyForCreationDto.Employees);
                }

                companyEntities.Add(new Company
                {
                    Name = companyForCreationDto.Name,
                    Address = companyForCreationDto.Address,
                    Country = companyForCreationDto.Country,
                    Employees = employees
                });
            }

            foreach (Company company in companyEntities)
            {
                _repository.Company.CreateCompany(company);
            }
            
            _repository.Save();


            //var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            List<Company> companyEntitiesList = companyEntities.ToList();
            List<CompanyDto> companyCollectionToReturn = new List<CompanyDto>();
            foreach(Company company in companyEntitiesList)
            {
                companyCollectionToReturn.Add(new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    FullAddress = string.Join(' ', company.Address, company.Country)
                });
            }

            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));
            
            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteCompany(Guid id)
        {
            var company = _repository.Company.GetCompany(id, trackChanges: false);
            if(company == null)
            {
                _logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
                return NotFound();
            }

            _repository.Company.DeleteCompany(company);
            _repository.Save();

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCompany(Guid id, [FromBody]CompanyForUpdateDto company)
        {
            if (company == null)
            {
                _logger.LogError("CompanyForUpdateDto object sent from client is null.");
                return BadRequest("CompanyForUpdateDto object is null");
            }


            var companyEntity = _repository.Company.GetCompany(id, trackChanges: true);
            if (companyEntity == null)
            {
                _logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
                return NotFound();
            }

            //extracting the added employee(s)
            List<EmployeeForCreationDto> employeeForCreationDtoList = company.Employees.ToList();
            List<Employee> employees = new List<Employee>();
            foreach(EmployeeForCreationDto employeeForCreationDto in employeeForCreationDtoList)
            {
                employees.Add(new Employee {
                    Name = employeeForCreationDto.Name,
                    Age = employeeForCreationDto.Age,
                    Position = employeeForCreationDto.Position
                });
            }

            //_mapper.Map(company, companyEntity);
            companyEntity.Name = company.Name;
            companyEntity.Address = company.Address;
            companyEntity.Country = company.Country;
            companyEntity.Employees = (ICollection<Employee>)employees;

            _repository.Save();

            return NoContent();
        }
    }
}