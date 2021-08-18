using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VRTweaks.Controls.UI
{

/*	[HarmonyPatch(typeof(uGUI_GraphicRaycaster), nameof(uGUI_GraphicRaycaster.UpdateGraphicRaycasters))]
	public static class uGUI_GraphicRaycaster_UpdateGraphicRaycasters__Patch
	{
		[HarmonyPrefix]
		static bool Prefix(uGUI_GraphicRaycaster __instance)
		{
			foreach (uGUI_GraphicRaycaster uGUI_GraphicRaycaster in uGUI_GraphicRaycaster.allRaycasters)
			{
				if (uGUI_GraphicRaycaster.updateRaycasterStatusDelegate != null)
				{
					uGUI_GraphicRaycaster.updateRaycasterStatusDelegate(uGUI_GraphicRaycaster);
				}
				else if (uGUI_GraphicRaycaster.interactionDistance > 0f)
				{
					//Player.mainObject.transform.position
					if (Vector3.SqrMagnitude(VRHandsController.rightController.transform.position - uGUI_GraphicRaycaster.transform.position) < uGUI_GraphicRaycaster.interactionDistance * uGUI_GraphicRaycaster.interactionDistance)
					{
						uGUI_GraphicRaycaster.enabled = true;
					}
					else
					{
						uGUI_GraphicRaycaster.enabled = false;
					}
				}
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(GamepadInputModule), nameof(GamepadInputModule.HandleNavigation))]
	public static class GamepadInputModule_HandleNavigation__Patch
	{
		[HarmonyPrefix]
		static bool Prefix(GamepadInputModule __instance)
		{
			ErrorMessage.AddDebug("currentNavigableGrid: " + __instance.currentNavigableGrid);
			if (__instance.currentNavigableGrid != null)
			{
				//ErrorMessage.AddDebug("CallingOther");
				bool flag = __instance.currentNavigableGrid.SelectItemInDirection(__instance.moveDirection.x, __instance.moveDirection.y);
				ErrorMessage.AddDebug("Flag: " + flag);
				if ((__instance.moveDirection.x != 0 || __instance.moveDirection.y != 0) && !flag)
				{
					uGUI_INavigableIconGrid navigableGridInDirection = __instance.currentNavigableGrid.GetNavigableGridInDirection(__instance.moveDirection.x, __instance.moveDirection.y);
					if (navigableGridInDirection != null)
					{
						Graphic selectedIcon = __instance.currentNavigableGrid.GetSelectedIcon();
						if (selectedIcon == null)
						{
							navigableGridInDirection.SelectFirstItem();
							__instance.currentNavigableGrid = navigableGridInDirection;
						}
						else
						{
							Vector3 position = selectedIcon.rectTransform.position;
							object selectedItem = __instance.currentNavigableGrid.GetSelectedItem();
							__instance.currentNavigableGrid.DeselectItem();
							if (navigableGridInDirection.SelectItemClosestToPosition(position))
							{
								__instance.currentNavigableGrid = navigableGridInDirection;
							}
							else
							{
								__instance.currentNavigableGrid.SelectItem(selectedItem);
							}
						}
					}
				}
				if (__instance.currentNavigableGrid != null)
				{
					Graphic selectedIcon2 = __instance.currentNavigableGrid.GetSelectedIcon();
					ErrorMessage.AddDebug("selectedIcon2: " + selectedIcon2);
					if (selectedIcon2 != null)
					{
						RaycastResult raycastResult = default(RaycastResult);
						Canvas canvas = selectedIcon2.canvas;
						ErrorMessage.AddDebug("CanvasName: " + canvas.name);
						if (canvas != null)
						{
							BaseRaycaster component = canvas.GetComponent<BaseRaycaster>();
							ErrorMessage.AddDebug("GOName: " + component.gameObject.name);
							if (component != null)
							{
								RectTransform rectTransform = selectedIcon2.rectTransform;
								Vector3 vector = VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.forward * FPSInputModule.current.maxInteractionDistance;//rectTransform.TransformPoint(rectTransform.rect.center);
								raycastResult.gameObject = selectedIcon2.gameObject;
								raycastResult.module = component;
								raycastResult.index = 0f;
								raycastResult.depth = selectedIcon2.depth;
								raycastResult.sortingLayer = canvas.sortingLayerID;
								raycastResult.sortingOrder = canvas.sortingOrder;
								Camera eventCamera = component.eventCamera;
								if (eventCamera == null)
								{
									ErrorMessage.AddDebug("EventCamera is not null");
									raycastResult.distance = (vector - eventCamera.transform.position).magnitude;
									Vector3 vector2 = eventCamera.WorldToScreenPoint(vector);
									raycastResult.screenPosition = new Vector2(vector2.x, vector2.y);
								}
								else
								{
									raycastResult.distance = 0f;
									raycastResult.screenPosition = Vector2.zero;
								}
								raycastResult.worldPosition = vector;
								raycastResult.worldNormal = rectTransform.up;
								CursorManager.SetRaycastResult(raycastResult);
							}
						}
					}
				}
			}
			return false;
		}
	}

	/*class GamePadInputMod : BaseRaycaster
	{
		public static Canvas m_Canvas;
		public static LayerMask m_BlockingMask = -1;
		public static List<Graphic> m_RaycastResults = new List<Graphic>();
		private static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();

		[FormerlySerializedAs("blockingObjects")]
		[SerializeField]
		private BlockingObjects m_BlockingObjects;

		[FormerlySerializedAs("ignoreReversedGraphics")]
		[SerializeField]
		private bool m_IgnoreReversedGraphics = true;

		public enum BlockingObjects
		{
			// Token: 0x04000239 RID: 569
			None,
			// Token: 0x0400023A RID: 570
			TwoD,
			// Token: 0x0400023B RID: 571
			ThreeD,
			// Token: 0x0400023C RID: 572
			All
		}

		public BlockingObjects blockingObjects
		{
			get
			{
				return this.m_BlockingObjects;
			}
			set
			{
				this.m_BlockingObjects = value;
			}
		}
		public static Canvas can;
		public Canvas canvas
		{
			get
			{
				if (m_Canvas != null)
				{
					return m_Canvas;
				}
				m_Canvas = base.GetComponent<Canvas>();
				can = canvas;
				ErrorMessage.AddDebug("m_Canvas: " + m_Canvas);
				ErrorMessage.AddDebug("BaseName: " + base.name);
				return m_Canvas;
			}
		}

		public bool ignoreReversedGraphics
		{
			get
			{
				return this.m_IgnoreReversedGraphics;
			}
			set
			{
				this.m_IgnoreReversedGraphics = value;
			}
		}

		public static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
		{
			ErrorMessage.AddDebug("ThisRaycast");
			int count = foundGraphics.Count;
			for (int i = 0; i < count; i++)
			{
				Graphic graphic = foundGraphics[i];
				if (graphic.raycastTarget && !graphic.canvasRenderer.cull && graphic.depth != -1 && RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera) && (!(eventCamera != null) || eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z <= eventCamera.farClipPlane) && graphic.Raycast(pointerPosition, eventCamera))
				{
					GamePadInputMod.s_SortedGraphics.Add(graphic);
				}
			}
			GamePadInputMod.s_SortedGraphics.Sort((Graphic g1, Graphic g2) => g2.depth.CompareTo(g1.depth));
			count = GamePadInputMod.s_SortedGraphics.Count;
			for (int j = 0; j < count; j++)
			{
				results.Add(GamePadInputMod.s_SortedGraphics[j]);
			}
			GamePadInputMod.s_SortedGraphics.Clear();
		}

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
			ErrorMessage.AddDebug("eventData.pointerCurrentRaycast.gameObject: " + eventData.pointerCurrentRaycast.gameObject);
        }

		public override Camera eventCamera
		{
			get
			{
				if (this.canvas.renderMode == RenderMode.ScreenSpaceOverlay || (this.canvas.renderMode == RenderMode.ScreenSpaceCamera && this.canvas.worldCamera == null))
				{
					return null;
				}
				if (!(this.canvas.worldCamera != null))
				{
					return Camera.main;
				}
				cam = this.canvas.worldCamera;
				return this.canvas.worldCamera;
			}
		}
		public static Camera cam;

	}
	
	[HarmonyPatch(typeof(GraphicRaycaster), nameof(GraphicRaycaster.Raycast), new Type[] { typeof(PointerEventData), typeof(List<RaycastResult>)})]
	public static class GraphicRaycaster_Raycast__Patch
	{
		public static Canvas m_Canvas;
		public static GraphicRaycaster graphics;
		[SerializeField]
		public static LayerMask m_BlockingMask = -1;
		[NonSerialized]
		public static List<Graphic> m_RaycastResults = new List<Graphic>();
		[NonSerialized]
		public static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();

		private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
		{
			int count = foundGraphics.Count;
			for (int i = 0; i < count; i++)
			{
				Graphic graphic = foundGraphics[i];
				//ErrorMessage.AddDebug("raycastTarget: " + graphic.raycastTarget);
				//ErrorMessage.AddDebug("cull: " + graphic.canvasRenderer.cull);
				ErrorMessage.AddDebug("RectangleContainsScreenPoint: " + RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, Camera.main.WorldToScreenPoint(VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.right * FPSInputModule.current.maxInteractionDistance), eventCamera));
				//if(eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z <= eventCamera.farClipPlane)
					//ErrorMessage.AddDebug("WorldToScreenPoint: " + eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z);
				//ErrorMessage.AddDebug("Raycast: " + graphic.Raycast(pointerPosition, eventCamera));
				if (graphic.raycastTarget 
					&& !graphic.canvasRenderer.cull 
					&& graphic.depth != -1 
					//&& RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera) 
					&& (!(eventCamera != null) 
					|| eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z <= eventCamera.farClipPlane) 
					&& graphic.Raycast(pointerPosition, eventCamera))
				{
					s_SortedGraphics.Add(graphic);
					//ErrorMessage.AddDebug("s_SortedGraphicsCount: " + s_SortedGraphics.Count);
				}
			}
			s_SortedGraphics.Sort((Graphic g1, Graphic g2) => g2.depth.CompareTo(g1.depth));
			count = s_SortedGraphics.Count;
			//ErrorMessage.AddDebug("Count: " + count);
			for (int j = 0; j < count; j++)
			{
				results.Add(s_SortedGraphics[j]);
			}
			//ErrorMessage.AddDebug("Results Count: " + results.Count);
			s_SortedGraphics.Clear();
		}

		public static Canvas canvas
		{
			get
			{
				if (m_Canvas != null)
				{
					return m_Canvas;
				}
				m_Canvas = graphics.GetComponent<Canvas>();
				return m_Canvas;
			}
		}

		[HarmonyPrefix]
		static bool Prefix(PointerEventData eventData, List<RaycastResult> resultAppendList, GraphicRaycaster __instance)
		{
			graphics = __instance;
			if (canvas == null)
			{
				ErrorMessage.AddDebug("m_Canvas is null");
				return false;
			}
			IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(canvas);
			if (graphicsForCanvas == null || graphicsForCanvas.Count == 0)
			{
				ErrorMessage.AddDebug("graphicsForCanvas is null or graphicsForCanvas count equals 0");
				return false;
			}
			Camera eventCamera = Camera.main;
			int targetDisplay;
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || eventCamera == null)
			{
				targetDisplay = canvas.targetDisplay;
			}
			else
			{
				targetDisplay = eventCamera.targetDisplay;
			}
			//ErrorMessage.AddDebug("Name: " + eventData.position);
			Vector3 vector = Display.RelativeMouseAt(eventData.position);
			if (vector != Vector3.zero)
			{
				if ((int)vector.z != targetDisplay)
				{
					ErrorMessage.AddDebug("not target");
					return false;
				}
			}
			else
			{
				vector = eventData.position;
			}
			Vector2 vector2;
			if (eventCamera == null)
			{
				ErrorMessage.AddDebug("eventCamera is null doing display stuff");
				float num = (float)Screen.width;
				float num2 = (float)Screen.height;
				if (targetDisplay > 0 && targetDisplay < Display.displays.Length)
				{
					num = (float)Display.displays[targetDisplay].systemWidth;
					num2 = (float)Display.displays[targetDisplay].systemHeight;
				}
				vector2 = new Vector2(vector.x / num, vector.y / num2);
			}
			else
			{
				vector2 = eventCamera.ScreenToViewportPoint(vector);
			}
			if (vector2.x < 0f || vector2.x > 1f || vector2.y < 0f || vector2.y > 1f)
			{
				return false;
			}
			float num3 = float.MaxValue;
			Ray r = default(Ray);
			if (eventCamera != null)
			{
				r = eventCamera.ScreenPointToRay(vector);
				//ErrorMessage.AddDebug("R: " + r);
			}
			//ErrorMessage.AddDebug("renderMode: " + canvas.renderMode);
		//	ErrorMessage.AddDebug("blockingObjects: " + __instance.blockingObjects);
			if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && __instance.blockingObjects != GraphicRaycaster.BlockingObjects.None)
			{
				float f = 100f;
				if (eventCamera != null)
				{
					float z = r.direction.z;
					f = (Mathf.Approximately(0f, z) ? float.PositiveInfinity : Mathf.Abs((eventCamera.farClipPlane - eventCamera.nearClipPlane) / z));
				}
				if ((__instance.blockingObjects == GraphicRaycaster.BlockingObjects.ThreeD || __instance.blockingObjects == GraphicRaycaster.BlockingObjects.All) && ReflectionMethodsCache.Singleton.raycast3D != null)
				{
					RaycastHit[] array = ReflectionMethodsCache.Singleton.raycast3DAll(r, f, m_BlockingMask);
					if (array.Length != 0)
					{
						num3 = array[0].distance;
					}
				}
				if ((__instance.blockingObjects == GraphicRaycaster.BlockingObjects.TwoD || __instance.blockingObjects == GraphicRaycaster.BlockingObjects.All) && ReflectionMethodsCache.Singleton.raycast2D != null)
				{
					RaycastHit2D[] array2 = ReflectionMethodsCache.Singleton.getRayIntersectionAll(r, f, m_BlockingMask);
					if (array2.Length != 0)
					{
						num3 = array2[0].distance;
					}
				}
			}
			//ErrorMessage.AddDebug("num3: " + num3);
			m_RaycastResults.Clear();
			//ErrorMessage.AddDebug("Canvas: " + canvas);
			//ErrorMessage.AddDebug("eventCamera: " + eventCamera);
			//ErrorMessage.AddDebug("graphicsForCanvas: " + graphicsForCanvas.Count);
			Raycast(canvas, eventCamera, vector, graphicsForCanvas, m_RaycastResults);
			int count = m_RaycastResults.Count;
			//ErrorMessage.AddDebug("m_RaycastResults: " + count);
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = m_RaycastResults[i].gameObject;
				//ErrorMessage.AddDebug("GameObject: " + gameObject);
				bool flag = true;
				if (__instance.ignoreReversedGraphics)
				{
					if (eventCamera == null)
					{
						Vector3 rhs = gameObject.transform.rotation * Vector3.forward;
						//flag = (Vector3.Dot(Vector3.forward, rhs) > 0f);
					}
					else
					{
						Vector3 b = eventCamera.transform.rotation * Vector3.forward * eventCamera.nearClipPlane;
						//flag = (Vector3.Dot(gameObject.transform.position - eventCamera.transform.position - b, gameObject.transform.forward) >= 0f);
					}
				}
				if (flag)
				{
					Transform transform = gameObject.transform;
					Vector3 forward = transform.right;
					//ErrorMessage.AddDebug("GameObject: " + gameObject.name);
					GameObject test = new GameObject("test");
					test.transform.SetParent(VRHandsController.rightController.transform);
					var line = test.AddComponent<LineRenderer>();
					line.startWidth = 0.005f;
					line.endWidth = 0.006f;
					line.SetPosition(0, VRHandsController.rightController.transform.position);
					line.SetPosition(1,  forward * FPSInputModule.current.maxInteractionDistance);
					GameObject.Destroy(line, 0.01f);
					//line.SetPosition(2, endPoint);
					float num4;
					if (eventCamera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
					{
						ErrorMessage.AddDebug("I shouldnt see this 2");
						num4 = 0f;
					}
					else
					{
						num4 = FPSInputModule.current.maxInteractionDistance;// Vector3.Dot(forward, transform.position - r.origin) / Vector3.Dot(forward, r.direction);
						if (num4 < 0f)
						{
							ErrorMessage.AddDebug("I shouldnt see this 1");
							goto IL_490;
						}
					}
					if (num4 < num3)
					{
						RaycastResult item = new RaycastResult
						{
							gameObject = gameObject,
							module = __instance,
							distance = FPSInput.fpsRaycastResult.distance,
							screenPosition = vector,
							displayIndex = targetDisplay,
							index = (float)resultAppendList.Count,
							depth = m_RaycastResults[i].depth,
							sortingLayer = canvas.sortingLayerID,
							sortingOrder = canvas.sortingOrder,
							worldPosition = r.origin + r.direction * num4,
							worldNormal = -forward
						};
						resultAppendList.Add(item);
						//ErrorMessage.AddDebug("item: " + item);
					}
				}
			IL_490:;
			}
			return false;
		}
	}*/
}