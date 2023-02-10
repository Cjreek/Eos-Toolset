using Eos.Models;
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
    internal class GlobalSearchViewModel : DialogViewModel
    {
        private String _searchText;
        private IEnumerable<BaseModel?> _searchResult;
        private IEnumerable<IRepository> _source;

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

        public IEnumerable<BaseModel?> SearchResult => _searchResult;
        public BaseModel? ResultModel { get; set; }

        public GlobalSearchViewModel()
        {
            _searchText = "";
            _source = MasterRepository.Project.AllRepositories.Concat(MasterRepository.Standard.AllRepositories);
            _source = _source.Where(repo => repo.GetType() != typeof(ModelRepository<Appearance>));
            _searchResult = Search(_searchText);
        }

        private bool FilterModel(BaseModel? model, string searchText)
        {
            if ((model == null) || (MasterRepository.Project.HasOverride(model))) return false;

            var tlk = model.TlkDisplayName;
            if (tlk != null) return tlk[MasterRepository.Project.DefaultLanguage].Text.ToLower().Contains(searchText);
            return model.GetLabel().ToLower().Contains(searchText);
        }

        protected IEnumerable<BaseModel?> Search(String searchText)
        {
            _searchResult = _source.SelectMany(repo => repo.GetItems()).Where(model => FilterModel(model, searchText));
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

        public DelegateCommand<GlobalSearchViewModel> CloseCommand { get; private set; } = new DelegateCommand<GlobalSearchViewModel>(vm =>
        {
            vm.ResultModel = null;
            WindowService.Close(vm);
        });

        public DelegateCommand<GlobalSearchViewModel> OKCommand { get; private set; } = new DelegateCommand<GlobalSearchViewModel>(vm =>
        {
            if (vm.ResultModel?.Overrides != null)
            {
                vm.ResultModel = MasterRepository.Standard.GetByID(vm.ResultModel.GetType(), vm.ResultModel?.Overrides ?? Guid.Empty);
            }
            WindowService.Close(vm);
        });
    }
}
