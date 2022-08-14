﻿using Eos.Nwn;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Eos.Extensions
{
    internal class ResRefConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is NWNResourceType type)
                return MasterRepository.Resources.Get((string?)value, type) ?? DependencyProperty.UnsetValue;
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
