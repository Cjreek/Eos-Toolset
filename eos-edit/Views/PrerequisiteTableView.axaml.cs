using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using Eos.Models;
using Eos.Types;
using Eos.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Eos.Views
{
    class RequirementTypeToVisibleConverter : IValueConverter
    {
        public int ParamNumber { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if ((value is RequirementType requType) && (parameter is String controlType))
            {
                if (ParamNumber == 1)
                {
                    switch (requType)
                    {
                        case RequirementType.ARCSPELL:
                        case RequirementType.SPELL:
                        case RequirementType.BAB:
                            return controlType == "INT";
                        case RequirementType.FEATOR:
                        case RequirementType.FEAT:
                            return controlType == "FEAT";
                        case RequirementType.VAR:
                            return controlType == "VAR";
                        case RequirementType.CLASSNOT:
                        case RequirementType.CLASSOR:
                            return controlType == "CLASS";
                        case RequirementType.RACE:
                            return controlType == "RACE";
                        case RequirementType.SAVE:
                            return controlType == "SAVE";
                        case RequirementType.SKILL:
                            return controlType == "SKILL";
                    }
                }
                else if (ParamNumber == 2)
                {
                    switch (requType)
                    {
                        case RequirementType.SKILL:
                        case RequirementType.SAVE:
                        case RequirementType.VAR:
                            return controlType == "INT";
                    }
                }
            }

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SaveType
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }

    public partial class PrerequisiteTableView : LanguageAwarePage
    {
        public SaveType? SelectedSaveType { get; set; }

        public List<SaveType> SaveTypes { get; set; } = new List<SaveType>();

        public PrerequisiteTableView()
        {
            SaveTypes.Add(new SaveType()
            {
                Name = "Fortitude",
                Value = 1,
            });
            SaveTypes.Add(new SaveType()
            {
                Name = "Reflex",
                Value = 2,
            });
            SaveTypes.Add(new SaveType()
            {
                Name = "Willpower",
                Value = 3,
            });

            InitializeComponent();
        }
    }
}
