using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ROC.Core.Assets
{
	public interface IAssetsProvider : IDisposable
	{
		UniTask InitializeAsync();
		UniTask<T> LoadAssetAsync<T>(string address) where T : Object;
		UniTask<T> LoadAssetAsync<T>(AssetReference assetReference) where T : Object;
		UniTask<GameObject> InstantiateAsync(string address, Transform parent = null);
		UniTask<GameObject> InstantiateAsync(AssetReference assetReference, Transform parent = null);
		UniTask<IList<T>> LoadAssetsAsync<T>(IEnumerable<string> addresses) where T : Object;
		UniTask<T> LoadAssetFromLabelAsync<T>(string label) where T : Object;
		UniTask<IList<T>> LoadAssetsFromLabelAsync<T>(string label) where T : Object;
		UniTask<SceneInstance> LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single, CancellationToken cancellationToken = default);
		UniTask UnloadSceneAsync(SceneInstance scene, CancellationToken cancellationToken = default);
		void Release(Object asset);
		void CleanUp();
	}
}