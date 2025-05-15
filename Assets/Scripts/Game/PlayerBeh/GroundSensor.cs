using UnityEngine;

namespace ROC.Game.PlayerBeh
{
	public class GroundSensor : MonoBehaviour
	{
		public bool IsGrounded { get; private set; }

		private int _groundLayer;
		private float _bottomThreshold;

		private void Start()
		{
			_groundLayer = LayerMask.NameToLayer(Constants.GroundLayer);
			_bottomThreshold = GetComponent<CircleCollider2D>().radius / -3;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (IsInGroundLayer(collision.gameObject) && IsBelow(collision))
			{
				IsGrounded = true;
			}
		}

		private void OnCollisionStay2D(Collision2D collision)
		{
			if (IsInGroundLayer(collision.gameObject) && IsBelow(collision))
			{
				IsGrounded = true;
			}
		}

		private void OnCollisionExit2D(Collision2D collision)
		{
			if (IsInGroundLayer(collision.gameObject))
			{
				IsGrounded = false;
			}
		}

		private bool IsBelow(Collision2D collision)
		{
			foreach (var contact in collision.contacts)
			{
				Vector2 normal = contact.normal;
				if (normal.y > 0.5f && contact.point.y < transform.position.y + _bottomThreshold)
					return true;

			}
			return false;
		}

		private bool IsInGroundLayer(GameObject obj)
		{
			return obj.layer == _groundLayer;
		}
	}
}