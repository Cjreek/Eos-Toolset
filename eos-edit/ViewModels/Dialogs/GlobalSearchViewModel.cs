using Eos.Models;
using Eos.Repositories;
using Eos.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    public class GlobalSearchViewModel : DialogViewModel
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

            int searchNumber;
            if (!int.TryParse(searchText, out searchNumber))
                searchNumber = -9999;
            
            var tlk = model.TlkDisplayName;
            if (tlk != null) return tlk[MasterRepository.Project.DefaultLanguage].Text.ToLower().Contains(searchText) || (model.CalculatedIndex == searchNumber);
            return model.GetLabel().ToLower().Contains(searchText) || (model.CalculatedIndex == searchNumber);
        }

        protected IEnumerable<BaseModel?> Search(String searchText)
        {
            _searchResult = _source.SelectMany(repo => repo.GetItems()).Where(model => FilterModel(model, searchText));
            NotifyPropertyChanged(nameof(SearchResult));

            return _searchResult;
        }

        protected override string GetWindowTitle()
        {
            return "Global Search";
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

        public ReactiveCommand<GlobalSearchViewModel, Unit> CloseCommand { get; private set; } = ReactiveCommand.Create<GlobalSearchViewModel>(vm =>
        {
            vm.ResultModel = null;
            WindowService.Close(vm);
        });

        public ReactiveCommand<GlobalSearchViewModel, Unit> OKCommand { get; private set; } = ReactiveCommand.Create<GlobalSearchViewModel>(vm =>
        {
            if (vm.ResultModel?.Overrides != null)
            {
                vm.ResultModel = MasterRepository.Standard.GetByID(vm.ResultModel.GetType(), vm.ResultModel?.Overrides ?? Guid.Empty);
            }

            WindowService.Close(vm);
        });
    }
}
