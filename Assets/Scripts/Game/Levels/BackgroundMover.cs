using UnityEngine;

namespace ROC.Game.Levels
{
	public sealed class BackgroundMover : MonoBehaviour
	{
		[Range(0f, 1f)]
		[SerializeField] private float yParallaxFactor = 0.3f;
		[SerializeField] private Level _level;
		[SerializeField] private Transform _background;

		private Camera _cam;

		private float _lvlLeftBoundX;
		private float _lvlRightBoundX;

		private float _bgWidth;
		private float _camHalfWidth;
		private float _bgMinPosX;
		private float _bgMaxPosX;

		private Vector3 _startPos;
		private Vector3 _startCamPos;

		private void Start()
		{
			_cam = Camera.main;

			var bounds = _level.GetLevelBounds();

			_lvlLeftBoundX = bounds.min.x;
			_lvlRightBoundX = bounds.max.x;

			_bgWidth = _background.GetComponent<SpriteRenderer>().bounds.size.x;
			_camHalfWidth = _cam.orthographicSize * _cam.aspect;

			_startPos = _background.position;
			_startCamPos = _cam.transform.position;

			_bgMinPosX = _lvlLeftBoundX - _camHalfWidth + (_bgWidth / 2f);
			_bgMaxPosX = _lvlRightBoundX + _camHalfWidth - (_bgWidth / 2f);
		}

		void LateUpdate()
		{
			if (_cam == null)
				return;

			var progress = Mathf.InverseLerp(_lvlLeftBoundX, _lvlRightBoundX, _cam.transform.position.x);

			var x = Mathf.Lerp(_bgMinPosX, _bgMaxPosX, progress);

			var y = _startPos.y + (_cam.transform.position.y - _startCamPos.y) * yParallaxFactor;

			_background.position = new Vector3(x, y, _background.position.z);
		}
	}
}