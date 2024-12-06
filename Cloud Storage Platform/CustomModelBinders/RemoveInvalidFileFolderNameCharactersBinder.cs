using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Cloud_Storage_Platform.CustomModelBinders
{
    public class RemoveInvalidFileFolderNameCharactersBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string parameterName = bindingContext.FieldName;
            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(parameterName);
            if (valueProviderResult != ValueProviderResult.None) 
            {
                string? value = valueProviderResult.FirstValue;
                string? updatedValue = FilteredName(value);
                bindingContext.Result = ModelBindingResult.Success(updatedValue);
                return Task.CompletedTask;
            }
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        private string? FilteredName(string? originalString)
        {
            string? filteredString = originalString;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filteredString = filteredString?.Replace(c.ToString(), "");
            }
            return filteredString;
        }
    }
}
