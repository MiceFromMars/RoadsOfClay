using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROC.UI.Utils
{
	/// <summary>
	/// Moves a RectTransform through waypoints with natural jumping arcs.
	/// </summary>
	public class UIArcMovement : MonoBehaviour, IDisposable
	{
		[SerializeField] private RectTransform _targetRectTransform;
		[SerializeField] private RectTransform[] _pointTransforms;
		[SerializeField] private float _baseDuration = 0.5f;
		[SerializeField] private float _pauseDuration = 0.3f;
		[SerializeField] private float _minJumpHeight = 50f;
		[SerializeField] private float _maxJumpHeight = 150f;
		[SerializeField] private bool _autoStart = true;
		[SerializeField] private bool _randomizeStartPoint = false;

		[Header("Scaling Effect")]
		[SerializeField] private bool _useScaling = true;
		[SerializeField] private float _squashStrength = 0.2f;  // How much to squash at takeoff/landing
		[SerializeField] private float _stretchStrength = 0.1f; // How much to stretch at peak height

		private bool _isMoving;
		private int _currentIndex;
		private bool _movingForward = true;
		private float _timer;
		private bool _isPaused;
		private Vector2 _startPos;
		private Vector2 _endPos;
		private float _currentJumpHeight;
		private float _currentMoveDuration;
		private Vector3 _originalScale;
		private Vector2 _previousPos;
		private Vector2 _currentVelocity;

		private void Awake()
		{
			if (_targetRectTransform == null)
			{
				_targetRectTransform = GetComponent<RectTransform>();
			}

			_originalScale = _targetRectTransform.localScale;
		}

		private void OnEnable()
		{
			if (_autoStart)
			{
				StartMovement();
			}
		}

		private void Update()
		{
			if (!_isMoving || _pointTransforms == null || _pointTransforms.Length < 2)
				return;

			if (_isPaused)
			{
				_timer += Time.deltaTime;
				if (_timer >= _pauseDuration)
				{
					_isPaused = false;
					_timer = 0;
					PrepareNextMovement();
				}
				return;
			}

			_timer += Time.deltaTime;
			float normalizedTime = Mathf.Clamp01(_timer / _currentMoveDuration);

			if (normalizedTime >= 1.0f)
			{
				// Ensure we're exactly at the target position
				_targetRectTransform.anchoredPosition = _endPos;

				// Apply landing squash
				if (_useScaling)
				{
					ApplyLandingSquash();
				}

				_previousPos = _endPos;

				// Start pause or move to next point
				_isPaused = true;
				_timer = 0;
				return;
			}

			// Get previous position for velocity calculation
			Vector2 oldPos = _targetRectTransform.anchoredPosition;

			// Calculate position using natural jumping motion
			Vector2 newPos = CalculateJumpPosition(normalizedTime);
			_targetRectTransform.anchoredPosition = newPos;

			// Calculate actual movement velocity for this frame
			_currentVelocity = (newPos - oldPos) / Time.deltaTime;

			// Apply scaling if enabled - directly tied to jump phase
			if (_useScaling)
			{
				ApplyJumpScaling(normalizedTime);
			}

			_previousPos = newPos;
		}

		private Vector2 CalculateJumpPosition(float t)
		{
			// Linear movement for horizontal position
			float x = Mathf.Lerp(_startPos.x, _endPos.x, t);

			// Base y-position (linear path between start and end)
			float baseY = Mathf.Lerp(_startPos.y, _endPos.y, t);

			// Simple parabolic arc for jump: h * 4 * t * (1-t)
			// This creates a natural-looking jump that starts and ends at 0
			float jumpOffset = _currentJumpHeight * 4 * t * (1 - t);

			return new Vector2(x, baseY + jumpOffset);
		}

		private void ApplyJumpScaling(float normalizedTime)
		{
			// Calculate where we are in the jump curve
			// 0 = start, 0.5 = peak, 1 = end

			float xScale, yScale;

			// Determine if we're near takeoff, peak, or landing
			if (normalizedTime < 0.15f) // Takeoff phase
			{
				// Squash at takeoff - more squashed at the very beginning
				float takeoffFactor = 1f - normalizedTime / 0.15f;
				float squashAmount = _squashStrength * takeoffFactor;

				yScale = 1f - squashAmount;
				xScale = 1f + squashAmount * 0.7f;
			}
			else if (normalizedTime > 0.85f) // Landing phase
			{
				// Squash as approaching landing - more squashed closer to landing
				float landingFactor = (normalizedTime - 0.85f) / 0.15f;
				float squashAmount = _squashStrength * landingFactor;

				yScale = 1f - squashAmount;
				xScale = 1f + squashAmount * 0.7f;
			}
			else // Mid-air phase
			{
				// Calculate how close we are to the peak (0.5)
				float peakProximity = 1f - Mathf.Abs(normalizedTime - 0.5f) / 0.35f;
				peakProximity = Mathf.Max(0, peakProximity); // Ensure non-negative

				// Stretch at peak - most stretched at exactly the peak
				float stretchAmount = _stretchStrength * peakProximity;

				yScale = 1f + stretchAmount;
				xScale = 1f - stretchAmount * 0.3f;
			}

			// Apply with smoothing to avoid jerky transitions
			Vector3 targetScale = new Vector3(
				_originalScale.x * xScale,
				_originalScale.y * yScale,
				_originalScale.z
			);

			_targetRectTransform.localScale = Vector3.Lerp(
				_targetRectTransform.localScale,
				targetScale,
				Time.deltaTime * 15f  // Smooth transition speed
			);
		}

		private void ApplyLandingSquash()
		{
			// Apply strong squash on landing
			float yScale = 1f - _squashStrength;
			float xScale = 1f + _squashStrength * 0.7f;

			_targetRectTransform.localScale = new Vector3(
				_originalScale.x * xScale,
				_originalScale.y * yScale,
				_originalScale.z
			);
		}

		private void OnDestroy()
		{
			Dispose();
		}

		/// <summary>
		/// Starts the endless movement loop.
		/// </summary>
		public void StartMovement()
		{
			if (_isMoving)
				return;

			if (_pointTransforms == null || _pointTransforms.Length < 2)
			{
				Debug.LogError("UIArcMovement requires at least 2 point transforms to function.");
				return;
			}

			// Validate all point transforms
			for (int i = 0; i < _pointTransforms.Length; i++)
			{
				if (_pointTransforms[i] == null)
				{
					Debug.LogError($"UIArcMovement: Point transform at index {i} is null.");
					return;
				}
			}

			// Store original scale if not already stored
			if (_originalScale == Vector3.zero)
			{
				_originalScale = _targetRectTransform.localScale;
			}

			_isMoving = true;
			_timer = 0;
			_isPaused = false;

			if (_randomizeStartPoint)
			{
				_currentIndex = Random.Range(0, _pointTransforms.Length);
				_targetRectTransform.anchoredPosition = _pointTransforms[_currentIndex].anchoredPosition;
			}
			else
			{
				_currentIndex = 0;
				_targetRectTransform.anchoredPosition = _pointTransforms[0].anchoredPosition;
			}

			_previousPos = _targetRectTransform.anchoredPosition;

			// Apply initial takeoff squash
			if (_useScaling)
			{
				ApplyLandingSquash();
			}

			PrepareNextMovement();
		}

		/// <summary>
		/// Stops the movement loop.
		/// </summary>
		public void StopMovement()
		{
			_isMoving = false;

			// Reset scale to original when stopping
			if (_useScaling && _targetRectTransform != null)
			{
				_targetRectTransform.localScale = _originalScale;
			}
		}

		/// <summary>
		/// Set new points for the movement path.
		/// </summary>
		public void SetPoints(RectTransform[] newPoints)
		{
			if (newPoints == null || newPoints.Length < 2)
			{
				Debug.LogError("UIArcMovement requires at least 2 point transforms to function.");
				return;
			}

			// Validate all new point transforms
			for (int i = 0; i < newPoints.Length; i++)
			{
				if (newPoints[i] == null)
				{
					Debug.LogError($"UIArcMovement: New point transform at index {i} is null.");
					return;
				}
			}

			bool wasMoving = _isMoving;

			if (wasMoving)
			{
				StopMovement();
			}

			_pointTransforms = newPoints;

			if (wasMoving)
			{
				StartMovement();
			}
		}

		private void PrepareNextMovement()
		{
			int nextIndex = GetNextPointIndex();

			_startPos = _targetRectTransform.anchoredPosition;
			_endPos = _pointTransforms[nextIndex].anchoredPosition;

			// Calculate distance for height scaling
			float distance = Vector2.Distance(_startPos, _endPos);
			float distanceFactor = Mathf.Clamp01(distance / 300f);

			// Calculate jump height based on distance
			// Shorter jumps get less height, longer jumps get more height
			_currentJumpHeight = Mathf.Lerp(_minJumpHeight, _maxJumpHeight, distanceFactor);

			// Add small random variation to jump height (±20%)
			_currentJumpHeight *= Random.Range(0.8f, 1.2f);

			// Set movement duration based on distance
			// Important: Start from the base duration, don't compound on previous durations
			float durationFactor = Mathf.Sqrt(distanceFactor * 0.5f + 0.5f);
			_currentMoveDuration = _baseDuration * durationFactor;

			// Reset timer
			_timer = 0;

			// Update current index
			_currentIndex = nextIndex;

			// Check if we should reverse direction
			if (_currentIndex == 0 || _currentIndex == _pointTransforms.Length - 1)
			{
				_movingForward = !_movingForward;
			}
		}

		private int GetNextPointIndex()
		{
			if (_movingForward)
			{
				return (_currentIndex + 1) % _pointTransforms.Length;
			}
			else
			{
				return (_currentIndex - 1 + _pointTransforms.Length) % _pointTransforms.Length;
			}
		}

		public void Dispose()
		{
			StopMovement();
		}
	}
}