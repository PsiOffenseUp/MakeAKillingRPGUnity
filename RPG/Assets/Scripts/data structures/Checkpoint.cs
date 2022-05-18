using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RadHare
{
	public class Checkpoint
	{
		public Vector3 position { get; private set; }
		bool isActive;

		public Checkpoint(Vector3 position)
		{
			this.position = position;
			isActive = false;
		}

		public void SetActive() { isActive = true; }
		public void SetInactive() { isActive = false; }
	}
}
