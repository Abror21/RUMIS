using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Core
{
    /// <summary>
    /// Bind double and float numbers accepting both "," and "." as a decimal separator.
    /// </summary>
    public class NumberModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (bindingContext.ModelMetadata.IsNullableValueType && string.IsNullOrEmpty(value))
                return Task.CompletedTask;

            // Depending on CultureInfo, the NumberDecimalSeparator can be "," or "."
            // Both "." and "," should be accepted, but aren't.
            string wantedSeperator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            string alternateSeperator = (wantedSeperator == "," ? "." : ",");

            if (value.IndexOf(wantedSeperator) == -1
                && value.IndexOf(alternateSeperator) != -1)
            {
                value = value.Replace(alternateSeperator, wantedSeperator);
            }

            Type type = bindingContext.ModelType;
            object model = null;

            if (type == typeof(decimal) || type == typeof(decimal?))
                model = decimal.Parse(value, NumberStyles.Any);
            else if (type == typeof(double) || type == typeof(double?))
                model = double.Parse(value, NumberStyles.Any);

            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}
