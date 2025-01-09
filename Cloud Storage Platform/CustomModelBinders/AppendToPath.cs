using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Cloud_Storage_Platform.CustomModelBinders
{
    public class AppendToPath : IModelBinder
    {
        private readonly IConfiguration _configuration;
        public AppendToPath(IConfiguration configuration) 
        {
            _configuration = configuration;
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string parameterName = bindingContext.FieldName;
            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(parameterName);
            if (valueProviderResult != ValueProviderResult.None) 
            {
                if (!valueProviderResult.FirstValue.IsNullOrEmpty()) 
                {
                    string decodedValue = Uri.UnescapeDataString(valueProviderResult.FirstValue!); 
                    string updatedValue = _configuration["InitialPathForStorage"] + decodedValue;
                    bindingContext.Result = ModelBindingResult.Success(updatedValue);

                    return Task.CompletedTask;
                }
                bindingContext.Result = ModelBindingResult.Success(valueProviderResult.FirstValue);
                return Task.CompletedTask;
            }
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }
    }
}
