using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo
{
	[ExecuteInEditMode]
	[AddComponentMenu("Face Camera")]
	public class FaceCamera : MonoBehaviour
	{
		void Update()
		{
			transform.rotation = Quaternion.LookRotation(
				transform.position - Camera.main.transform.position,
				Camera.main.transform.up);
		}
	}
}
