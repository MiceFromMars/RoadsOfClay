using UnityEngine;

namespace ROC.Data.Config
{
	[CreateAssetMenu(fileName = "AppConfig", menuName = "ROC/Config/AppConfig")]
	public class AppConfig : ScriptableObject
	{
		[SerializeField] private int _vSyncCount = 0;
		[SerializeField] private int _targetFrameRate = 120;

		public int VSyncCount => _vSyncCount;
		public int TargetFrameRate => _targetFrameRate;

	}
}