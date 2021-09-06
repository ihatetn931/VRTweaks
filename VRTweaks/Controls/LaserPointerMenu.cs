using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRTweaks.Controls.UI;

namespace VRTweaks.Controls
{
    public class LaserPointerMenu : MonoBehaviour
    {
        public Color color;
        public static float thickness = 0.005f;
        public LineRenderer line;
        public static Color colorCyan = new Color(0, 1, 1, 1f);
        public static Color colorBlue = new Color(0, 0, 1, 1f);
        public static Color colorGreen = new Color(0, 1, 0, 1f);
        public static Color colorHIt = new Color(0, 0.475f, 1, 1);

        void SetPointerTransform()
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
                        var screenPoint = Camera.main.ScreenPointToRay(FPSInputModule.current.lastRaycastResult.screenPosition).GetPoint(FPSInputModule.current.lastRaycastResult.distance);
                        line.SetPosition(1, Vector3.MoveTowards(transform.position,screenPoint, FPSInputModule.current.maxInteractionDistance));
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
            Debug.Log("Transform: " + this.transform);

            if ( line == null)
                line = this.gameObject.AddComponent<LineRenderer>();
            line = this.gameObject.GetComponent<LineRenderer>();
            Debug.Log("Line: " + line);

            line.startColor = colorCyan;
            line.endColor = colorBlue;
            line.material = newMaterial;
            line.startWidth = 0.002f;
            line.endWidth = 0.003f;
        }

        void Update()
        {
            line.SetPosition(0, this.transform.position);
            SetPointerTransform();
        }
    }
}
