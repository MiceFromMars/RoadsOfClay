using System;
using UnityEngine;

namespace ROC.Game.Enemy
{
	public interface IEnemy
	{
		void Initialize(Action<IEnemy, int> onDeathCallback);
		void TakeDamage(int damage);
		void SetPosition(Vector3 position);
		GameObject GameObject { get; }
	}
}