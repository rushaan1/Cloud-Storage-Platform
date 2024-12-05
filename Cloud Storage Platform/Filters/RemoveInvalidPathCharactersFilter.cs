using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Cloud_Storage_Platform.Filters
{
    public class RemoveInvalidPathCharactersFilter : IAsyncResourceFilter
    {
        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            string[] queriesToFilter = {"newFolderPath", "path", "searchString" };
            IQueryCollection query = context.HttpContext.Request.Query;
            Dictionary<string, StringValues> queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(query.ToString());

            foreach (string q in queriesToFilter) 
            {
                if (query.ContainsKey(q)) 
                {
                    queryDictionary[q] = FilteredPath(q);
                }
            }
        }

        private string FilteredPath(string originalString) 
        {
            string filteredString = originalString;
            foreach (char c in Path.GetInvalidFileNameChars()) 
            {
                filteredString = filteredString.Replace(c.ToString(), "");   
            }
            return filteredString;
        }
    }
}
