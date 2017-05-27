// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace HolographicPhotoProject.Utils
{
    /// <summary>
    /// Converts from a boolean value to a Visibility enum and vice versa.
    /// This is used to control the visibility of UI elements using booleans.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public static class Constants
        {
            public const Visibility True = Visibility.Visible;
            public const Visibility False = Visibility.Collapsed;
        }

        public BooleanToVisibilityConverter() { }

        /// <summary>
        /// Converts from a boolean value to a Visibility enum.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool))
            {
                return null;
            }
            var flag = (bool)value;

            var reverse = parameter as string;
            if (reverse != null && reverse == "Reverse")
            {
                flag = !flag;
            }

            return flag ? Constants.True : Constants.False;
        }

        /// <summary>
        /// Converts from a Visibility enum to a boolean value.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool))
            {
                return null;
            }
            var flag = (bool)value;

            var reverse = parameter as string;
            if (reverse != null && reverse == "Reverse")
            {
                flag = !flag;
            }

            if (Equals(flag, Constants.True))
            {
                return true;
            }
            if (Equals(flag, Constants.False))
            {
                return false;
            }
            return null;
        }
    }
}
