using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateCompanyExistsAttribute : IAsyncActionFilter
    {
        IRepositoryManager _repository; 
        ILoggerManager _logger;
        
        public ValidateCompanyExistsAttribute(IRepositoryManager repository, ILoggerManager logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Extract request params from the context
            var trackChanges = context.HttpContext.Request.Method.Equals("PUT");
            var companyId = (Guid)context.ActionArguments["id"];

            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company == null)
            {
                _logger.LogInfo($"A company with the given id: {companyId} cannot be found.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("company", company);
                await next();
            }
        }
    }
}
