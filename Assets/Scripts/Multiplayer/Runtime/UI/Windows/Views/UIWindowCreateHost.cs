using System;
using System.Collections.Generic;
using System.Threading;
using Core.UI.Windows;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using Multiplayer.Lan;
using Multiplayer.Lan.Contracts;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Windows.Views
{
    public class UIWindowCreateHost: WindowView<UIWindowCreateHost.ViewModel>
    {
        [SerializeField] private UIButtonView closeButton;
        [SerializeField] private WaitingSubview waitingSubview;
        [SerializeField] private ApproveConnectionSubview approveConnectionSubview;
        private List<Subview<SubviewType>> _subviews;
        private ViewModel _viewModel;

        protected override UniTask BindViewAsync(ViewModel viewModel, CancellationToken ct)
        {
            _viewModel = viewModel;
            _subviews = new List<Subview<SubviewType>>(){waitingSubview, approveConnectionSubview};

            closeButton.Initialize(_viewModel.CloseWindow);
            
            _viewModel.JoinUsername
                .Subscribe(approveConnectionSubview.OnUsernameChanged)
                .AddTo(this);
            
            _viewModel.CurrentSubview
                .Subscribe(OnSubviewChanged)
                .AddTo(this);
            
            waitingSubview.CancelButton
                .OnClickAsObservable()
                .Subscribe(_ => _viewModel.CancelWaiting())
                .AddTo(this);

            approveConnectionSubview.CancelButton
                .OnClickAsObservable()
                .Subscribe(_ => _viewModel.CancelJoin())
                .AddTo(this);

            approveConnectionSubview.ApproveJoinButton
                .OnClickAsObservable()
                .Subscribe(_ => _viewModel.ApproveJoin())
                .AddTo(this);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnAfterOpenAsync(CancellationToken ct)
        {
            _viewModel.CreateSession();
            return UniTask.CompletedTask;
        }

        private void OnSubviewChanged(SubviewType subviewType)
        {
            foreach (var subview in _subviews)
            {
                var isActive = subview.Type == subviewType;
                subview.SetActiveAsync(isActive,CancellationToken.None);
            }
        }

        
        [Serializable]
        public abstract class Subview<T> where T : Enum
        {
            [SerializeField] private Transform parent;
            public abstract T Type { get; }

            public async UniTask SetActiveAsync(bool isActive, CancellationToken ct)
            {
                parent.gameObject.SetActive(isActive);
            }
        }

        public enum SubviewType
        {
            Waiting = 0,
            ApproveConnection,
        }

        [Serializable]
        public class WaitingSubview : Subview<SubviewType>
        {
            public override SubviewType Type => SubviewType.Waiting;
            
            [field: SerializeField] public Button CancelButton { get; protected set; }
        }

        [Serializable]
        public class ApproveConnectionSubview : Subview<SubviewType>
        {
            public override SubviewType Type => SubviewType.ApproveConnection;
            [field: SerializeField] public TMP_Text UsernameText { get; protected set; }
            [field: SerializeField] public Button ApproveJoinButton { get; protected set; }
            [field: SerializeField] public Button CancelButton { get; protected set; }

            public void OnUsernameChanged(string value)
            {
                UsernameText.text = value;
            }
        }

        public class ViewModel : IViewModel
        {
            public ReactiveProperty<SubviewType> CurrentSubview { get; }
            public ReactiveProperty<string> JoinUsername { get; }
            public ReactiveCommand<(NetworkConnection connection, JoinRequest request)> OnJoinRequestReceived { get; }
            
            private LanController _lanController;
            private IWindowsController _windowsController;
            private JoinApprovalService _joinApprovalService;
            private (NetworkConnection connection, JoinRequest request) _lastJoinRequest;

            public ViewModel(LanController lanController, IWindowsController windowsController, JoinApprovalService joinApprovalService)
            {
                _joinApprovalService = joinApprovalService;
                JoinUsername = new ReactiveProperty<string>();
                CurrentSubview = new ReactiveProperty<SubviewType>(SubviewType.Waiting);
                OnJoinRequestReceived = new ReactiveCommand<(NetworkConnection, JoinRequest)>();
                _windowsController = windowsController;
                _lanController = lanController;
            }

            public void CreateSession()
            {
                CurrentSubview.Value = SubviewType.Waiting;
                _lanController.CreateSession();
                _joinApprovalService.OnJoinRequested += OnJoinRequested;
            }

            public void CancelWaiting()
            {
                _lanController.CancelHosting();
                _ = _windowsController.CloseTopAsync();
                _joinApprovalService.OnJoinRequested -= OnJoinRequested;
            }

            public void CloseWindow()
            {
                CancelWaiting();
                if(_lastJoinRequest.connection is not null)
                    CancelJoin();
            }

            public void ApproveJoin()
            {
                _joinApprovalService.Approve(_lastJoinRequest.connection);
            }

            public void CancelJoin()
            {
                _joinApprovalService.Reject(_lastJoinRequest.connection);
            }

            private void OnJoinRequested(NetworkConnection connection, JoinRequest request)
            {
                CurrentSubview.Value = SubviewType.ApproveConnection;
                JoinUsername.Value = request.PlayerName;
                _lastJoinRequest = (connection, request);
                OnJoinRequestReceived?.Execute(_lastJoinRequest);
            }
            
            public void Dispose()
            {
                _joinApprovalService.OnJoinRequested -= OnJoinRequested;
            }
        }
    }
}