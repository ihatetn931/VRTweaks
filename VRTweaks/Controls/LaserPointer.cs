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
        public AxisType facingAxis = AxisType.XAxis;
        public float length = 2.9f;
        public bool showCursor = true;
        GameObject holder;
        GameObject pointer;
        public GameObject cursor;
        public GameObject UIcursor;
        public uGUI_InputGroup lastGroup { get; private set; }
        public static RaycastHit hitObject;
        Vector3 cursorScale = new Vector3(0.02f, 0.02f, 0.02f);
        float contactDistance = 0f;
        Transform contactTarget = null;
        public static Color colorRed = new Color(1, 0, 0, 1);
        public static Color colorCyan = new Color(0, 1, 1, 0.5f);
        public static Color colorBlue = new Color(0, 0, 1, 0.5f);

        void SetPointerTransform(float setLength, float setThicknes, Vector3 hitPoint)
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
                    if (FPSInput.fpsRaycastResult.gameObject != null)
                    {
                        UIcursor.transform.localPosition = FPSInput.fpsRaycastResult.worldPosition;
                        UIcursor.transform.position = FPSInput.fpsRaycastResult.worldPosition;
                        UIcursor.GetComponent<MeshRenderer>().material.color = Color.green;
                        UIcursor.layer = FPSInput.fpsRaycastResult.gameObject.GetComponent<Image>().gameObject.layer - 1;
                        //ErrorMessage.AddDebug("ImageLayer: " + FPSInput.fpsRaycastResult.gameObject.GetComponent<Image>().gameObject.layer);
                        //ErrorMessage.AddDebug("RectTransformLayer: " + FPSInput.fpsRaycastResult.gameObject.GetComponent<RectTransform>().gameObject.layer);
                        //ErrorMessage.AddDebug("UIcursorLayer: " + UIcursor.layer);
                    }
                    else
                    {
                        UIcursor.GetComponent<MeshRenderer>().material.color = Color.black;
                    }
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
            Material newMaterial = new Material(Shader.Find("Sprites/Default"));
            newMaterial.SetColor(ShaderPropertyID._Color, colorCyan);

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

                UIcursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                UIcursor.SetActive(true);
                UIcursor.transform.parent = holder.transform;
                UIcursor.GetComponent<MeshRenderer>().material = newMaterial;
                UIcursor.transform.localScale = cursorScale;

                UIcursor.GetComponent<SphereCollider>().isTrigger = true;
                UIcursor.AddComponent<Rigidbody>().isKinematic = true;

                
            }

            SetPointerTransform(length, thickness, Vector3.right);
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
                    cursor.GetComponent<MeshRenderer>().material.color = Color.red;
                }
                cursor.GetComponent<MeshRenderer>().material.color = Color.blue;
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
            bool rayHit = Physics.Raycast(raycast, out hitObject);
            if (rayHit)
            {
                float beamLength = GetBeamLength(rayHit, hitObject);
                SetPointerTransform(beamLength, thickness, hitObject.point);
            }
        }
    }
}
