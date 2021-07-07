using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using HarmonyLib;
using UnityEngine.Events;
using System;

namespace VRTweaks.Controls
{
    public class MotionCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum AxisType
        {
            XAxis,
            ZAxis
        }

        public Color color;
        public static float thickness = 0.005f;
        public AxisType facingAxis = AxisType.XAxis;
        public float length = 2.9f;
        public bool showCursor = true;
        GameObject holder;
        GameObject pointer;
        public static GameObject cursor;

        Vector3 cursorScale = new Vector3(0.05f, 0.05f, 0.05f);
        float contactDistance = 0f;
        Transform contactTarget = null;

        void SetPointerTransform(float setLength, float setThicknes)
        {
            //if the additional decimal isn't added then the beam position glitches
            float beamPosition = setLength / (2 + 0.00001f);

            if (facingAxis == AxisType.XAxis)
            {
                pointer.transform.localScale = new Vector3(setLength, setThicknes, setThicknes);
                pointer.transform.localPosition = new Vector3(beamPosition, 0f, 0f);
                if (showCursor)
                {
                    cursor.transform.localPosition = new Vector3(setLength - cursor.transform.localScale.x, 0f, 0f);
                }
            }
            else
            {
                pointer.transform.localScale = new Vector3(setThicknes, setThicknes, setLength);
                pointer.transform.localPosition = new Vector3(0f, 0f, beamPosition);

                if (showCursor)
                {
                    cursor.transform.localPosition = new Vector3(0f, 0f, setLength - cursor.transform.localScale.z);
                }
            }
        }

        // Use this for initialization
        void Start()
        {

            Color colorRed = new Color(1, 0, 0, 1f);
            Material newMaterial = new Material(Shader.Find("Transparent/Diffuse"));
            newMaterial.SetColor(ShaderPropertyID._Color, colorRed);

            holder = new GameObject();
            holder.transform.parent = this.transform;
            holder.transform.localPosition = Vector3.zero;

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.transform.parent = holder.transform;
            pointer.GetComponent<MeshRenderer>().material = newMaterial;

            pointer.GetComponent<BoxCollider>().isTrigger = true;
            pointer.AddComponent<Rigidbody>().isKinematic = true;
            pointer.layer = 2;

            if (showCursor)
            {
                cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cursor.SetActive(true);
                cursor.transform.parent = holder.transform;
                cursor.GetComponent<MeshRenderer>().material = newMaterial;
                cursor.transform.localScale = cursorScale;

                cursor.GetComponent<SphereCollider>().isTrigger = true;
                cursor.AddComponent<Rigidbody>().isKinematic = true;

                cursor.layer = 2;
            }

            SetPointerTransform(length, thickness);
        }

        float GetBeamLength(bool bHit, RaycastHit hit)
        {
            float actualLength = length;

            //reset if beam not hitting or hitting new target
            if (!bHit || (contactTarget && contactTarget != hit.transform))
            {
                contactDistance = 0f;
                contactTarget = null;
            }

            //check if beam has hit a new target
            if (bHit)
            {
                if (hit.distance <= 0)
                {
                }
                contactDistance = hit.distance;
                contactTarget = hit.transform;
            }

            //adjust beam length if something is blocking it
            if (bHit && contactDistance < length)
            {
                actualLength = contactDistance;
            }

            if (actualLength <= 0)
            {
                actualLength = length;
            }

            return actualLength;
        }

        void FixedUpdate()
        {

            Ray raycast = new Ray(transform.position, transform.right);
            RaycastHit hitObject;
            bool rayHit = Physics.Raycast(raycast, out hitObject);


            var pointerEnterHandler = hitObject.collider.GetComponentInChildren<IPointerEnterHandler>();
            var pointerHoverHandler = hitObject.collider.GetComponentInChildren<IPointerHoverHandler>();
            var pointerClickHandler = hitObject.collider.GetComponentInChildren<IPointerClickHandler>();
            var pointerDownHandler = hitObject.collider.GetComponentInChildren<IPointerDownHandler>();
            var pointerExitHandler = hitObject.collider.GetComponentInChildren<IPointerExitHandler>();
            var pointerUpHandler = hitObject.collider.GetComponentInChildren<IPointerUpHandler>();
            ErrorMessage.AddDebug("pointerEnterHandler: " + pointerEnterHandler);
            ErrorMessage.AddDebug("pointerEnterHandler: " + pointerHoverHandler);
            ErrorMessage.AddDebug("pointerEnterHandler: " + pointerClickHandler);
            ErrorMessage.AddDebug("pointerEnterHandler: " + pointerDownHandler);
            ErrorMessage.AddDebug("pointerEnterHandler: " + pointerExitHandler);
            ErrorMessage.AddDebug("pointerEnterHandler: " + pointerUpHandler);


            if (rayHit)
            {
                float beamLength = GetBeamLength(rayHit, hitObject);
                SetPointerTransform(beamLength, thickness);
               
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(eventData != null)
            ErrorMessage.AddDebug("OnPointerEnter: " + eventData.pointerCurrentRaycast);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData != null)
                ErrorMessage.AddDebug("OnPointerExit: " + eventData.pointerCurrentRaycast);
        }
    }
}