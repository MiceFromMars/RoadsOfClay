using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using UnityEngine;

namespace ROC.UI.Common
{
	/// <summary>
	/// Base implementation for all presenters in the MVP pattern
	/// </summary>
	/// <typeparam name="TView">The type of view this presenter is associated with</typeparam>
	public abstract class BasePresenter<TView> : IPresenter where TView : IView
	{
		protected readonly TView _view;
		protected readonly IEventBus _eventBus;
		protected CancellationTokenSource _cts = new();
		protected bool _isInitialized;
		protected bool _isDisposed;

		protected BasePresenter(TView view, IEventBus eventBus)
		{
			_view = view ?? throw new ArgumentNullException(nameof(view));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
		}

		public virtual void Initialize()
		{
			if (_isInitialized)
				return;

			_isInitialized = true;
			SubscribeToEvents();
			OnInitialize();
		}

		protected virtual void OnInitialize() { }

		protected virtual void SubscribeToEvents() { }

		protected virtual void UnsubscribeFromEvents() { }

		public virtual async UniTask Show(CancellationToken cancellationToken = default)
		{
			if (!_isInitialized)
			{
				Initialize();
			}

			await OnBeforeShow();
			await _view.Show(cancellationToken);
			await OnAfterShow();
		}

		protected virtual UniTask OnBeforeShow()
		{
			return UniTask.CompletedTask;
		}

		protected virtual UniTask OnAfterShow()
		{
			return UniTask.CompletedTask;
		}

		public virtual async UniTask Hide(CancellationToken cancellationToken = default)
		{
			await OnBeforeHide();
			await _view.Hide(cancellationToken);
			await OnAfterHide();
		}

		protected virtual UniTask OnBeforeHide()
		{
			return UniTask.CompletedTask;
		}

		protected virtual UniTask OnAfterHide()
		{
			return UniTask.CompletedTask;
		}

		public virtual void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			try
			{
				_cts.Cancel();
				_cts.Dispose();
				_cts = null;

				UnsubscribeFromEvents();
				OnDispose();

				// Dispose the view if it's disposable
				if (_view is IDisposable disposableView)
				{
					disposableView.Dispose();
				}

				// Destroy the view GameObject if applicable
				if (_view is MonoBehaviour viewBehaviour && viewBehaviour != null)
				{
					if (viewBehaviour.gameObject != null)
					{
						// Use Destroy or DestroyImmediate based on whether we're in play mode
						if (Application.isPlaying)
						{
							UnityEngine.Object.Destroy(viewBehaviour.gameObject);
						}
						else
						{
							UnityEngine.Object.DestroyImmediate(viewBehaviour.gameObject);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error during presenter disposal: {ex.Message}");
			}
		}

		/// <summary>
		/// Override this method to implement custom disposal logic in derived classes.
		/// This is called during the Dispose method, before the view is disposed.
		/// </summary>
		protected virtual void OnDispose() { }
	}
}