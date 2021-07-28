using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VRTweaks.Controls.UI
{

	[HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.ProcessMouseEvent))]
	public static class FPSInputModuler_UpdateCursor__Patch
	{
		[HarmonyPrefix]
		static bool Prefix(FPSInputModule __instance)
		{
			__instance.gameObject.AddComponent<FPSInput>();
			var test = __instance.GetComponent<FPSInput>();
			//ErrorMessage.AddDebug("Test: " + test);;
			if (test != null)
			{
				test.ProcessMouseEvent(__instance);
			}
			if (HandReticle.main != null)
			{
				HandReticle.main.RequestCrosshairHide();
			}
			return false;
		}
	}

	public class FPSInput : VRInputModule
	{
		public static Vector2 result;
		public static FPSInputModule fpsInput;
		public static float Distance = 0;
		private GameObject lastPress;
		public uGUI_InputGroup lastGroup { get; private set; }
		private bool skipMouseEvent;
		private bool _lockRotation;
		public static RaycastResult fpsRaycastResult;
		private GameObject[] dragHoverHandler = new GameObject[3];

		public void ProcessMouseEvent(FPSInputModule inputFPS)
		{
			VRInputModule.VRState mousePointerEventData = this.GetMousePointerEventData();
			//ErrorMessage.AddDebug("AnyPressesThisFrame: " + mousePointerEventData.);
			VRInputModule.VRButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
			VRInputModule.VRButtonEventData eventData2 = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData;
			//VRInputModule.VRButtonEventData eventData3 = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData;
			PointerEventData buttonData = eventData.buttonData;
			PointerEventData buttonData2 = eventData2.buttonData;
			//PointerEventData buttonData3 = eventData3.buttonData;
			//ErrorMessage.AddDebug("buttonData: " + buttonData.pointerCurrentRaycast.distance);
			if (buttonData != null)
			{
				RaycastResult pointerCurrentRaycast = buttonData.pointerCurrentRaycast;
				GameObject gameObject = pointerCurrentRaycast.gameObject;
				if (gameObject != null)
				{

					fpsRaycastResult = pointerCurrentRaycast;
					uGUI_InputGroup componentInParent = gameObject.GetComponentInParent<uGUI_InputGroup>();
					//ErrorMessage.AddDebug("componentInParent: " + componentInParent);
					if (componentInParent != null && componentInParent != this.lastGroup && pointerCurrentRaycast.distance > inputFPS.maxInteractionDistance)
					{
						return;
					}
					ITooltip componentInParent2 = gameObject.GetComponentInParent<ITooltip>();
					//ErrorMessage.AddDebug("componentInParent2: " + componentInParent2);
					if (componentInParent2 == null || (!componentInParent2.showTooltipOnDrag && (buttonData.dragging || buttonData2.dragging /*|| buttonData3.dragging*/)))
					{
						uGUI_Tooltip.Clear();
					}
					else
					{
						uGUI_Tooltip.Set(componentInParent2);
					}
				}
				else
				{
					uGUI_Tooltip.Clear();
				}

				this.ProcessHover(buttonData);
				this.ProcessMousePress(eventData);
				this.ProcessMove(buttonData);
				this.ProcessDrag(buttonData);
				this.ProcessDragHover(buttonData, ref this.dragHoverHandler[0]);
				this.ProcessMousePress(eventData2);
				this.ProcessDrag(buttonData2);
				this.ProcessDragHover(buttonData2, ref this.dragHoverHandler[1]);
				this.ProcessDragHover(buttonData2, ref this.dragHoverHandler[2]);
				if (!Mathf.Approximately(buttonData.scrollDelta.sqrMagnitude, 0f))
				{
					ExecuteEvents.ExecuteHierarchy<IScrollHandler>(ExecuteEvents.GetEventHandler<IScrollHandler>(pointerCurrentRaycast.gameObject), buttonData, ExecuteEvents.scrollHandler);
				}
			}
		}
		[HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.GetCursorScreenPosition))]
		public static class FPSInputModuler_GetCursorScreenPosition__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref Vector2 __result, FPSInputModule __instance)
			{
				fpsInput = __instance;
				if (VROptions.GetUseGazeBasedCursor() || !Input.mousePresent || Cursor.lockState == CursorLockMode.Locked)
				{
					result = GraphicsUtil.GetScreenSize() * 0.5f;
				}
				else
				{
					if (VRHandsController.rightController != null)
					{
						result = new Vector2(VRHandsController.rightController.transform.position.x, VRHandsController.rightController.transform.position.y);
					}
				}
				__result = result;
				return false;
			}
		}

		/*public static void GetAll(FPSInput test)
		{
			test.GetDistance();
		}

		public void GetDistance()
		{
			RaycastHit hitObject;
			if (VRHandsController.rightController != null)
			{
				Ray raycast = new Ray(VRHandsController.rightController.transform.position, VRHandsController.rightController.transform.right * fpsInput.maxInteractionDistance);
				PointerEventData _pointerEventData = new PointerEventData(EventSystem.current);
				if (Physics.Raycast(raycast, out hitObject))
				{
					//ErrorMessage.AddDebug("hitObject: " + hitObject.transform.gameObject);
					//PointerEnter(hitObject, _pointerEventData);
				}
			}
		}

		public void PointerEnter(RaycastHit obj, PointerEventData eventData)
		{
			if (obj.transform != null)
			{
				List<RaycastResult> results = new List<RaycastResult>();
				base.GetPointerData(1, out eventData, true);

				eventData.Reset();
				eventData.delta = Vector2.zero;
				eventData.position = Camera.main.WorldToScreenPoint(obj.point);
				eventData.button = PointerEventData.InputButton.Left;

				base.eventSystem.RaycastAll(eventData, results);
				//PointerInputModule.MouseState mousePointerEventData = this.GetMousePointerEventData();
				VRInputModule.VRState mousePointerEventData = this.GetMousePointerEventData();
				VRInputModule.VRButtonEventData buttData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
				VRInputModule.VRButtonEventData eventData2 = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData;
				VRInputModule.VRButtonEventData eventData3 = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData;

				PointerEventData buttonData = buttData.buttonData;
				PointerEventData buttonData2 = eventData2.buttonData;
				PointerEventData buttonData3 = eventData3.buttonData;
				RaycastResult pointerCurrentRaycast = buttonData.pointerCurrentRaycast;

				this.ProcessHover(buttonData);
				this.ProcessMousePress(buttData);
				this.ProcessMove(buttonData);
				this.ProcessDrag(buttonData);
				this.ProcessDragHover(buttonData, ref this.dragHoverHandler[0]);
				this.ProcessMousePress(eventData2);
				this.ProcessDrag(buttonData2);
				this.ProcessDragHover(buttonData2, ref this.dragHoverHandler[1]);
				this.ProcessMousePress(eventData3);
				this.ProcessDrag(buttonData3);
				this.ProcessDragHover(buttonData2, ref this.dragHoverHandler[2]);
				if (!Mathf.Approximately(buttonData.scrollDelta.sqrMagnitude, 0f))
				{
					ExecuteEvents.ExecuteHierarchy<IScrollHandler>(ExecuteEvents.GetEventHandler<IScrollHandler>(pointerCurrentRaycast.gameObject), buttonData, ExecuteEvents.scrollHandler);
				}
				foreach (var res in results)
                {
					base.HandlePointerExitAndEnter(eventData, res.gameObject);
					//ErrorMessage.AddDebug("Results: " + res.gameObject.name);
					if (res.distance <= LaserPointer.maxHitDistance)
					{
						Distance = res.distance;
					}
				}
			}
		}*/

		protected override void ProcessMove(PointerEventData pointerEvent)
		{
			GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
			base.HandlePointerExitAndEnter(pointerEvent, gameObject);
		}

		private void ProcessHover(PointerEventData pointerEvent)
		{
			GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
			if (gameObject == null)
			{
				return;
			}
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerHoverHandler>(gameObject);
			if (eventHandler == null)
			{
				return;
			}
			ExecuteEvents.Execute<IPointerHoverHandler>(eventHandler, pointerEvent, FPSInputModule.sPointerHoverHandler);
		}

		private void ProcessDragHover(PointerEventData pointerEvent, ref GameObject dragHover)
		{
			GameObject gameObject = pointerEvent.dragging ? pointerEvent.pointerCurrentRaycast.gameObject : null;
			GameObject gameObject2 = (gameObject != null) ? ExecuteEvents.GetEventHandler<IDragHoverHandler>(gameObject) : null;
			if (gameObject2 != dragHover)
			{
				if (dragHover != null)
				{
					ExecuteEvents.Execute<IDragHoverHandler>(dragHover, pointerEvent, FPSInputModule.sDragHoverExitHandler);
				}
				dragHover = gameObject2;
				if (dragHover != null)
				{
					ExecuteEvents.Execute<IDragHoverHandler>(dragHover, pointerEvent, FPSInputModule.sDragHoverEnterHandler);
				}
			}
			if (dragHover != null)
			{
				ExecuteEvents.Execute<IDragHoverHandler>(dragHover, pointerEvent, FPSInputModule.sDragHoverStayHandler);
			}
		}

		private void ProcessMousePress(VRInputModule.VRButtonEventData data)
		{
			PointerEventData buttonData = data.buttonData;
			GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
			//ErrorMessage.AddDebug("Press GameObject: " + gameObject);
			if (data.PressedThisFrame())
			{
				buttonData.eligibleForClick = true;
				buttonData.delta = Vector3.zero;
				buttonData.dragging = false;
				buttonData.useDragThreshold = true;
				buttonData.pressPosition = buttonData.position;
				buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
				base.DeselectIfSelectionChanged(gameObject, buttonData);
				GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
				if (gameObject2 == null)
				{
					gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				}
				if (gameObject2 == null && gameObject != null)
				{
					ScrollRect componentInParent = gameObject.GetComponentInParent<ScrollRect>();
					if (componentInParent != null && componentInParent.content != null)
					{
						RectTransform content = componentInParent.content;
						if (gameObject.GetComponent<Transform>().IsChildOf(content))
						{
							gameObject2 = content.gameObject;
						}
					}
					if (gameObject2 == null && gameObject.GetComponentInParent<uGUI_InputGroup>() != null)
					{
						gameObject2 = gameObject;
					}
				}
				if (gameObject2 == null)
				{
					this.ChangeGroup(null, false);
				}
				else if (this.lastPress == null || gameObject2 != this.lastPress)
				{
					uGUI_InputGroup newGroup;
					if (gameObject2.GetComponentInParent<uGUI_QuickSlots>() != null)
					{
						newGroup = uGUI_PDA.main;
					}
					else if (gameObject2.GetComponentInParent<uGUI_PinnedRecipes>() != null)
					{
						newGroup = this.lastGroup;
					}
					else
					{
						newGroup = gameObject2.GetComponentInParent<uGUI_InputGroup>();
					}
					this.ChangeGroup(newGroup, false);
				}
				this.lastPress = gameObject2;
				float unscaledTime = Time.unscaledTime;
				if (gameObject2 == buttonData.lastPress)
				{
					if (unscaledTime - buttonData.clickTime < 0.3f)
					{
						PointerEventData pointerEventData = buttonData;
						int clickCount = pointerEventData.clickCount + 1;
						pointerEventData.clickCount = clickCount;
					}
					else
					{
						buttonData.clickCount = 1;
					}
					buttonData.clickTime = unscaledTime;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.pointerPress = gameObject2;
				buttonData.rawPointerPress = gameObject;
				buttonData.clickTime = unscaledTime;
				buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
				if (buttonData.pointerDrag != null)
				{
					ExecuteEvents.Execute<IInitializePotentialDragHandler>(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
				}
			}
			if (data.ReleasedThisFrame())
			{
				//this.lastRaycastResult = default(RaycastResult);
				ExecuteEvents.Execute<IPointerUpHandler>(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
				{
					ExecuteEvents.Execute<IPointerClickHandler>(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
				}
				else if (buttonData.pointerDrag != null)
				{
					ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, buttonData, ExecuteEvents.dropHandler);
				}
				buttonData.eligibleForClick = false;
				buttonData.pointerPress = null;
				buttonData.rawPointerPress = null;
				if (buttonData.pointerDrag != null && buttonData.dragging)
				{
					ExecuteEvents.Execute<IEndDragHandler>(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
				}
				buttonData.dragging = false;
				buttonData.pointerDrag = null;
				if (gameObject != buttonData.pointerEnter)
				{
					base.HandlePointerExitAndEnter(buttonData, null);
					base.HandlePointerExitAndEnter(buttonData, gameObject);
				}
			}
		}

		public bool lockRotation
		{
			get
			{
				return this._lockRotation;
			}
			private set
			{
				if (this._lockRotation != value)
				{
					this._lockRotation = value;
					if (MainCameraControl.main != null)
					{
						MainCameraControl.main.SetEnabled(!this._lockRotation);
					}
				}
			}
		}

		protected void ChangeGroup(uGUI_InputGroup newGroup, bool lockMovement)
		{
			if (this.lastGroup == null)
			{
				if (newGroup != null)
				{
					GameInput.ClearInput();
					newGroup.OnSelect(lockMovement);
					this.lastGroup = newGroup;
					this.lockRotation = true;
					this.skipMouseEvent = true;
				}
			}
			else if (newGroup == null)
			{
				GameInput.ClearInput();
				this.lastGroup.OnDeselect();
				this.lastGroup = null;
				this.lastPress = null;
				this.lockRotation = false;
				this.skipMouseEvent = true;
			}
			else if (this.lastGroup != newGroup)
			{
				GameInput.ClearInput();
				this.lastGroup.OnDeselect();
				newGroup.OnSelect(lockMovement);
				this.lastGroup = newGroup;
				this.skipMouseEvent = true;
			}
			else
			{
				this.lastGroup.OnReselect(lockMovement);
			}
			GamepadInputModule gamepadInputModule = GamepadInputModule.current;
			if (gamepadInputModule != null)
			{
				gamepadInputModule.OnGroupChanged(newGroup);
			}
		}
		public override void Process()
        {
			ErrorMessage.AddDebug("Process");
        }
    }
}
