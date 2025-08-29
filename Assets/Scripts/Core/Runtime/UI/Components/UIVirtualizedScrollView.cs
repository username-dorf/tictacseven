using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public interface IListViewModelUntyped
    {
        ReactiveCollection<IListItemViewModel> Items { get; }
    }

    public class ListViewModel<TItemViewModel> : IListViewModelUntyped
        where TItemViewModel : class, IListItemViewModel
    {
        public ReactiveCollection<IListItemViewModel> Items { get; }

        public ListViewModel()
        {
            Items = new ReactiveCollection<IListItemViewModel>();
        }

        public void Add(TItemViewModel vm)
        {
            Items.Add(vm);
        }

        public void Insert(int i, TItemViewModel vm)
        {
            Items.Insert(i, vm);
        }

        public void RemoveAt(int i)
        {
            Items.RemoveAt(i);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }

    [DisallowMultipleComponent]
    public class UIVirtualizedScrollView : MonoBehaviour, IDisposable
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private ListItemViewBehaviour itemPrefab;
        private RectTransform _viewport;
        private RectTransform _content;

        [Header("Buffer")] 
        [SerializeField, Min(1)] private int prewarmMin = 6;
        [SerializeField, Min(0)] private int overdraw = 2;

        private IListViewModelUntyped _viewModel;
        private IItemHeightProvider _heightProvider;
        private VerticalLayoutGroup _verticalLayoutGroup;
        private CompositeDisposable _disposable;

        private ListItemViewPool _pool;
        private readonly List<ListItemViewBehaviour> _window = new();

        //Spacers
        private GameObject _topSpacer;
        private GameObject _bottomSpacer;

        private LayoutElement _topLayoutElement;
        private LayoutElement _bottomLayoutElement;

        private readonly List<float> _heights = new();
        private readonly List<float> _prefixPos = new();
        private float _avgHeight = 100f;
        private int _firstBound = -1;
        private float _lastNorm;


        public void Initialize(IListViewModelUntyped viewModel, IItemHeightProvider heightProvider = null)
        {
            if (scrollRect ==null) 
                throw new Exception("ScrollRect is null");
            
            _viewport ??= scrollRect.viewport;
            _content ??= scrollRect.content;
            
            _verticalLayoutGroup = _content.GetComponent<VerticalLayoutGroup>();
            if (_verticalLayoutGroup == null) 
                throw new Exception("Content must have VerticalLayoutGroup");

            if (itemPrefab == null) 
                throw new Exception("ItemPrefab is null");
            
            var layoutElement = itemPrefab.GetComponent<LayoutElement>();
            if (layoutElement == null || layoutElement.preferredHeight <= 0f)
                throw new Exception("ItemPrefab must have LayoutElement with preferredHeight > 0");

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _heightProvider = heightProvider ?? new ConstHeightProvider(layoutElement.preferredHeight);
            _avgHeight = Mathf.Max(1f, layoutElement.preferredHeight);

            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            CreateSpacers();
            _pool = new ListItemViewPool(itemPrefab, _content);
            _lastNorm = scrollRect.verticalNormalizedPosition;
            
            scrollRect.onValueChanged.AsObservable()
                .Subscribe(_ => UpdateByScroll())
                .AddTo(_disposable);
            
            Observable.EveryLateUpdate()
                .Subscribe(_ => NormalizeAndUpdateByScroll())
                .AddTo(_disposable);

            _viewModel.Items.ObserveAdd()
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    RebuildGeometry();
                    bool needRecreate = _window.Count == 0 || _window.Count > _viewModel.Items.Count;
                    Refresh(needRecreate);
                })
                .AddTo(_disposable);

            _viewModel.Items.ObserveRemove()
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    RebuildGeometry();
                    bool needRecreate = _viewModel.Items.Count == 0 || _window.Count > _viewModel.Items.Count;
                    Refresh(needRecreate);
                })
                .AddTo(_disposable);

            _viewModel.Items
                .ObserveReset()
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    RebuildGeometry();
                    Refresh(true);
                })
                .AddTo(_disposable);

            RebuildGeometry();
            Refresh(true);
            Canvas.ForceUpdateCanvases();
            SetScrollOffsetY(0f);
            UpdateByScroll();
        }

        public void Dispose()
        {
            TearDownWindow();
            _disposable?.Dispose();
        }

        public void ScrollToIndex(int index)
        {
            var count = _viewModel.Items.Count;
            if (count == 0) 
                return;
            index = Mathf.Clamp(index, 0, count - 1);

            float y = _verticalLayoutGroup.padding.top + _prefixPos[index];
            SetScrollOffsetY(y);
            UpdateByScroll();
        }

        public void SetPrewarmMin(int value)
        {
            prewarmMin = Mathf.Max(1, value);
            Refresh(true);
        }

        public void SetOverdraw(int value)
        {
            overdraw = Mathf.Max(0, value);
            Refresh(true);
        }
        
        private void CreateSpacers()
        {
            if (_topSpacer == null)
            {
                _topSpacer = new GameObject("__TopSpacer__", typeof(RectTransform), typeof(LayoutElement));
                _topSpacer.transform.SetParent(_content, false);
                _topLayoutElement = _topSpacer.GetComponent<LayoutElement>();
                _topLayoutElement.minHeight = 0;
                _topLayoutElement.preferredHeight = 0;
            }

            if (_bottomSpacer == null)
            {
                _bottomSpacer = new GameObject("__BottomSpacer__", typeof(RectTransform), typeof(LayoutElement));
                _bottomSpacer.transform.SetParent(_content, false);
                _bottomLayoutElement = _bottomSpacer.GetComponent<LayoutElement>();
                _bottomLayoutElement.minHeight = 0;
                _bottomLayoutElement.preferredHeight = 0;
            }
        }

        private void RebuildGeometry()
        {
            _heights.Clear();
            _prefixPos.Clear();
            _prefixPos.Add(0f);

            var sum = 0f;
            var spacing = _verticalLayoutGroup.spacing;
            _avgHeight = 0f;

            for (int i = 0; i < _viewModel.Items.Count; i++)
            {
                var h = Mathf.Max(1f, _heightProvider.GetPreferredHeight(_viewModel.Items[i]));
                _heights.Add(h);
                _avgHeight += h;
                sum += h;
                _prefixPos.Add(sum + spacing * (i + 1));
            }

            if (_viewModel.Items.Count > 0) 
                _avgHeight /= _viewModel.Items.Count;
        }

        private void Refresh(bool rebuildWindowSize)
        {
            var count = _viewModel.Items.Count;

            if (rebuildWindowSize)
            {
                int visible = EstimateVisibleByViewport() + overdraw * 2;
                int need = (count == 0)
                    ? 0
                    : Mathf.Min(Mathf.Max(prewarmMin, visible), count);

                RecreateWindow(need);
            }
            
            if (count == 0)
            {
                SetSpacers(0, 0);
                LayoutRebuilder.MarkLayoutForRebuild(_content);
                return;
            }

            _topSpacer.transform.SetSiblingIndex(0);
            for (int i = 0; i < _window.Count; i++)
                _window[i].transform.SetSiblingIndex(i + 1);
            _bottomSpacer.transform.SetSiblingIndex(_window.Count + 1);
            
            _firstBound = -1;
            UpdateByScroll();
        }

        private int EstimateVisibleByViewport()
        {
            if (_viewport == null) 
                return prewarmMin;
            var h = _viewport.rect.height;
            if (_viewModel.Items.Count == 0) 
                return 1;
            var n = Mathf.CeilToInt(h / Mathf.Max(1f, _avgHeight + _verticalLayoutGroup.spacing));
            return Mathf.Max(1, n);
        }

        private void NormalizeAndUpdateByScroll()
        {
            if (scrollRect == null)
                return;
            var norm = scrollRect.verticalNormalizedPosition;
            if (scrollRect.velocity.sqrMagnitude > 1f || !Mathf.Approximately(norm, _lastNorm))
            {
                UpdateByScroll();
                _lastNorm = norm;
            }
        }
        private void UpdateByScroll()
        {
            if (_viewModel.Items.Count == 0 || _window.Count == 0)
            {
                SetSpacers(0, 0);
                return;
            }

            var y = GetScrollOffsetY();
            int first = LowerBoundPrefix(y);
            first = Mathf.Clamp(first, 0, Math.Max(0, _viewModel.Items.Count - _window.Count));
            if (first != _firstBound)
            {
                _firstBound = first;
                BindWindowTo(first);
            }
        }

        private int LowerBoundPrefix(float y)
        {
            var low = 0;
            var high = _viewModel.Items.Count;
            while (low < high)
            {
                var mid = (low + high) >> 1;
                if (_prefixPos[mid] <= y)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }

            return Mathf.Clamp(low - 1, 0, _viewModel.Items.Count - 1);
        }

        private void BindWindowTo(int first)
        {
            var count = _viewModel.Items.Count;
            var last = Mathf.Min(count - 1, first + _window.Count - 1);
            var visibleCount = last >= first ? (last - first + 1) : 0;

            var sumHeightsBefore = SumHeights(0, first - 1);
            var sumHeightsVisible = SumHeights(first, last);
            var sumHeightsAfter = SumHeights(last + 1, count - 1);

            var spacingsBefore = first;
            var spacingsVisible = Math.Max(0, visibleCount - 1);
            var spacingsAfter = Math.Max(0, (count - 1) - first - spacingsVisible);

            var topH = first > 0 ? sumHeightsBefore + _verticalLayoutGroup.spacing * Mathf.Max(0, spacingsBefore - 1) : 0f;
            var botH = spacingsAfter > 0 ? sumHeightsAfter + _verticalLayoutGroup.spacing * Mathf.Max(0, spacingsAfter - 1) : 0f;

            SetSpacers(topH, botH);

            for (int i = 0; i < _window.Count; i++)
            {
                int idx = first + i;
                var view = _window[i];

                if (idx >= 0 && idx < count)
                {
                    view.Bind(_viewModel.Items[idx]);
                    if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);
                }
                else
                {
                    view.Unbind();
                    if (view.gameObject.activeSelf) view.gameObject.SetActive(false);
                }
            }
            LayoutRebuilder.MarkLayoutForRebuild(_content);
        }

        private float SumHeights(int from, int to)
        {
            if (_heights.Count == 0 || to < from)
                return 0f;
            var sum = 0f;
            from = Mathf.Clamp(from, 0, _heights.Count - 1);
            to = Mathf.Clamp(to, 0, _heights.Count - 1);
            for (int i = from; i <= to; i++)
            {
                sum += _heights[i];
            }
            return sum;
        }

        private void SetSpacers(float top, float bottom)
        {
            var topActive = top > 0.01f;
            var botActive = bottom > 0.01f;

            _topSpacer.SetActive(topActive);
            _bottomSpacer.SetActive(botActive);

            _topLayoutElement.preferredHeight = topActive ? top : 0f;
            _bottomLayoutElement.preferredHeight = botActive ? bottom : 0f;
        }

        private void RecreateWindow(int need)
        {
            TearDownWindow();

            for (var i = 0; i < need; i++)
            {
                var view = _pool.Rent();
                _window.Add(view);
            }

            _topSpacer.transform.SetSiblingIndex(0);
            for (var i = 0; i < _window.Count; i++)
            {
                _window[i].transform.SetSiblingIndex(i + 1);
            }
            _bottomSpacer.transform.SetSiblingIndex(_window.Count + 1);
        }

        private void TearDownWindow()
        {
            foreach (var v in _window)
            {
                v.Unbind();
                _pool.Return(v);
            }

            _window.Clear();
        }

        private float TotalContentHeight()
        {
            float sumHeights = 0f;
            foreach (var height in _heights)
            {
                sumHeights += height;
            }

            return _verticalLayoutGroup.padding.top
                   + sumHeights
                   + _verticalLayoutGroup.spacing * Mathf.Max(0, _heights.Count - 1)
                   + _verticalLayoutGroup.padding.bottom;
        }

        private float GetScrollOffsetY()
        {
            if (_viewport == null)
                return 0f;
            var total = TotalContentHeight();
            var viewHeight = _viewport.rect.height;
            var scrollable = Mathf.Max(0f, total - viewHeight);
            
            float norm = scrollRect.verticalNormalizedPosition;
            if (float.IsNaN(norm))
                norm = 1f;

            return (1f - norm) * scrollable;
        }

        private void SetScrollOffsetY(float y)
        {
            if (_viewport == null)
                return;
            var total = TotalContentHeight();
            var viewH = _viewport.rect.height;
            var scrollable = Mathf.Max(0f, total - viewH);
            var norm = scrollable <= 0f ? 1f : 1f - Mathf.Clamp01(y / scrollable);
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = norm;
        }
    }
}