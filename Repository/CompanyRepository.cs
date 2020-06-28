using Contracts;
using Entities;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges) =>
            await FindAll(trackChanges)
            .OrderBy(c => c.Name)
            .ToListAsync();

        public async Task<Company> GetCompanyAsync(Guid companyId, bool trackChanges) =>
            await FindByCondition(c => c.Id.Equals(companyId), trackChanges)
            .SingleOrDefaultAsync();

        public void CreateCompany(Company company) => Create(company);

        //get child Employee resources added alongside Company
        public List<Employee> GetEmployees(IEnumerable<EmployeeForCreationDto> Employees)
        {
            List<Employee> employees = new List<Employee>();
            foreach (EmployeeForCreationDto comp in Employees)
            {
                employees.Add(new Employee {Name = comp.Name, Age = comp.Age, Position = comp.Position });
            }

            return employees;
        }

        public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges) =>
            await FindByCondition(x => ids.Contains(x.Id), trackChanges)
            .ToListAsync();

        public void DeleteCompany(Company company) =>
            Delete(company);
    }
}
