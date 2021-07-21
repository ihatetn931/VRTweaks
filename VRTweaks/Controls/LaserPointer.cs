using UnityEngine;

namespace VRTweaks.Controls
{
    class LaserPointer : MonoBehaviour
    {
		public static float hitDistance;
		public static LineRenderer DrawLine(Vector3 start, Vector3 end)
		{
			GameObject myLine = new GameObject();
			myLine.transform.position = start;
			myLine.AddComponent<LineRenderer>();
			LineRenderer lr = myLine.GetComponent<LineRenderer>();
			///ConstructableBase componentInParent = Builder.ghostModel.GetComponentInParent<ConstructableBase>();
			lr.material = new Material(Shader.Find("Sprites/Default"));
			lr.startColor = Color.cyan;
			lr.endColor = Color.blue;
			lr.startWidth = 0.01f;
			lr.endWidth = 0.02f;
			//lr.SetPosition(0, start);
			//lr.SetPosition(1, end * 10);
			myLine.transform.SetParent(VRHandsController.rightController.transform);
			GameObject.Destroy(myLine, 0.01f);
			return lr;
		}

		public static void UpdatePointer(LineRenderer line)
		{
			Vector3 endPosition = GetEnd();

			UpdateLength(line, endPosition);
		}

		public static void UpdateLength(LineRenderer lineRenderer, Vector3 endPosition)
		{
			lineRenderer.SetPosition(0, VRHandsController.rightController.transform.position);
			lineRenderer.SetPosition(1, endPosition);
		}

		public static Vector3 GetEnd()
		{
			//float distance = hitDistance;
			float distance = GetDistance();
			ConstructableBase componentInParent = Builder.ghostModel.GetComponentInParent<ConstructableBase>();
			Vector3 endPosition = CalculateEnd(componentInParent.placeMaxDistance);

			if (distance != 0.0f)
				endPosition = CalculateEnd(hitDistance);
			return endPosition;
		}

		public static Vector3 CalculateEnd(float length)
		{
			return VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.right * length;
		}

		public static float GetDistance()
		{
			RaycastHit hitObject;
			ConstructableBase componentInParent = Builder.ghostModel.GetComponentInParent<ConstructableBase>();
			Ray raycast = new Ray(VRHandsController.rightController.transform.position, VRHandsController.rightController.transform.right);
			bool rayHit = Physics.Raycast(raycast, out hitObject, Builder.placeMaxDistance);
			hitDistance = hitObject.distance;
			//PointerEventData _pointerEventData = new PointerEventData(EventSystem.current);
			//HandTargetEventData handTarget = new HandTargetEventData(EventSystem.current);

			//PointerEnter(hitObject, _pointerEventData, handTarget);

			return Builder.placeMaxDistance;
		}
	}
}
