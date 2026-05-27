using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LocalLedger.Helpers;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string type)
        {
            if (type == "Budget")
            {
                return boolValue
                    ? new SolidColorBrush(Color.Parse("#EF4444"))
                    : new SolidColorBrush(Color.Parse("#3B82F6"));
            }

            if (boolValue)
            {
                return type switch
                {
                    "Income" => new SolidColorBrush(Color.Parse("#10B981")),
                    "Expense" => new SolidColorBrush(Color.Parse("#EF4444")),
                    _ => Brushes.Gray
                };
            }
            return new SolidColorBrush(Color.Parse("#374151"));
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.TransactionType type)
        {
            return type switch
            {
                Models.TransactionType.Income => new SolidColorBrush(Color.Parse("#10B981")),
                Models.TransactionType.Expense => new SolidColorBrush(Color.Parse("#EF4444")),
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FirstCharConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
        {
            return str[0].ToString();
        }
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnumEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        return value.Equals(parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked)
        {
            return parameter;
        }
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
