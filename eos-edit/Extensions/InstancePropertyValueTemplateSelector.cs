using Eos.Models.Tables;
using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Markup.Xaml.Templates;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Eos.Nwn.Tlk;

namespace Eos.Extensions
{
    public class InstancePropertyValueTemplateSelector : IDataTemplate
    {
        public DataTemplate? ErrorTemplate { get; set; }
        public DataTemplate? SpaceTemplate { get; set; }
        public DataTemplate? IntTemplate { get; set; }
        public DataTemplate? DoubleTemplate { get; set; }
        public DataTemplate? BoolTemplate { get; set; }
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? TlkTemplate { get; set; }
        public DataTemplate? VariantTemplate { get; set; }
        public DataTemplate? RaceTemplate { get; set; }
        public DataTemplate? ClassTemplate { get; set; }
        public DataTemplate? SkillTemplate { get; set; }
        public DataTemplate? DomainTemplate { get; set; }
        public DataTemplate? SpellTemplate { get; set; }
        public DataTemplate? FeatTemplate { get; set; }
        public DataTemplate? AreaEffectTemplate { get; set; }
        public DataTemplate? AppearanceTemplate { get; set; }
        public DataTemplate? VisualEffectTemplate { get; set; }
        public DataTemplate? PortraitTemplate { get; set; }
        public DataTemplate? PolymorphTemplate { get; set; }
        public DataTemplate? MasterFeatTemplate { get; set; }
        public DataTemplate? SoundsetTemplate { get; set; }
        public DataTemplate? PackageTemplate { get; set; }
        public DataTemplate? DiseaseTemplate { get; set; }
        public DataTemplate? PoisonTemplate { get; set; }
        public DataTemplate? TrapTemplate { get; set; }
        public DataTemplate? BaseItemTemplate { get; set; }
        public DataTemplate? ItemPropertyTemplate { get; set; }
        public DataTemplate? AppearanceSoundsetTemplate { get; set; }
        public DataTemplate? WeaponSoundTemplate { get; set; }
        public DataTemplate? InventorySoundTemplate { get; set; }
        public DataTemplate? CompanionTemplate { get; set; }
        public DataTemplate? FamiliarTemplate { get; set; }
        public DataTemplate? ProgrammedEffectTemplate { get; set; }
        public DataTemplate? CustomObjectTemplate { get; set; }
        public DataTemplate? CustomEnumTemplate { get; set; }
        public DataTemplate? DamageTypeTemplate { get; set; }
        public DataTemplate? DamageTypeGroupTemplate { get; set; }

        public Control? Build(object? param)
        {
            var template = SelectTemplate(param);
            return template?.Build(param);
        }

        public bool Match(object? data)
        {
            return true;
        }

        public IDataTemplate? SelectTemplate(object? item)
        {
            DataTypeDefinition? dataTypeDef = null;
            if (item is CustomValueInstance valueInstance)
                dataTypeDef = valueInstance.Property.DataType;
            else if (item is DataTypeDefinition itemDataTypeDef)
                dataTypeDef = itemDataTypeDef;

            if (dataTypeDef != null)
            {
                if (dataTypeDef.ID == Guid.Parse("a136669b-e618-4be1-9a29-8b76f85c60be"))
                    return SpaceTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(int))
                    return IntTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(double))
                    return DoubleTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(bool))
                    return BoolTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(string))
                    return StringTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(TLKStringSet))
                    return TlkTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(VariantValue))
                    return VariantTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Race))
                    return RaceTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(CharacterClass))
                    return ClassTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Domain))
                    return DomainTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Spell))
                    return SpellTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Skill))
                    return SkillTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Feat))
                    return FeatTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(AreaEffect))
                    return AreaEffectTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Appearance))
                    return AppearanceTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(VisualEffect))
                    return VisualEffectTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Portrait))
                    return PortraitTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Polymorph))
                    return PolymorphTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(MasterFeat))
                    return MasterFeatTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Soundset))
                    return SoundsetTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(ClassPackage))
                    return PackageTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Disease))
                    return DiseaseTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Poison))
                    return PoisonTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Trap))
                    return TrapTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(BaseItem))
                    return BaseItemTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(ItemProperty))
                    return ItemPropertyTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(AppearanceSoundset))
                    return AppearanceSoundsetTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(WeaponSound))
                    return WeaponSoundTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(InventorySound))
                    return InventorySoundTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Companion))
                    return CompanionTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Familiar))
                    return FamiliarTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(ProgrammedEffect))
                    return ProgrammedEffectTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(DamageType))
                    return DamageTypeTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(DamageTypeGroup))
                    return DamageTypeGroupTemplate ?? ErrorTemplate;

                if (dataTypeDef.CustomType is CustomObject customObject)
                    return CustomObjectTemplate ?? ErrorTemplate;
                if (dataTypeDef.CustomType is CustomEnum customEnum)
                    return CustomEnumTemplate ?? ErrorTemplate;

                return ErrorTemplate;
            }

            return SpaceTemplate;
        }
    }
}
