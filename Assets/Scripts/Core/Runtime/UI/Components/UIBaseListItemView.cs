// BaseListItemView.cs

using UniRx;
using UniRx.Toolkit;
using UnityEngine;

namespace Core.UI.Components
{
    public interface IListItemViewModel
    {
    }

    public interface IListItemView
    {
        void Bind(IListItemViewModel viewModel);
        void Unbind();
    }

    public abstract class ListItemViewBehaviour : MonoBehaviour, IListItemView
    {
        public abstract void Bind(IListItemViewModel viewModel);
        public abstract void Unbind();
    }

    public class ListItemViewPool : ObjectPool<ListItemViewBehaviour>
    {
        private readonly ListItemViewBehaviour _prefab;
        private readonly Transform _parent;

        public ListItemViewPool(ListItemViewBehaviour prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        protected override ListItemViewBehaviour CreateInstance()
        {
            var inst = Object.Instantiate(_prefab, _parent);
            inst.gameObject.SetActive(false);
            return inst;
        }

        protected override void OnBeforeRent(ListItemViewBehaviour instance)
        {
            instance.gameObject.SetActive(true);
        }

        protected override void OnBeforeReturn(ListItemViewBehaviour instance)
        {
            instance.Unbind();
            instance.gameObject.SetActive(false);
        }
    }

    public interface IItemHeightProvider
    {
        float GetPreferredHeight(IListItemViewModel viewModel);
    }

    public class ConstHeightProvider : IItemHeightProvider
    {
        private readonly float _height;

        public ConstHeightProvider(float height)
        {
            _height = Mathf.Max(1f, height);
        }

        public float GetPreferredHeight(IListItemViewModel viewModel) => _height;
    }

    public abstract class UIBaseListItemView<TViewModel> : ListItemViewBehaviour
        where TViewModel : class, IListItemViewModel
    {
        protected TViewModel ViewModel { get; private set; }
        private CompositeDisposable _disposable;

        public override void Bind(IListItemViewModel viewModel)
        {
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            ViewModel = viewModel as TViewModel;
            OnBind(ViewModel, _disposable);
        }

        public override void Unbind()
        {
            _disposable?.Dispose();
            _disposable = null;
            ViewModel = null;
            OnUnbind();
        }

        protected abstract void OnBind(TViewModel viewModel, CompositeDisposable disposable);

        protected virtual void OnUnbind()
        {
        }
    }
}