//https://www.nexusmods.com/subnautica/mods/173
using UnityEngine;
using UnityEngine.UI;

namespace VREnhancementsBZ
{
    class VehicleHUDManager : MonoBehaviour
    {
        public static GameObject vehicleCanvas;
        static uGUI_CanvasScaler canvasScaler;
        static Transform barsPanel;
        static Transform quickSlots;
        static Transform pinnedReciepes;
        static Transform seatruckSegHealth;
        static Transform ErrorMessages;

        public Transform seatruckHUD;
        public Transform exosuitHUD;
        public Transform hoverbikeHUD;
        static Transform compass;
        static Transform HUDContent;


        Vector3 seatruckHUDPos = new Vector3(1000, 50, 1000);
        Vector3 seatruckCompassPos = new Vector3(0, 650, 950);
        Vector3 seatruckQuickSlotsPos = new Vector3(0, -800, 1100);
        Vector3 seatruckBarsPanelPos = new Vector3(-1200, -350, 1000);
        Vector3 seatruckPinnedReciepes = new Vector3(1200, 600, 1000);
        Vector3 seatruckSegmentHealthPos = new Vector3(0, 750, 950);
        Vector3 seatruckErrorMessagePos = new Vector3(0, -700, 1100);

        Vector3 hoverbikeHUDPos = new Vector3(850, -0, 1000);
        Vector3 hoverbikeCompassPos = new Vector3(0, 650, 950);
        Vector3 hoverbikeQuickSlotsPos = new Vector3(0, -400, 1100);
        Vector3 hoverbikeBarsPanelPos = new Vector3(-1300, -300, 1000);
        Vector3 hoverbikePinnedReciepes = new Vector3(1000, 500, 1000);
        Vector3 hoverbikeErrorMessagePos = new Vector3(0, -300, 1000);

        Vector3 exosuitHUDPos = new Vector3(700, -200, 600);
        Vector3 exosuitCompassPos = new Vector3(0, 400, 700);
        Vector3 exosuitQuickSlotsPos = new Vector3(0, -700, 700);
        Vector3 exosuitBarsPanelPos = new Vector3(-900, -400, 600);
        Vector3 exosuitPinnedReciepes = new Vector3(850, 350, 1000);
        Vector3 exosuitErrorMessagePos = new Vector3(0, -800, 1000);

        Vector3 originalCompassPos;
        Vector3 originalQuickSlotsPos;
        Vector3 originalBarsPanelPos;
        Vector3 originalPinnedReciepes;
        Vector3 orginalErrorMessagePos;
        Quaternion orginalErrorMessageRot;


        void Awake()
        {
            //create a new worldspace canvas for vehicles
            vehicleCanvas = new GameObject("VRVehicleCanvas");
            DontDestroyOnLoad(vehicleCanvas);
            Canvas canvas = vehicleCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = "HUD";
            vehicleCanvas.AddComponent<CanvasGroup>();
            canvasScaler = vehicleCanvas.AddComponent<uGUI_CanvasScaler>();
            //the canvasScaler moves elements in front of the UI camera based on the mode.
            //In inversed mode it moves the elements so that they look like they are attached to the canvasScaler anchor from the main camera's perspective.
            canvasScaler.vrMode = uGUI_CanvasScaler.Mode.Inversed;
            //TODO:Not that important but figure out how to get raycasts working on the new canvas so drag and drop will work on the quickslots while PDA is open in the vehicle
            //vehicleCanvas.AddComponent<uGUI_GraphicRaycaster>();
            vehicleCanvas.layer = LayerMask.NameToLayer("UI");
            vehicleCanvas.transform.localScale = Vector3.one * 0.001f;

            HUDContent = GameObject.Find("HUD/Content").transform;

            seatruckHUD = HUDContent.Find("Seatruck").transform;
            exosuitHUD = HUDContent.Find("Exosuit").transform;
            hoverbikeHUD = HUDContent.Find("Hoverbike").transform;

            quickSlots = HUDContent.Find("QuickSlots").transform;
            compass = HUDContent.Find("DepthCompass").transform;
            barsPanel = HUDContent.Find("BarsPanel").transform;
            pinnedReciepes = HUDContent.Find("PinnedRecipes").transform;
            ErrorMessages = HUDContent.Find("HandReticle/UseCanvas").transform;
            seatruckSegHealth = HUDContent.Find("Seatruck/Segments").transform;

            seatruckHUD.SetParent(vehicleCanvas.transform, false);//move the vehicle specific HUD elements to the new vehicle Canvas
            seatruckHUD.localPosition = seatruckHUDPos;

            exosuitHUD.SetParent(vehicleCanvas.transform, false);
            exosuitHUD.localPosition = exosuitHUDPos;

            hoverbikeHUD.SetParent(vehicleCanvas.transform, false);
            hoverbikeHUD.localPosition = hoverbikeHUDPos;

            vehicleCanvas.SetActive(false);

        }

        void Update()
        {
            Player player = Player.main;
            if (player)
            {
                if (player.inSeatruckPilotingChair || player.inExosuit || player.inHovercraft)
                {
                    if (!vehicleCanvas.activeInHierarchy)
                    {
                        //save the original HUD element positions before moving them
                        originalCompassPos = compass.localPosition;
                        originalQuickSlotsPos = quickSlots.localPosition;
                        originalBarsPanelPos = barsPanel.localPosition;
                        originalPinnedReciepes = pinnedReciepes.localPosition;
                        orginalErrorMessagePos = ErrorMessages.localPosition;
                        orginalErrorMessageRot = ErrorMessages.rotation;

                        //move the elements
                        compass.SetParent(vehicleCanvas.transform, false);
                        quickSlots.SetParent(vehicleCanvas.transform, false);
                        barsPanel.SetParent(vehicleCanvas.transform, false);
                        pinnedReciepes.SetParent(vehicleCanvas.transform, false);
                        seatruckSegHealth.SetParent(vehicleCanvas.transform, false);
                        ErrorMessages.SetParent(vehicleCanvas.transform, false);
;                       //set custom element positions based on vehicle
                        if (player.inSeatruckPilotingChair)
                        {
                            compass.localPosition = seatruckCompassPos;
                            quickSlots.localPosition = seatruckQuickSlotsPos;
                            barsPanel.localPosition = seatruckBarsPanelPos;
                            pinnedReciepes.localPosition = seatruckPinnedReciepes;
                            seatruckSegHealth.gameObject.SetActive(true);
                            seatruckSegHealth.localPosition = seatruckSegmentHealthPos;
                            ErrorMessages.localPosition = seatruckErrorMessagePos;
                            ErrorMessages.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 1);
                        }
                        if (player.inHovercraft)
                        {
                            compass.localPosition = hoverbikeCompassPos;
                            quickSlots.localPosition = hoverbikeQuickSlotsPos;
                            barsPanel.localPosition = hoverbikeBarsPanelPos;
                            pinnedReciepes.localPosition = hoverbikePinnedReciepes;
                            seatruckSegHealth.gameObject.SetActive(false);
                            ErrorMessages.localPosition = hoverbikeErrorMessagePos;
                            ErrorMessages.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 1);
                        }
                        if (player.inExosuit)
                        {
                            compass.localPosition = exosuitCompassPos;
                            quickSlots.localPosition = exosuitQuickSlotsPos;
                            barsPanel.localPosition = exosuitBarsPanelPos;
                            pinnedReciepes.localPosition = exosuitPinnedReciepes;
                            ErrorMessages.localPosition = exosuitErrorMessagePos;
                            ErrorMessages.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 1);
                            seatruckSegHealth.gameObject.SetActive(false);
                        }
                        if (HandReticle.main != null)
                        {
                            if (!player.inHovercraft && !player.inSeatruckPilotingChair && !player.pda.isOpen)
                            {
                                HandReticle.main.UnrequestCrosshairHide();
                                Cursor.visible = true;
                            }
                            else
                            {
                                HandReticle.main.RequestCrosshairHide();
                                Cursor.visible = false;
                            }
                        }
                        //TODO:Make the anchor be the vehicle instead so the hud will not move with the head if I get the upper body ik working while piloting
                        canvasScaler.SetAnchor(SNCameraRoot.main.mainCam.transform.parent);
                        vehicleCanvas.SetActive(true);
                    }
                    //using vehicleCanvas.transform.up to compensate for the rotation done by the canvas scaler in inversed mode.
                    seatruckHUD.rotation = Quaternion.LookRotation(seatruckHUD.position, vehicleCanvas.transform.up);
                    hoverbikeHUD.rotation = Quaternion.LookRotation(hoverbikeHUD.position, vehicleCanvas.transform.up);
                    exosuitHUD.rotation = Quaternion.LookRotation(exosuitHUD.position, vehicleCanvas.transform.up);
                    quickSlots.rotation = Quaternion.LookRotation(quickSlots.position, vehicleCanvas.transform.up);
                    compass.rotation = Quaternion.LookRotation(compass.position, vehicleCanvas.transform.up);
                    barsPanel.rotation = Quaternion.LookRotation(barsPanel.position, vehicleCanvas.transform.up);
                    pinnedReciepes.rotation = Quaternion.LookRotation(pinnedReciepes.position, vehicleCanvas.transform.up);
                    seatruckSegHealth.rotation = Quaternion.LookRotation(seatruckSegHealth.position, vehicleCanvas.transform.up);
                    ErrorMessages.rotation = Quaternion.LookRotation(ErrorMessages.position, vehicleCanvas.transform.up);

                }
                else if (vehicleCanvas.activeInHierarchy)
                {
                    //if not in seatruck, snowfox(hoverbike) or exosuit but vehicleCanvas is active then move elements back to the normal HUD and disable vehicleCanvas
                    compass.SetParent(HUDContent, false);
                    compass.localPosition = originalCompassPos;

                    quickSlots.SetParent(HUDContent, false);
                    quickSlots.localPosition = originalQuickSlotsPos;

                    barsPanel.SetParent(HUDContent, false);
                    barsPanel.localPosition = originalBarsPanelPos;

                    pinnedReciepes.SetParent(HUDContent, false);
                    pinnedReciepes.localPosition = originalPinnedReciepes;

                    ErrorMessages.SetParent(HUDContent, false);
                    ErrorMessages.localPosition = orginalErrorMessagePos;
                    ErrorMessages.rotation = orginalErrorMessageRot;

                    seatruckSegHealth.SetParent(HUDContent, false);
                    seatruckSegHealth.gameObject.SetActive(false);

                    vehicleCanvas.SetActive(false);
                    //reset the rotation to look at the UI camera at (0,0,0);
                    UIElementsFixes.UpdateHUDLookAt();
                }
            }
        }
    }
}
