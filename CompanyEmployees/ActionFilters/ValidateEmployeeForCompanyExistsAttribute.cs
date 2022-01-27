using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateEmployeeForCompanyExistsAttribute : IAsyncActionFilter
    {
        IRepositoryManager _repository;
        ILoggerManager _logger;

        public ValidateEmployeeForCompanyExistsAttribute(IRepositoryManager repository, ILoggerManager logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Extracts request params and HTTP method
            var method = context.HttpContext.Request.Method;
            var trackChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;
            
            var companyId = (Guid)context.ActionArguments["companyId"];
            var company = await _repository.Company.GetCompanyAsync(companyId);
            if (company == null)
            {
                _logger.LogInfo($"A company with the given id: {companyId} cannot be found.");
                context.Result = new NotFoundResult();
            }

            var employeeId = (Guid)context.ActionArguments["id"];
            var employee = await _repository.Employee.GetEmployeeAsync(companyId, employeeId, trackChanges);
            if (employee == null)
            {
                _logger.LogInfo($"An employee with the given id: {employeeId} cannot be found.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }
        }
    }
}
