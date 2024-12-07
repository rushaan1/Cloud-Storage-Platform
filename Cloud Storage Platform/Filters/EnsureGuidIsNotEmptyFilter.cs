using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Cloud_Storage_Platform.Filters
{
    public class EnsureGuidIsNotEmptyFilter : IAsyncResourceFilter
    {
        private readonly string[] _queryNames;
        public EnsureGuidIsNotEmptyFilter(string[] queryNames) 
        {
            _queryNames = queryNames;
        }
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (_queryNames == null || _queryNames.Length == 0) 
            {
                await next();
                return;
            }

            IEnumerable<KeyValuePair<string, StringValues>> guidQueryParameters = context.HttpContext.Request.Query.Where(q => _queryNames.Contains(q.Key));
            foreach (KeyValuePair<string, StringValues> queryParameter in guidQueryParameters) 
            {
                foreach (var value in queryParameter.Value) 
                {
                    if (!Guid.TryParse(value, out Guid parsedGuid) || parsedGuid == Guid.Empty)
                    {
                        context.Result = new BadRequestObjectResult("Invalid Guid: " + queryParameter.Key);
                        return;
                    }
                }
            }
            await next();
        }
    }
}
