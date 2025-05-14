using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using UnityEngine;

namespace ROC.UI.Common
{
	/// <summary>
	/// Interface for presenters with covariance support
	/// </summary>
	public interface IPresenter<out TView> : IDisposable where TView : IView
	{
		void Initialize();
		UniTask Show(CancellationToken cancellationToken = default);
		UniTask Hide(CancellationToken cancellationToken = default);
	}

	/// <summary>
	/// Base implementation for all presenters in the MVP pattern
	/// </summary>
	/// <typeparam name="TView">The type of view this presenter is associated with</typeparam>
	public abstract class BasePresenter<TView> : IPresenter<TView> where TView : IView
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
				_cts?.Cancel();
				_cts?.Dispose();
				_cts = null;

				UnsubscribeFromEvents();
				OnDispose();

				// Handle view and its GameObject with extra care to avoid destroyed object access
				if (_view != null)
				{
					// Store the GameObject reference only if it's a valid UnityEngine.Object
					GameObject viewGameObject = null;
					bool isViewGameObjectValid = false;

					try
					{
						// This might throw if the GameObject is already destroyed
						viewGameObject = _view.GameObject;
						isViewGameObjectValid = viewGameObject != null && viewGameObject;
					}
					catch (Exception)
					{
						// GameObject is already destroyed or invalid, so we can't use it
						isViewGameObjectValid = false;
					}

					// First destroy the GameObject if needed and if it's still valid
					if (isViewGameObjectValid)
					{
						if (Application.isPlaying)
						{
							UnityEngine.Object.Destroy(viewGameObject);
						}
						else
						{
							UnityEngine.Object.DestroyImmediate(viewGameObject);
						}
					}

					// Then dispose the view after the GameObject is destroyed
					// This way the view won't try to access a destroyed GameObject
					try
					{
						_view.Dispose();
					}
					catch (Exception viewDisposeEx)
					{
						Debug.LogWarning($"Error disposing view: {viewDisposeEx.Message}");
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