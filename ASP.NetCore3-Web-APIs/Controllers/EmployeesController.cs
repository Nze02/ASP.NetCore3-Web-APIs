using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NetCore3_Web_APIs.Controllers
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
            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            
            var employeesFromDb = _repository.Employee.GetEmployees(companyId, trackChanges: false);
            //var employeeDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
            var employeeDto = employeesFromDb.Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                Age = e.Age,
                Position = e.Position
            }).ToList();

            return Ok(employeesFromDb); 
        }


        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                
                return NotFound();
            }
            
            var employeeFromDb = _repository.Employee.GetEmployee(companyId, id, trackChanges: false);
            if (employeeFromDb == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                
                return NotFound();
            }

            //var employee = _mapper.Map<EmployeeDto>(employeeDb);
            var employee = new EmployeeDto
            {
                Id = employeeFromDb.Id,
                Name = employeeFromDb.Name,
                Age = employeeFromDb.Age,
                Position = employeeFromDb.Position
            };


            return Ok(employee);
        }

        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody]EmployeeForCreationDto employee)
        {
            //Check if request contains NULL values
            if(employee == null)
            {
                _logger.LogError("EmployeeForCreationDto object sent from client is null.");
                return BadRequest("EmployeeForCreationDto object is null");
            }

            //Check to see that company with such Guid exists
            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            //var employeeEntity = _mapper.Map<Employee>(employee);
            var employeeEntity = new Employee
            {
                Name = employee.Name,
                Age = employee.Age,
                Position = employee.Position
            };

            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            _repository.Save();

            var employeeToReturn = new EmployeeDto
            {
                Id = employeeEntity.Id,
                Name = employeeEntity.Name,
                Age = employeeEntity.Age,
                Position = employeeEntity.Position,
        };

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEmployee(Guid companyId, Guid id)
        {
            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

                return NotFound();
            }

            var employeeForCompany = _repository.Employee.GetEmployee(companyId, id, trackChanges: false);
            if (employeeForCompany == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");

                return NotFound();
            }

            _repository.Employee.DeleteEmployee(employeeForCompany);
            _repository.Save();

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
        {
            if (employee == null)
            {
                _logger.LogError("EmployeeForUpdateDto object sent from client is null.");
                return BadRequest("EmployeeForUpdateDto object is null");
            }

            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

                return NotFound();
            }

            var employeeEntity = _repository.Employee.GetEmployee(companyId, id, trackChanges: true);
            if (employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");

                return NotFound();
            }

            //_mapper.Map(employee, employeeEntity);
            employeeEntity.Name = employee.Name;
            employeeEntity.Age = employee.Age;
            employeeEntity.Position = employee.Position;
            _repository.Save();
            
            return NoContent();
        }
    }
}