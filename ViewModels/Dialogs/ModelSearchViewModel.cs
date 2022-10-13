using Eos.Models;
using Eos.Nwn.Tlk;
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
    public abstract class ModelSearchViewModel<T> : DialogViewModel where T : BaseModel, new()
    {
        private String _searchText;
        private IEnumerable<T?> _searchResult;
        private VirtualModelRepository<T> _repository;

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

        protected abstract TLKStringSet? GetModelText(T? model);

        private bool FilterOld(T? model, String searchText)
        {
            if (model == null) return false;

            var overrideModel = MasterRepository.Project.GetOverride(model);
            if (overrideModel != null)
            {
                //orderDict[overrideModel] = n;
                //n++;
                return false;
            }

            var tlk = GetModelText(model);
            if (tlk == null) return false;

            var result = tlk[TLKLanguage.English].Text.ToLower().Contains(searchText);

            //if (result && !orderDict.ContainsKey(model))
            //{
            //    orderDict[model] = n;
            //    n++;
            //}

            return result;
        }
        private bool Filter(T? model, String searchText)
        {
            if ((model == null) || (model.Overrides != null)) return false;
            var tlk = GetModelText(model);
            if (tlk == null) return false;
            return tlk[TLKLanguage.English].Text.ToLower().Contains(searchText);
        }

        protected IEnumerable<T?> Search(String searchText)
        {
            //Dictionary<BaseModel, int> orderDict = new Dictionary<BaseModel, int>();

            //var n = 0;
            _searchResult = _repository
                .Where(model => Filter(model, searchText));
             //   .OrderBy(model => model != null ? orderDict[model] : -1);

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

        public DelegateCommand<ModelSearchViewModel<T>> CloseCommand { get; private set; } = new DelegateCommand<ModelSearchViewModel<T>>(vm =>
        {
            vm.ResultModel = null;
            WindowService.Close(vm);
        });

        public DelegateCommand<ModelSearchViewModel<T>> OKCommand { get; private set; } = new DelegateCommand<ModelSearchViewModel<T>>(vm =>
        {
            WindowService.Close(vm);
        });
    }
}
