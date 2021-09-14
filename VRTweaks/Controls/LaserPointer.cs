using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRTweaks.Controls.UI;

namespace VRTweaks.Controls
{
    public class LaserPointer : MonoBehaviour
    {
        public enum AxisType
        {
            XAxis,
            ZAxis
        }

        public Color color;
        public static float thickness = 0.005f;
        public float length = FPSInputModule.current.maxInteractionDistance;
        GameObject holder;
        public LineRenderer line;
        public static RaycastHit hitObject;
        float contactDistance = 0f;
        Transform contactTarget = null;
        public static Color colorRed = new Color(1, 0, 0, 1f);
        public static Color colorCyan = new Color(0, 1, 1, 1f);
        public static Color colorBlue = new Color(0, 0, 1, 1f);
        public static Color colorGreen = new Color(0, 1, 0, 1f);
        public static Color colorHIt = new Color(0, 0.475f, 1, 1);

        void SetPointerTransform(float setLength, float setThicknes, RaycastHit hitPoint)
        {
            if (FPSInputModule.current.lastRaycastResult.gameObject != null)
            {
                line.sortingOrder = FPSInputModule.current.lastRaycastResult.sortingOrder;
                line.sortingLayerID = FPSInputModule.current.lastRaycastResult.sortingLayer;
                if (FPSInputModule.current.lastRaycastResult.isValid)
                {
                    Camera eventCamera = FPSInputModule.current.lastRaycastResult.module.eventCamera;
                    if (Camera.main != null)
                    {
                        if (eventCamera != null)
                        {
                            line.endColor = colorHIt;
                            line.SetPosition(1, Vector3.MoveTowards(transform.position, Camera.main.ScreenPointToRay(FPSInputModule.current.lastRaycastResult.screenPosition).GetPoint(FPSInputModule.current.lastRaycastResult.distance), FPSInputModule.current.maxInteractionDistance));
                            FPSInputModule.current.lastRaycastResult.Clear();
                        }
                        else
                        {
                            line.endColor = colorGreen;
                            line.SetPosition(1, Vector3.MoveTowards(transform.position, FPSInputModule.current.lastRaycastResult.worldPosition, FPSInputModule.current.maxInteractionDistance));
                            FPSInputModule.current.lastRaycastResult.Clear();
                        }
                    }
                }
            }
            else
            {
                line.endColor = colorBlue;
                line.SetPosition(1, Vector3.MoveTowards(transform.position, hitPoint.point, FPSInputModule.current.maxInteractionDistance));
                FPSInputModule.current.lastRaycastResult.Clear();
            }
        }

        void SetPointerTransform1()
        {
            if (FPSInputModule.current.lastRaycastResult.gameObject != null)
            {
                line.sortingOrder = FPSInputModule.current.lastRaycastResult.sortingOrder;
                line.sortingLayerID = FPSInputModule.current.lastRaycastResult.sortingLayer;
                if (FPSInputModule.current.lastRaycastResult.isValid)
                {
                    Camera eventCamera = FPSInputModule.current.lastRaycastResult.module.eventCamera;
                    if (eventCamera != null)
                    {
                        line.endColor = colorHIt;
                        line.SetPosition(1, Vector3.MoveTowards(transform.position, Camera.main.ScreenPointToRay(FPSInputModule.current.lastRaycastResult.screenPosition).GetPoint(FPSInputModule.current.lastRaycastResult.distance), FPSInputModule.current.maxInteractionDistance));
                        FPSInputModule.current.lastRaycastResult.Clear();
                    }
                    else
                    {
                        line.endColor = colorGreen;
                        line.SetPosition(1, Vector3.MoveTowards(transform.position, FPSInputModule.current.lastRaycastResult.worldPosition, FPSInputModule.current.maxInteractionDistance));
                        FPSInputModule.current.lastRaycastResult.Clear();
                    }
                }
            }
        }

        void Start()
        {
            Material newMaterial = new Material(Shader.Find("Sprites/Default"));
            // newMaterial.SetColor(ShaderPropertyID._Color, colorCyan);

            holder = new GameObject();
            holder.transform.parent = this.transform;
            holder.transform.localPosition = Vector3.zero;

            if (line == null)
                line = holder.transform.gameObject.AddComponent<LineRenderer>();
            line = holder.transform.gameObject.GetComponent<LineRenderer>();
            line.startColor = colorCyan;
            line.endColor = colorBlue;
            line.material = newMaterial;
            line.startWidth = 0.004f;
            line.endWidth = 0.005f;
            if (Player.main != null)
            {
                if (line.gameObject.GetComponent<BoxCollider>() == null)
                    line.gameObject.AddComponent<BoxCollider>();
                line.gameObject.GetComponent<BoxCollider>().isTrigger = true;

                if (line.gameObject.GetComponent<Rigidbody>() == null)
                    line.gameObject.AddComponent<Rigidbody>();
                line.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            }

            Ray ray = new Ray(transform.position, transform.forward);
            Physics.Raycast(ray, out RaycastHit result);
            SetPointerTransform(length, thickness, result);
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

        void Update()
        {
            Vector3 aim = new Vector3(-1, 0, 0);
            Ray raycast = new Ray(transform.position, transform.forward);
            bool rayHit = Physics.Raycast(raycast, out hitObject, LayerID.TerrainCollider);
            line.SetPosition(0, transform.position);
            if (rayHit)
            {
                float beamLength = GetBeamLength(rayHit, hitObject);
                //line.SetPosition(1, Vector3.MoveTowards(transform.position, hitObject.point, FPSInputModule.current.maxInteractionDistance));
                SetPointerTransform(beamLength, thickness, hitObject);
            }
            else
            {
                SetPointerTransform1();
            }
        }
    }
}