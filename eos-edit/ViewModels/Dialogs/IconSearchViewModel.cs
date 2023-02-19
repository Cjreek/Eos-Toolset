using Eos.Nwn;
using Eos.Repositories;
using Eos.Services;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    internal class IconSearchViewModel : DialogViewModel
    {
        private String _searchText;
        private IEnumerable<String?> _searchResult;
        private IEnumerable<String?> _allIcons;

        public IconSearchViewModel()
        {
            _allIcons = MasterRepository.Resources.GetResourceKeys(NWNResourceType.TGA).Where(icon => (icon?.StartsWith("i", StringComparison.OrdinalIgnoreCase) ?? false) && !(icon?.StartsWith("iw", StringComparison.OrdinalIgnoreCase) ?? false) && !(icon?.StartsWith("ia", StringComparison.OrdinalIgnoreCase) ?? false));

            _searchText = "";
            _searchResult = _allIcons;
        }

        public String SearchText
        {
            get { return _searchText; }
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    NotifyPropertyChanged();
                    Search(_searchText.ToLower());
                }
            }
        }

        public IEnumerable<String?> SearchResult => _searchResult;
        public String? ResultResRef { get; set; }

        protected IEnumerable<String?> Search(String searchText)
        {
            _searchResult = _allIcons.Where(iconname => iconname?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false);
            NotifyPropertyChanged(nameof(SearchResult));

            return _searchResult;
        }

        protected override int GetDefaultWidth()
        {
            return 800;
        }

        protected override int GetDefaultHeight()
        {
            return 450;
        }

        protected override bool GetCanResize()
        {
            return true;
        }

        public DelegateCommand<IconSearchViewModel> CloseCommand { get; private set; } = new DelegateCommand<IconSearchViewModel>(vm =>
        {
            vm.ResultResRef = null;
            WindowService.Close(vm);
        });

        public DelegateCommand<IconSearchViewModel> OKCommand { get; private set; } = new DelegateCommand<IconSearchViewModel>(vm =>
        {
            WindowService.Close(vm);
        });
    }
}
