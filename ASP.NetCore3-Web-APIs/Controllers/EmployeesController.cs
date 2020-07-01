using ASP.NetCore3_Web_APIs.ActionFilters;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
                return BadRequest("Max age can't be less than min age.");

            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            
            var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeesFromDb.MetaData));
            
            //var employeeDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
            var employeeDto = employeesFromDb.Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                Age = e.Age,
                Position = e.Position
            }).ToList();

            return Ok(employeeDto); 
        }


        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                
                return NotFound();
            }
            
            var employeeFromDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges: false);
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
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody]EmployeeForCreationDto employee)
        {
            //------------- The commented code below is replaced by the ActionFilter called as an attribute above ----------

            //Check if request contains NULL values
            //if (employee == null)
            //{
            //    _logger.LogError("EmployeeForCreationDto object sent from client is null.");
            //    return BadRequest("EmployeeForCreationDto object is null");
            //}

            ////Suppress the BadRequest error when the ModelState is invalid
            //if (!ModelState.IsValid)
            //{
            //    _logger.LogError("Invalid model state for the EmployeeForCreationDto object");
            //    //ModelState.AddModelError(string key, string errorMessage);
                
            //    return UnprocessableEntity(ModelState);
            //}

            

            //Check to see that company with such Guid exists
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
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
            await _repository.SaveAsync();

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
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var employeeForCompany = HttpContext.Items["employee"] as Employee;
            //------------- The commented code below is replaced by the ActionFilter called as an attribute above ----------

            //var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            //if (company == null)
            //{
            //    _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

            //    return NotFound();
            //}

            //var employeeForCompany = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges: false);
            //if (employeeForCompany == null)
            //{
            //    _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");

            //    return NotFound();
            //}

            _repository.Employee.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
        {
            //------------- The commented code below is replaced by the ActionFilter called as an attribute above ----------

            //if (employee == null)
            //{
            //    _logger.LogError("EmployeeForUpdateDto object sent from client is null.");
            //    return BadRequest("EmployeeForUpdateDto object is null");
            //}

            //if (!ModelState.IsValid)
            //{
            //    _logger.LogError("Invalid model state for the EmployeeForUpdateDto object");
            //    return UnprocessableEntity(ModelState);
            //}

            var employeeEntity = HttpContext.Items["employee"] as Employee;
            //------------- The commented code below is replaced by the ActionFilter called as an attribute above ----------

            //var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            //if (company == null)
            //{
            //    _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

            //    return NotFound();
            //}

            //var employeeEntity = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges: true);
            //if (employeeEntity == null)
            //{
            //    _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");

            //    return NotFound();
            //}

            //_mapper.Map(employee, employeeEntity);
            employeeEntity.Name = employee.Name;
            employeeEntity.Age = employee.Age;
            employeeEntity.Position = employee.Position;
            await _repository.SaveAsync();
            
            return NoContent();
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges: true);
            if (employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            }

            //var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            var employeeToPatch = new EmployeeForUpdateDto
            {
                Name = employeeEntity.Name,
                Age = employeeEntity.Age,
                Position = employeeEntity.Position
            };

            patchDoc.ApplyTo(employeeToPatch, ModelState);  //validating patchDoc (checks to see that paths are correct/exists)
            TryValidateModel(employeeToPatch);  //validates the values to make sure model is valid
            
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            //saving employeeEntity to db
            //_mapper.Map(employeeToPatch, employeeEntity);
            employeeEntity.Name = employeeToPatch.Name;
            employeeEntity.Age = employeeToPatch.Age;
            employeeEntity.Position = employeeToPatch.Position;
            await _repository.SaveAsync();
            
            return NoContent();

        }
    }
}