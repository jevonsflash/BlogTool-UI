﻿//-----------------------------------------------------------------------
// <copyright file="NumberConverter.cs" company="Visual JSON Editor">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>http://visualjsoneditor.codeplex.com/license</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;

namespace BlogTool.Converter.JsonEditorConverter
{
    public class NumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? value.ToString() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(decimal))
            {
                try
                {
                    return decimal.Parse(value.ToString());
                }
                catch
                {
                    return default(decimal);
                }
            }
            else
            {
                try
                {
                    return double.Parse(value.ToString());
                }
                catch
                {
                    return default(double);
                }
            }
        }
    }
}
