using Eos.Models;
using Eos.Nwn.Tlk;
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
    public abstract class ModelSearchViewModel<T> : DialogViewModel where T : BaseModel, new()
    {
        private String _searchText;
        private IEnumerable<T?> _searchResult;
        private VirtualModelRepository<T> _repository;

        public bool IsTableSearch { get; set; }

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

        public ModelSearchViewModel(VirtualModelRepository<T> repository)
        {
            _repository = repository;
            _searchText = "";
            _searchResult = Search(_searchText);
        }

        public IEnumerable<T?> SearchResult => _searchResult;
        public T? ResultModel { get; set; }

        public int IconHeight { get; set; } = 24;
        public int IconWidth { get; set; } = 24;

        protected abstract TLKStringSet? GetModelText(T? model);

        private bool Filter(T? model, String searchText)
        {
            //if ((model == null) || (model.Overrides != null)) return false;
            if ((model == null) || (MasterRepository.Project.HasOverride(model))) return false;

            int searchNumber = -1;
            int.TryParse(searchText, out searchNumber);
            
            var tlk = GetModelText(model);
            if (tlk == null) return false;
            return tlk[MasterRepository.Project.DefaultLanguage].Text.ToLower().Contains(searchText) || (model.CalculatedIndex == searchNumber);
        }

        protected IEnumerable<T?> Search(String searchText)
        {
            _searchResult = _repository.Where(model => Filter(model, searchText));
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

        public ReactiveCommand<ModelSearchViewModel<T>, Unit> CloseCommand { get; private set; } = ReactiveCommand.Create<ModelSearchViewModel<T>>(vm =>
        {
            vm.ResultModel = null;
            WindowService.Close(vm);
        });

        public ReactiveCommand<ModelSearchViewModel<T>, Unit> OKCommand { get; private set; } = ReactiveCommand.Create<ModelSearchViewModel<T>>(vm =>
        {
            if (vm.ResultModel?.Overrides != null)
            {
                vm.ResultModel = (T?)MasterRepository.Standard.GetByID(typeof(T), vm.ResultModel?.Overrides ?? Guid.Empty);
            }

            WindowService.Close(vm);
        });
    }
}
