using UnityEngine;
using UnityEngine.U2D;

namespace ROC.Utils
{
	[ExecuteInEditMode]
	public sealed class GroundGenerator : MonoBehaviour
	{
		[SerializeField] private SpriteShapeController _spriteShapeController;

		[SerializeField, Range(3, 100)] private int _groundLength = 50;
		[SerializeField, Range(1f, 50f)] private float _xMultiplyer = 2f;
		[SerializeField, Range(1f, 50f)] private float _yMultiplyer = 2f;
		[SerializeField, Range(0f, 1f)] private float _curveSmoothness = 0.5f;
		[SerializeField] private float _noiseStep = 0.5f;
		[SerializeField] private float _height = 10f;

		private Vector3 _lastPos;


		private void OnValidate()
		{
			_spriteShapeController.spline.Clear();

			for (int i = 0; i < _groundLength; i++)
			{
				_lastPos = transform.position + new Vector3(i * _xMultiplyer, Mathf.PerlinNoise(0, i * _noiseStep) * _yMultiplyer);
				_spriteShapeController.spline.InsertPointAt(i, _lastPos);

				if (i != 0 && i != _groundLength - 1)
				{
					_spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
					_spriteShapeController.spline.SetLeftTangent(i, Vector3.left * _xMultiplyer * _curveSmoothness);
					_spriteShapeController.spline.SetRightTangent(i, Vector3.right * _xMultiplyer * _curveSmoothness);
				}
			}

			_spriteShapeController.spline.InsertPointAt(_groundLength, new Vector3(_lastPos.x, transform.position.y - _height));
			_spriteShapeController.spline.InsertPointAt(_groundLength + 1, new Vector3(transform.position.x, transform.position.y - _height));
		}
	}
}
