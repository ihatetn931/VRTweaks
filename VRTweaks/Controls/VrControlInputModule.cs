using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VRTweaks.Controls
{
	public abstract class VRInputModule : BaseInputModule
	{
		protected bool GetPointerData(int id, out PointerEventData data, bool create)
		{
			if (!this.m_PointerData.TryGetValue(id, out data) && create)
			{
				data = new PointerEventData(base.eventSystem)
				{
					pointerId = id
				};
				this.m_PointerData.Add(id, data);
				return true;
			}
			return false;
		}

		protected void RemovePointerData(PointerEventData data)
		{
			this.m_PointerData.Remove(data.pointerId);
		}

		protected void CopyFromTo(PointerEventData from, PointerEventData to)
		{
			to.position = from.position;
			to.delta = from.delta;
			to.scrollDelta = from.scrollDelta;
			to.pointerCurrentRaycast = from.pointerCurrentRaycast;
			to.pointerEnter = from.pointerEnter;
		}

		protected PointerEventData.FramePressState StateForMouseButton(GameInput.Button buttonId)
		{
			bool mouseButtonDown = GameInput.GetButtonDown(buttonId);
			bool mouseButtonUp = GameInput.GetButtonUp(buttonId);
			if (mouseButtonDown && mouseButtonUp)
			{
				return PointerEventData.FramePressState.PressedAndReleased;
			}
			if (mouseButtonDown)
			{
				return PointerEventData.FramePressState.Pressed;
			}
			if (mouseButtonUp)
			{
				return PointerEventData.FramePressState.Released;
			}
			return PointerEventData.FramePressState.NotChanged;
		}

		protected virtual VRInputModule.VRState GetMousePointerEventData()
		{
			return this.GetMousePointerEventData(0);
		}

		protected virtual VRInputModule.VRState GetMousePointerEventData(int id)
		{
			PointerEventData pointerEventData;
			bool pointerData = this.GetPointerData(-1, out pointerEventData, true);
			pointerEventData.Reset();
			if (pointerData)
			{
				if (VRHandsController.rightController != null && Camera.main != null)
					pointerEventData.position = Camera.main.WorldToScreenPoint(VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.right * FPSInputModule.current.maxInteractionDistance);//Camera.main.WorldToViewportPoint(VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.right * FPSInputModule.current.maxInteractionDistance);
			}
			if (VRHandsController.rightController != null && Camera.main != null)
			{
				Vector2 mousePosition = Camera.main.WorldToScreenPoint(VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.right * FPSInputModule.current.maxInteractionDistance);//Camera.main.WorldToViewportPoint(VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.right * FPSInputModule.current.maxInteractionDistance);
				//ErrorMessage.AddDebug("LockState: " + Cursor.lockState);
				//if (Cursor.lockState == CursorLockMode.Locked)
				//{
				//	Cursor.lockState = CursorLockMode.None;
				//	pointerEventData.position = new Vector2(-1f, -1f);
				//	pointerEventData.delta = Vector2.zero;
			//	}
				//else
				//{
				pointerEventData.delta = mousePosition - pointerEventData.position;
				pointerEventData.position = mousePosition;
				//}
				//pointerEventData.scrollDelta = base.input.mouseScrollDelta;
				pointerEventData.button = PointerEventData.InputButton.Left;
				base.eventSystem.RaycastAll(pointerEventData, this.m_RaycastResultCache);
				RaycastResult pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(this.m_RaycastResultCache);
				pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
				//ErrorMessage.AddDebug("pointerCurrentRaycastGO: " + pointerCurrentRaycast.gameObject);
				this.m_RaycastResultCache.Clear();
				PointerEventData pointerEventData2;
				this.GetPointerData(-2, out pointerEventData2, true);
				this.CopyFromTo(pointerEventData, pointerEventData2);
				pointerEventData2.button = PointerEventData.InputButton.Right;
				PointerEventData pointerEventData3;
				this.GetPointerData(-3, out pointerEventData3, true);
				this.CopyFromTo(pointerEventData, pointerEventData3);
				pointerEventData3.button = PointerEventData.InputButton.Left;
				this.m_MouseState.SetButtonState(PointerEventData.InputButton.Left, this.StateForMouseButton(GameInput.Button.LeftHand), pointerEventData);
				this.m_MouseState.SetButtonState(PointerEventData.InputButton.Right, this.StateForMouseButton(GameInput.Button.RightHand), pointerEventData2);
				this.m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, this.StateForMouseButton(GameInput.Button.PDA), pointerEventData3);
			}
			return this.m_MouseState;
		}

		protected PointerEventData GetLastPointerEventData(int id)
		{
			PointerEventData result;
			this.GetPointerData(id, out result, false);
			return result;
		}

		private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
		{
			return !useDragThreshold || (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
		}

		protected virtual void ProcessMove(PointerEventData pointerEvent)
		{
			GameObject newEnterTarget = (Cursor.lockState == CursorLockMode.Locked) ? null : pointerEvent.pointerCurrentRaycast.gameObject;
			base.HandlePointerExitAndEnter(pointerEvent, newEnterTarget);
		}

		protected virtual void ProcessDrag(PointerEventData pointerEvent)
		{
			if (!pointerEvent.IsPointerMoving() || Cursor.lockState == CursorLockMode.Locked || pointerEvent.pointerDrag == null)
			{
				return;
			}
			if (!pointerEvent.dragging && VRInputModule.ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, (float)base.eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
			{
				ExecuteEvents.Execute<IBeginDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
				pointerEvent.dragging = true;
			}
			if (pointerEvent.dragging)
			{
				if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
				{
					ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
					pointerEvent.eligibleForClick = false;
					pointerEvent.pointerPress = null;
					pointerEvent.rawPointerPress = null;
				}
				ExecuteEvents.Execute<IDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
			}
		}

		public override bool IsPointerOverGameObject(int pointerId)
		{
			PointerEventData lastPointerEventData = this.GetLastPointerEventData(pointerId);
			return lastPointerEventData != null && lastPointerEventData.pointerEnter != null;
		}

		protected void ClearSelection()
		{
			BaseEventData baseEventData = this.GetBaseEventData();
			foreach (PointerEventData currentPointerData in this.m_PointerData.Values)
			{
				base.HandlePointerExitAndEnter(currentPointerData, null);
			}
			this.m_PointerData.Clear();
			base.eventSystem.SetSelectedGameObject(null, baseEventData);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("<b>Pointer Input Module of type: </b>" + base.GetType());
			stringBuilder.AppendLine();
			foreach (KeyValuePair<int, PointerEventData> keyValuePair in this.m_PointerData)
			{
				if (keyValuePair.Value != null)
				{
					stringBuilder.AppendLine("<B>Pointer:</b> " + keyValuePair.Key);
					stringBuilder.AppendLine(keyValuePair.Value.ToString());
				}
			}
			return stringBuilder.ToString();
		}

		public void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
		{
			if (ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo) != base.eventSystem.currentSelectedGameObject)
			{
				base.eventSystem.SetSelectedGameObject(null, pointerEvent);
			}
		}

		public const int kMouseLeftId = -1;

		public const int kMouseRightId = -2;

		public const int kMouseMiddleId = -3;

		public const int kFakeTouchesId = -4;

		protected Dictionary<int, PointerEventData> m_PointerData = new Dictionary<int, PointerEventData>();

		private readonly VRInputModule.VRState m_MouseState = new VRInputModule.VRState();

		protected class ButtonState
		{
			public VRInputModule.VRButtonEventData eventData
			{
				get
				{
					return this.m_EventData;
				}
				set
				{
					this.m_EventData = value;
				}
			}


			public PointerEventData.InputButton button
			{
				get
				{
					return this.m_Button;
				}
				set
				{
					this.m_Button = value;
				}
			}


			private PointerEventData.InputButton m_Button;

			private VRInputModule.VRButtonEventData m_EventData;
		}


		protected class VRState
		{
			public bool AnyPressesThisFrame()
			{
				for (int i = 0; i < this.m_TrackedButtons.Count; i++)
				{
					if (this.m_TrackedButtons[i].eventData.PressedThisFrame())
					{
						return true;
					}
				}
				return false;
			}

			public bool AnyReleasesThisFrame()
			{
				for (int i = 0; i < this.m_TrackedButtons.Count; i++)
				{
					if (this.m_TrackedButtons[i].eventData.ReleasedThisFrame())
					{
						return true;
					}
				}
				return false;
			}

			public VRInputModule.ButtonState GetButtonState(PointerEventData.InputButton button)
			{
				VRInputModule.ButtonState buttonState = null;
				for (int i = 0; i < this.m_TrackedButtons.Count; i++)
				{
					if (this.m_TrackedButtons[i].button == button)
					{
						buttonState = this.m_TrackedButtons[i];
						break;
					}
				}
				if (buttonState == null)
				{
					buttonState = new VRInputModule.ButtonState
					{
						button = button,
						eventData = new VRInputModule.VRButtonEventData()
					};
					this.m_TrackedButtons.Add(buttonState);
				}
				return buttonState;
			}

			public void SetButtonState(PointerEventData.InputButton button, PointerEventData.FramePressState stateForMouseButton, PointerEventData data)
			{
				VRInputModule.ButtonState buttonState = this.GetButtonState(button);
				buttonState.eventData.buttonState = stateForMouseButton;
				buttonState.eventData.buttonData = data;
			}

			private List<VRInputModule.ButtonState> m_TrackedButtons = new List<VRInputModule.ButtonState>();
		}

		public class VRButtonEventData
		{
			public bool PressedThisFrame()
			{
				return this.buttonState == PointerEventData.FramePressState.Pressed || this.buttonState == PointerEventData.FramePressState.PressedAndReleased;
			}

			public bool ReleasedThisFrame()
			{
				return this.buttonState == PointerEventData.FramePressState.Released || this.buttonState == PointerEventData.FramePressState.PressedAndReleased;
			}

			public PointerEventData.FramePressState buttonState;

			public PointerEventData buttonData;
		}
	}
}
