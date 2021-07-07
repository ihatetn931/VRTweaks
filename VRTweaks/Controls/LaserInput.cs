
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.XR;

namespace VRTweaks.Controls
{
  /*  public class LaserInput : MonoBehaviour
    {
        /*    public static RaycastHit DetectHit(Vector3 startPos, float distance, Vector3 direction)
            {
                //init ray to save the start and direction values
                Ray ray = new Ray(startPos, direction);
                //varible to hold the detection info
                RaycastHit hit;
                //the end Pos which defaults to the startPos + distance 
                Vector3 endPos = startPos + (distance * direction);
                if (Physics.Raycast(ray, out hit, distance))
                {
                    //if we detect something
                    ErrorMessage.AddDebug("RayCastHit: " + hit.collider.gameObject.name);
                    endPos = hit.point;
                }
                //lineRenderer.SetPosition(0, startPos);
                //lineRenderer.SetPosition(1, endPos);
                DrawLine(startPos, endPos, Color.red);
                return hit;
            }

            public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.0f)
            {
                GameObject myLine = new GameObject();
                myLine.transform.position = start;
                myLine.AddComponent<LineRenderer>();
                LineRenderer lr = myLine.GetComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
                lr.startColor = color;// (color, color);
                lr.startWidth = 0.5f;
                lr.endWidth = 0.1f;
                lr.SetPosition(0, start);
                lr.SetPosition(1, end);
                ErrorMessage.AddDebug("LineRender: " + lr);
                ErrorMessage.AddDebug("myLine: " + myLine);
                //GameObject.Destroy(myLine, duration);
            }
        void FixedUpdate()
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.forward, out hit, 40.0f))
            {
                ErrorMessage.AddDebug("Object: " + hit.collider.gameObject.name);
                ErrorMessage.AddDebug("LocalPosition: " + transform.position);
                ErrorMessage.AddDebug("Distance: " + hit.distance);
                Color color = new Color(1, 0, 0, 0.5f);
                //Debug.DrawRay(transform.position,transform.forward / hit.distance, Color.red);
                DrawLine(transform.position, transform.forward * hit.distance, color,0.1f);
            }
        }
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.1f)
        {
            GameObject myLine = new GameObject();
            myLine.transform.position = start;
            //myLine.transform.rotation = rotation;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Transparent/Diffuse")); //color;//  = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.material.color = color;
            lr.startColor = color;// (color, color);
            lr.startWidth = 0.5f;
            lr.endWidth = 0.1f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            ErrorMessage.AddDebug("LineRender: " + lr);
            ErrorMessage.AddDebug("myLine: " + myLine);
            GameObject.Destroy(myLine,duration);
        }

        /*public static void FixedUpdate()
        {
            // Bit shift the index of the layer (8) to get a bit mask
            int layerMask = 1 << 8;

            // This would cast rays only against colliders in layer 8.
            // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
            layerMask = ~layerMask;

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(VRHandsController.rightController.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
            {
                Debug.DrawRay(VRHandsController.rightController.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                Debug.Log("Did Hit");
            }
            else
            {
                Debug.DrawRay(VRHandsController.rightController.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                Debug.Log("Did not Hit");
            }
        }
    }*/
}
