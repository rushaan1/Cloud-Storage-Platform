using Microsoft.AspNetCore.Mvc.Filters;

namespace Cloud_Storage_Platform.Filters
{
    public class SkipUserIdentification : Attribute, IFilterMetadata
    {
    }
}
