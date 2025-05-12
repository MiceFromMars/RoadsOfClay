using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
using System.Threading;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ROC.Core.Assets
{
	public class AssetsProvider : IAssetsProvider
	{
		private readonly Dictionary<string, AsyncOperationHandle> _completedCache = new();
		private readonly Dictionary<string, List<AsyncOperationHandle>> _handles = new();
		private readonly Dictionary<GameObject, string> _instanceToKey = new();
		private bool _isInitialized;

		public async UniTask InitializeAsync(CancellationToken cancellationToken)
		{
			if (_isInitialized)
				return;

			await Addressables.InitializeAsync().ToUniTask(cancellationToken: cancellationToken);
			_isInitialized = true;
		}

		public async UniTask<T> LoadAssetAsync<T>(string address) where T : Object
		{
			if (_completedCache.TryGetValue(address, out AsyncOperationHandle completedHandle))
				return (T)completedHandle.Result;

			AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
			await handle.ToUniTask();

			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				_completedCache[address] = handle;
				AddHandle(address, handle);
				return handle.Result;
			}

			Debug.LogError($"Failed to load asset at address: {address}");
			return null;
		}

		public async UniTask<T> LoadAssetAsync<T>(AssetReference assetReference) where T : Object
		{
			string key = assetReference.AssetGUID;

			if (_completedCache.TryGetValue(key, out AsyncOperationHandle completedHandle))
				return (T)completedHandle.Result;

			AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetReference);
			await handle.ToUniTask();

			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				_completedCache[key] = handle;
				AddHandle(key, handle);
				return handle.Result;
			}

			Debug.LogError($"Failed to load asset with reference: {key}");
			return null;
		}

		public async UniTask<GameObject> InstantiateAsync(string address, Transform parent = null)
		{
			AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, parent);
			await handle.ToUniTask();

			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				GameObject instance = handle.Result;
				AddHandle(address, handle);

				// Track the instance to its source key
				_instanceToKey[instance] = address;

				return instance;
			}

			Debug.LogError($"Failed to instantiate asset at address: {address}");
			return null;
		}

		public async UniTask<GameObject> InstantiateAsync(AssetReference assetReference, Transform parent = null)
		{
			string key = assetReference.AssetGUID;

			AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(assetReference, parent);
			await handle.ToUniTask();

			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				GameObject instance = handle.Result;
				AddHandle(key, handle);

				// Track the instance to its source key
				_instanceToKey[instance] = key;

				return instance;
			}

			Debug.LogError($"Failed to instantiate asset with reference: {key}");
			return null;
		}

		public async UniTask<IList<T>> LoadAssetsAsync<T>(IEnumerable<string> addresses) where T : Object
		{
			AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(
				addresses,
				addressable => { },
				Addressables.MergeMode.Union);

			await handle.ToUniTask();

			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				// We're using a generated key for this composite operation
				string key = "multi_" + Guid.NewGuid().ToString();
				AddHandle(key, handle);
				return handle.Result;
			}

			Debug.LogError($"Failed to load multiple assets");
			return null;
		}

		public async UniTask<T> LoadAssetFromLabelAsync<T>(string label) where T : Object
		{
			IList<T> assets = await LoadAssetsFromLabelAsync<T>(label);
			return assets != null && assets.Count > 0 ? assets[0] : null;
		}

		public async UniTask<IList<T>> LoadAssetsFromLabelAsync<T>(string label) where T : Object
		{
			if (_completedCache.TryGetValue(label, out AsyncOperationHandle completedHandle))
				return (IList<T>)completedHandle.Result;

			AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, null);
			await handle.ToUniTask();

			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				_completedCache[label] = handle;
				AddHandle(label, handle);
				return handle.Result;
			}

			Debug.LogError($"Failed to load assets with label: {label}");
			return null;
		}

		public void Release(Object asset)
		{
			if (asset == null)
				return;

			if (asset is GameObject gameObject)
			{
				ReleaseGameObject(gameObject);
				return;
			}

			// For non-GameObject assets, we need to find the key in our cache
			string assetKey = FindKeyForAsset(asset);
			if (!string.IsNullOrEmpty(assetKey))
			{
				if (_completedCache.TryGetValue(assetKey, out AsyncOperationHandle handle))
				{
					// Only release if this is the exact asset we cached
					if ((Object)handle.Result == asset)
					{
						Addressables.Release(handle);
						_completedCache.Remove(assetKey);
						RemoveHandlesByKey(assetKey);
					}
				}
			}
			else
			{
				// If we can't find it in our cache, try to release it directly
				Addressables.Release(asset);
			}
		}

		public void CleanUp()
		{
			// Release all instances first
			foreach (var instance in new List<GameObject>(_instanceToKey.Keys))
			{
				ReleaseGameObject(instance);
			}

			_instanceToKey.Clear();

			// Release remaining handles
			foreach (var handlesList in _handles.Values)
			{
				foreach (AsyncOperationHandle handle in handlesList)
				{
					if (handle.IsValid())
					{
						Addressables.Release(handle);
					}
				}
			}

			_handles.Clear();
			_completedCache.Clear();
		}

		public void Dispose()
		{
			CleanUp();
		}

		private void AddHandle(string key, AsyncOperationHandle handle)
		{
			if (!_handles.TryGetValue(key, out List<AsyncOperationHandle> handlesList))
			{
				handlesList = new List<AsyncOperationHandle>();
				_handles[key] = handlesList;
			}

			handlesList.Add(handle);
		}

		private void ReleaseGameObject(GameObject gameObject)
		{
			if (_instanceToKey.TryGetValue(gameObject, out string key))
			{
				// Remove from our tracking dictionary
				_instanceToKey.Remove(gameObject);

				// Find and remove the specific handle for this instance
				if (_handles.TryGetValue(key, out var handlesList))
				{
					for (int i = handlesList.Count - 1; i >= 0; i--)
					{
						var handle = handlesList[i];
						if (handle.IsValid() && handle.Result is GameObject result && result == gameObject)
						{
							Addressables.ReleaseInstance(gameObject);
							handlesList.RemoveAt(i);
							break;
						}
					}

					// If no more handles for this key, clean up
					if (handlesList.Count == 0)
					{
						_handles.Remove(key);
					}
				}
				else
				{
					// If we somehow don't have the handle tracked, still release the instance
					Addressables.ReleaseInstance(gameObject);
				}
			}
			else
			{
				// If we don't have a record of this GameObject, just release it
				Addressables.ReleaseInstance(gameObject);
			}
		}

		private string FindKeyForAsset(Object asset)
		{
			foreach (var entry in _completedCache)
			{
				if (entry.Value.IsValid() && (Object)entry.Value.Result == asset)
				{
					return entry.Key;
				}
			}

			return null;
		}

		private void RemoveHandlesByKey(string key)
		{
			if (_handles.TryGetValue(key, out var handlesList))
			{
				// The actual release has already happened, so we just need to clean up our tracking
				_handles.Remove(key);
			}
		}

		public async UniTask<SceneInstance> LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single, CancellationToken cancellationToken = default)
		{
			AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneName, loadMode);
			await handle.ToUniTask(cancellationToken: cancellationToken);

			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				string key = $"scene_{sceneName}";
				AddHandle(key, handle);
				return handle.Result;
			}

			Debug.LogError($"Failed to load scene: {sceneName}");
			return default;
		}

		public async UniTask UnloadSceneAsync(SceneInstance scene, CancellationToken cancellationToken = default)
		{
			if (scene.Scene.IsValid())
			{
				AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync(scene);
				await handle.ToUniTask(cancellationToken: cancellationToken);

				string key = $"scene_{scene.Scene.name}";

				if (_handles.TryGetValue(key, out var handlesList))
				{
					for (int i = handlesList.Count - 1; i >= 0; i--)
					{
						if (handlesList[i].Result is SceneInstance sceneInstance &&
							sceneInstance.Scene.handle == scene.Scene.handle)
						{
							handlesList.RemoveAt(i);
							break;
						}
					}
				}
			}
		}
	}
}