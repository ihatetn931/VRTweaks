//https://www.nexusmods.com/subnautica/mods/173
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;
using VRTweaks;
using System;

namespace VREnhancementsBZ
{
    //TODO: Break this up if possible
    class UIElementsFixes
    {
        //static RectTransform CameraCyclopsHUD;
        static RectTransform CameraDroneHUD;
        static readonly float CameraHUDScaleFactor = 0.75f;
        static uGUI_SceneHUD sceneHUD;
        static CanvasGroup resTrackerCG;
        static CanvasGroup pingsCG;
        public static bool seaglideEquipped = false;
        static Transform barsPanel;
        static Transform quickSlots;
        static Transform compass;
        static Transform reciepe;
        static Transform subTitles;
        static Transform toolTip;
        static bool fadeBarsPanel = true;
        static float lastHealth = -1;
        static float lastOxygen = -1;
        static float lastFood = -1;
        static float lastWater = -1;
        static Rect defaultSafeRect;
        static readonly float menuDistance = 4f;
        static readonly float menuScale = 0.002f;
        public static bool showDefaultReticle = true;
        static bool hudScene;

        public static void SetDynamicHUD(bool enabled)
        {
            //TODO: Decide if checking if faders are not null is necessary
            UIFader qsFader = quickSlots.gameObject.GetComponent<UIFader>();
            UIFader barsFader = barsPanel.gameObject.GetComponent<UIFader>();

            if (qsFader && barsFader)
            {
                qsFader.SetAutoFade(enabled);
                barsFader.SetAutoFade(enabled);
                //uGUI_SunbeamCountdown.main.GetComponent<UIFader>().SetAutoFade(enabled);
            }
        }

        public static void SetDynamicHUDReciepe(bool enabled)
        {
            //TODO: Decide if checking if faders are not null is necessary
            UIFader reciepeFader = reciepe.gameObject.GetComponent<UIFader>();
            if (reciepeFader)
            {
                reciepeFader.SetAutoFade(enabled);
            }
        }
        public static void UpdateHUDOpacity(float alpha)
        {
            if (sceneHUD)
            {
                sceneHUD.GetComponent<CanvasGroup>().alpha = alpha;
                if (VehicleHUDManager.vehicleCanvas)
                    VehicleHUDManager.vehicleCanvas.GetComponent<CanvasGroup>().alpha = alpha;
            }
            uGUI_SunbeamCountdown.main.transform.Find("Background").GetComponent<CanvasRenderer>().SetAlpha(0);//make sure the background remains hidden
            //make the blips and pings more translucent than other hud elements
            if (resTrackerCG)
                resTrackerCG.alpha = Mathf.Clamp(alpha - 0.2f,0.1f,1);
            if (pingsCG)
                pingsCG.alpha = Mathf.Clamp(alpha - 0.2f, 0.1f, 1);
        }
        public static void UpdateHUDDistance(float distance)
        {
            if (sceneHUD)
            {
                Transform screenCanvas = sceneHUD.transform.parent;
                Camera uicamera = ManagedCanvasUpdate.GetUICamera();
                if (uicamera != null)
                {
                    Transform transform = uicamera.transform;
                    //move the screen canvas instead of just the HUD so all on screen elements like blips etc are also affect by the distance update.
                    screenCanvas.transform.localPosition = screenCanvas.transform.parent.transform.InverseTransformPoint(transform.position + transform.forward * distance);
                    //make sure the elements are still facing the camera after changing position
                    UpdateHUDLookAt();
                }
            }
        }
        public static void UpdateHUDScale(float scale)
        {
            if (sceneHUD)
            {
                sceneHUD.GetComponent<RectTransform>().localScale = Vector3.one * scale;
            }
        }
        public static void UpdateHUDSeparation(float separation)
        {
            if (sceneHUD)
            {
                Rect safeAreaRect;
                //to make sure that the Rect is centered the width should be 1 - 2x
                switch (separation)
                {
                    case 0:
                        safeAreaRect = defaultSafeRect;
                        break;
                    case 1:
                        safeAreaRect = new Rect(0.3f,0.3f,0.4f,0.3f);
                        break;
                    case 2:
                        safeAreaRect = new Rect(0.2f, 0.2f, 0.6f, 0.5f);
                        break;
                    case 3:
                        safeAreaRect = new Rect(0.15f, 0.15f, 0.7f, 0.6f);
                        break;
                    default:
                        safeAreaRect = defaultSafeRect;
                        break;
                }
                sceneHUD.GetComponent<uGUI_SafeAreaScaler>().vrSafeRect = safeAreaRect;
                //the position of element in front the UI Camera would change if the Rect size changes so making sure the elements still face the camera.
                UpdateHUDLookAt();

            }
        }

        public static void InitHUD()
        {
            sceneHUD.transform.localPosition = Vector3.zero;//not sure why this isn't zero by default.
            //uGUI_SunbeamCountdown.main.transform.SetParent(quickSlots.parent, false);//changes parent from ScreenCanvas to HUD/Content
            //RectTransform holderRect = uGUI_SunbeamCountdown.main.countdownHolder.GetComponent<RectTransform>();
            // holderRect.anchorMax = holderRect.anchorMin = holderRect.pivot = new Vector2(0.5f, 0.5f);
            //holderRect.localPosition = Vector3.zero;
            //RectTransform sbRect = uGUI_SunbeamCountdown.main.GetComponent<RectTransform>();
            //sbRect.anchorMin = sbRect.anchorMax = new Vector2(0.5f, 0);

            UpdateHUDOpacity(VRCustomOptionsMenu.HUD_Alpha);
            UpdateHUDDistance(VRCustomOptionsMenu.HUD_Distance);
            UpdateHUDScale(VRCustomOptionsMenu.HUD_Scale);

            //UpdateHUDSeparation done in uGUI_SceneLoading.End instead

            if (!quickSlots.GetComponent<UIFader>())
            {
                UIFader qsFader = quickSlots.gameObject.AddComponent<UIFader>();
                if (qsFader)
                    qsFader.SetAutoFade(VRCustomOptionsMenu.dynamicHUD);
            }

            if (!barsPanel.GetComponent<UIFader>())
            {
                UIFader barsFader = barsPanel.gameObject.AddComponent<UIFader>();
                if (barsFader)
                {
                    barsFader.SetAutoFade(VRCustomOptionsMenu.dynamicHUD);
                    barsFader.autoFadeDelay = 2;
                }
            }

            if (!reciepe.GetComponent<UIFader>())
            {
                UIFader reciepeFader = reciepe.gameObject.AddComponent<UIFader>();
                if (reciepeFader)
                    reciepeFader.SetAutoFade(VRCustomOptionsMenu.pinnedReciepehud);

            }
        }

        public static void SetSubtitleHeight(float percentage)
        {
            if (subTitles != null)
                subTitles.localPosition = new Vector3(VRCustomOptionsMenu.subtitleX, percentage, VRCustomOptionsMenu.HUD_Distance);
        }

        public static void SetSubtitleX(float percentage)
        {
            if (subTitles != null)
                subTitles.localPosition = new Vector3(percentage, VRCustomOptionsMenu.subtitleHeight, VRCustomOptionsMenu.HUD_Distance);
        }

        public static void SetSubtitleScale(float scale)
        {
            if (subTitles != null)
                subTitles.localScale = new Vector3(scale, scale, scale);
        }

        public static void SetTooltipScale(float scale)
        {
            if (toolTip != null)
            {
                if(toolTip.GetComponent<uGUI_Tooltip>().scaleFactorMax != scale)
                    toolTip.GetComponent<uGUI_Tooltip>().scaleFactorMax = scale;
            }
        }

        public static void UpdateHUDLookAt()
        {
            quickSlots.rotation = Quaternion.LookRotation(quickSlots.position);
            compass.rotation = Quaternion.LookRotation(compass.position);
            barsPanel.rotation = Quaternion.LookRotation(barsPanel.position);
            reciepe.rotation  = Quaternion.LookRotation(reciepe.position);
        }

        [HarmonyPatch(typeof(Hint), nameof(Hint.Awake))]
        class Hint_Awake_Patch
        {
            static void Postfix(Hint __instance)
            {
                __instance.message.oy = 800;
                __instance.warning.oy = 800;
            }
        }

        [HarmonyPatch(typeof(uGUI_ResourceTracker), nameof(uGUI_ResourceTracker.Start))]
        class ResourceTracker_Start_Patch
        {
            static void Postfix(uGUI_ResourceTracker __instance)
            {
                if (!resTrackerCG)
                {
                    resTrackerCG = __instance.gameObject.AddComponent<CanvasGroup>();
                    resTrackerCG.alpha = Mathf.Clamp(VRCustomOptionsMenu.HUD_Alpha - 0.2f, 0.1f, 1);
                }
                    
            }
        }

        [HarmonyPatch(typeof(uGUI_Pings), nameof(uGUI_Pings.OnEnable))]
        class Pings_Enable_Patch
        {
            static void Postfix(uGUI_Pings __instance)
            {
                if (!pingsCG)
                {
                    pingsCG = __instance.canvasGroup;
                    pingsCG.alpha = Mathf.Clamp(VRCustomOptionsMenu.HUD_Alpha - 0.2f, 0.1f, 1);
                }
                    
            }
        }

        [HarmonyPatch(typeof(ErrorMessage), nameof(ErrorMessage.AddMessage))]
        class AddErrorMessage_Patch
        {
            //disables error messages while loading to prevent the ugly overlapping error messages
            static bool Prefix()
            {
                if (uGUI.main.loading.IsLoading)
                {
                    return false;
                }
                else
                    return true;
            }
        }

 /*       //make sure the black overlays always hides the background for all HUD distances
        [HarmonyPatch(typeof(uGUI_PlayerDeath), nameof(uGUI_PlayerDeath.Start))]
        class uGUI_PlayerDeath_Start_Patch
        {
            static void Postfix(uGUI_PlayerDeath __instance)
            {
               // __instance.blackOverlay.gameObject.GetComponent<RectTransform>().localScale = Vector3.one * 2;
            }
        }

        [HarmonyPatch(typeof(uGUI_PlayerSleep), nameof(uGUI_PlayerSleep.Start))]
        class uGUI_PlayerSleep_Start_Patch
        {
            static void Postfix(uGUI_PlayerSleep __instance)
            {
               // __instance.blackOverlay.gameObject.GetComponent<RectTransform>().localScale = Vector3.one * 2;
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneIntro), nameof(uGUI_SceneIntro.Start))]
        class uGUI_uGUI_SceneIntro_Start_Patch
        {
            static void Postfix(uGUI_SceneIntro __instance)
            {
                __instance.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.one * 2000;
            }
        }*/

        [HarmonyPatch(typeof(Seaglide), nameof(Seaglide.OnDraw))]
        class Seaglide_OnDraw_Patch
        {
            static void Postfix()
            {
                seaglideEquipped = true;
            }
        }
        [HarmonyPatch(typeof(Seaglide), nameof(Seaglide.OnHolster))]
        class Seaglide_OnHolster_Patch
        {
            static void Postfix()
            {
                seaglideEquipped = false;
            }
        }

        [HarmonyPatch(typeof(uGUI), nameof(uGUI.Update))]
        class uGUI_Update_Patch
        {
            static void Postfix(uGUI __instance)
            {
                toolTip = __instance.transform.Find("Tooltip");
                SetTooltipScale(VRCustomOptionsMenu.toolTipscale);
            }
        }



        /*[HarmonyPatch(typeof(uGUI), nameof(uGUI.Awake))]
        class LoadingScreen_Patch
        {
            static void Postfix(uGUI __instance)
            {
                if (!__instance.loading.GetComponent<VRLoadingScreen>())
                    __instance.loading.gameObject.AddComponent<VRLoadingScreen>();
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.ShowLoadingScreen))]
        class uGUI_ShowLoading_Patch
        {
            static bool Prefix()
            {
                VRLoadingScreen.main.StartLoading();
                return true;
            }
        }*/

        //EnsureCreated is called at the end of PAXTerrainController.LoadAsync() which is just before the player takes control
        [HarmonyPatch(typeof(uGUI_BuilderMenu), nameof(uGUI_BuilderMenu.EnsureCreatedAsync))]
        class Loading_End_Patch
        {
            static void Postfix()
            {
                ManagedCanvasUpdate.GetUICamera().clearFlags = CameraClearFlags.Depth;//fixes problem with right hand tools preventing some blips from showing
                InitHUD();
                UpdateHUDLookAt();
                UpdateHUDSeparation(VRCustomOptionsMenu.HUD_Separation);//wasn't working in HUD awake so put it here instead
                VRTweaks.VRTweaks.Recenter();
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Awake))]
        class SceneHUD_Awake_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                Transform HUDContent;
                hudScene = true;
                sceneHUD = __instance;
                if (!sceneHUD.gameObject.GetComponent<CanvasGroup>())
                    sceneHUD.gameObject.AddComponent<CanvasGroup>();//add CanvasGroup to the HUD to be able to set the alpha of all HUD elements
                barsPanel = __instance.transform.Find("Content/BarsPanel");
                quickSlots = __instance.transform.Find("Content/QuickSlots");
                compass = __instance.transform.Find("Content/DepthCompass");
                reciepe = __instance.transform.Find("Content/PinnedRecipes");
                HUDContent = GameObject.Find("SubtitlesCanvas").transform;
                subTitles = HUDContent.Find("Subtitles");
                defaultSafeRect = sceneHUD.GetComponent<uGUI_SafeAreaScaler>().vrSafeRect;
                __instance.gameObject.AddComponent<VehicleHUDManager>();
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        class Player_Update_Patch
        {
            static void Postfix()
            {
                if (hudScene)
                {
                    UIFader barsFader = barsPanel.GetComponent<UIFader>();
                    UIFader qsFader = quickSlots.GetComponent<UIFader>();
                    UIFader reciepeFader = reciepe.GetComponent<UIFader>();

                    Player player = Player.main;

                    Survival survival = player.GetComponent<Survival>();

                    fadeBarsPanel = VRCustomOptionsMenu.dynamicHUD;
                    //float fadeInStart = 10;
                    //float fadeRange = 10;//max alpha at start+range degrees

                    if (VRCustomOptionsMenu.dynamicHUD && !player.GetPDA().isInUse && survival && barsFader)
                    {
                        if (Mathf.Abs(player.liveMixin.health - lastHealth) / player.liveMixin.maxHealth > 0.05f || player.liveMixin.GetHealthFraction() < 0.33f ||
                            player.GetOxygenAvailable() < (player.GetOxygenCapacity() / 3) || player.GetOxygenAvailable() > lastOxygen ||
                            survival.food < 50 || survival.food > lastFood ||
                            survival.water < 50 || survival.water > lastWater || survival.bodyTemperature.currentColdMeterValue > 50)
                        {
                            fadeBarsPanel = false;
                        }
                        lastHealth = player.liveMixin.health;
                        lastOxygen = player.GetOxygenAvailable();
                        lastFood = survival.food;
                        lastWater = survival.water;

                        barsFader.SetAutoFade(fadeBarsPanel);
                        qsFader.SetAutoFade(!Player.main.inExosuit && !Player.main.inSeatruckPilotingChair && !Player.main.inHovercraft);
                        reciepeFader.SetAutoFade(!Player.main.inExosuit && !Player.main.inSeatruckPilotingChair && !Player.main.inHovercraft && VRCustomOptionsMenu.pinnedReciepehud);
                    }
                    else
                    {
                        if(barsFader != null)
                            barsFader.SetAutoFade(false);

                        if(qsFader != null)
                            qsFader.SetAutoFade(false);

                        if(reciepeFader != null)
                            reciepeFader.SetAutoFade(false);
                    }
                    //if the PDA is in use turn on look down for hud
                    if (player.GetPDA().isInUse)
                    {
                        barsFader.SetAutoFade(false);
                        qsFader.SetAutoFade(false);
                        reciepeFader.SetAutoFade(false);
                        //uGUI_SunbeamCountdown.main.GetComponent<UIFader>().SetAutoFade(false);
                        //fades the hud in based on the view pitch. Forward is 360/0 degrees and straight down is 90 degrees.

                        //  if (MainCamera.camera.transform.localEulerAngles.x < 10)
                        //    UpdateHUDOpacity(Mathf.Clamp((MainCamera.camera.transform.localEulerAngles.x - fadeInStart) / fadeRange, 0, 1) * VRCustomOptionsMenu.HUD_Alpha);
                        //   else
                        //      UpdateHUDOpacity(0);
                    }//opacity is set back to HUDAlpha in PDA.Close Postfix
                }
            }
        }

        [HarmonyPatch(typeof(PDA), nameof(PDA.Close))]
        class PDA_Close_Patch
        {
            static void Postfix()
            {
                UpdateHUDOpacity(VRCustomOptionsMenu.HUD_Alpha);
            }
        }

        [HarmonyPatch(typeof(QuickSlots), nameof(QuickSlots.NotifySelect))]
        class QuickSlots_NotifySelect_Patch
        {
            static void Postfix()
            {
                UIFader qsFader = quickSlots.GetComponent<UIFader>();
                UIFader reciepeFader = reciepe.GetComponent<UIFader>();
                if(qsFader && reciepeFader)
                {
                    qsFader.Fade(VRCustomOptionsMenu.HUD_Alpha, 0, 0, true);//make quickslot visible as soon as the slot changes. Using Fade to cancel any running fades.
                    reciepeFader.Fade(VRCustomOptionsMenu.HUD_Alpha, 0, 0, true);
                    if (!seaglideEquipped)
                    {
                        qsFader.autoFadeDelay = 2;
                        reciepeFader.autoFadeDelay = 2;
                    }
                    else
                    {
                        qsFader.autoFadeDelay = 1;//fade with shorter delay if seaglide is active.
                        reciepeFader.autoFadeDelay = 1;
                    }
                    qsFader.SetAutoFade((VRCustomOptionsMenu.dynamicHUD || seaglideEquipped));
                    reciepeFader.SetAutoFade((VRCustomOptionsMenu.pinnedReciepehud || seaglideEquipped));
                }                
            }
        }

        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.Start))]
        class HandReticle_Start_Patch
        {
            //TODO: Make the tool power text appear and fade away after selecting a tool instead of when pointing at surfaces(weird that they did it this way)
            public static Sprite defaultReticle;
            static void Postfix()
            {
                if (HandReticle.main)
                    defaultReticle = HandReticle.main.transform.Find("IconCanvas/Default").gameObject.GetComponent<Image>().sprite;
            }
        }

        [HarmonyPatch(typeof(uGUI_MapRoomScanner), nameof(uGUI_MapRoomScanner.OnTriggerEnter))]
        class MapRoomScanner_TriggerEnter_Patch
        {
            static void Postfix(uGUI_MapRoomScanner __instance)
            {
                if (__instance.raycaster.enabled)
                    showDefaultReticle = true;
            }
        }

        [HarmonyPatch(typeof(uGUI_MapRoomScanner), nameof(uGUI_MapRoomScanner.OnTriggerExit))]
        class MapRoomScanner_TriggerExit_Patch
        {
            static void Postfix(uGUI_MapRoomScanner __instance)
            {
                if (!__instance.raycaster.enabled)
                    showDefaultReticle = false;
            }
        }


        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.SetIconInternal))]
        class HandReticle_SetIconInt_Patch
        {
            static bool Prefix(ref HandReticle.IconType newIconType)
            {
                //only show the reticle on interactive elements
                if (newIconType == HandReticle.IconType.Default && !showDefaultReticle && !Player.main.isPiloting)
                {
                        newIconType = HandReticle.IconType.None;
                }
                return true;

            }
        }

        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.LateUpdate))]
        class HR_LateUpdate_Patch
        {
            //fixes the reticle distance being locked to the interaction distance after interaction. eg Entering Seamoth and piloting Cyclops
            static bool Prefix(HandReticle __instance)
            {
                if (Player.main)
                {
                    Targeting.GetTarget(Player.main.gameObject, 2f, out GameObject activeTarget, out float reticleDistance);
                    SubRoot currSub = Player.main.GetCurrentSub();
                    //if piloting the cyclops and not using cyclops cameras
                    //TODO: find a way to use the raycast distance for the ui elements instead of the fixed value of 1.55
                    if (Player.main.isPiloting && currSub)
                    {
                        __instance.SetTargetDistance(reticleDistance > 1.55f ? 1.55f : reticleDistance);
                    }
                    else if (Player.main.GetMode() == Player.Mode.LockedPiloting)
                    {
                        __instance.SetTargetDistance(VRCustomOptionsMenu.HUD_Distance);
                   }
                }
                return true;
            }
            static void Postfix(HandReticle __instance)
            {
                float distance = __instance.transform.position.z;
                distance = distance < 0.6f ? 0.6f : distance;//prevent the reticle from getting too close to the player
                //this fixes reticle alignment in menus etc
                __instance.transform.position = new Vector3(0f, 0f, distance);
            }
        }

        [HarmonyPatch(typeof(uGUI_CameraDrone), nameof(uGUI_CameraDrone.Awake))]
        class CameraDrone_Awake_Patch
        {
            //Reduce the size of the HUD in the Drone Camera to make edges visible
            static void Postfix(uGUI_CameraDrone __instance)
            {
                CameraDroneHUD = __instance.transform.Find("Content/CameraScannerRoom").GetComponent<RectTransform>();
                if (CameraDroneHUD)
                {
                    CameraDroneHUD.localScale = new Vector3(CameraHUDScaleFactor * VRCustomOptionsMenu.HUD_Scale, CameraHUDScaleFactor * VRCustomOptionsMenu.HUD_Scale, 1f);
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_CameraDrone), nameof(uGUI_CameraDrone.OnEnable))]
        class CameraDrone_OnEnable_Patch
        {
            //make sure the camera HUD is visible
            //TODO: Check is this is still needed
            static void Postfix()
            {
                if (sceneHUD)
                    sceneHUD.GetComponent<CanvasGroup>().alpha = VRCustomOptionsMenu.HUD_Alpha;
            }
        }


        [HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.GetCursorScreenPosition))]
        class GetCursorScreenPosition_Patch
        {
            public static Vector2 result;
            static bool Prefix(ref Vector2 __result, FPSInputModule __instance)
            {

                if (XRSettings.enabled)
                {
                    if (Cursor.lockState == CursorLockMode.Locked)
                    {
                        Vector3 pos = MainCamera.camera.transform.position + MainCamera.camera.transform.forward * FPSInputModule.current.maxInteractionDistance;
                        result = MainCamera.camera.WorldToScreenPoint(pos);
                    }
                    else if (!VROptions.gazeBasedCursor)
                    {
                        result = new Vector2(Input.mousePosition.x / Screen.width * Camera.current.pixelWidth, Input.mousePosition.y / Screen.height * Camera.current.pixelHeight);
                    }

                }
                __result = result;
                return false;
            }
        }

        [HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.UpdateCursor))]
        class UpdateCursor_Pre_Patch
        {
            static bool Prefix(FPSInputModule __instance)
            {
                float num = 0.5f;
                float num2 = 0.1f;
                float num3 = Time.unscaledTime - __instance.lastValidRaycastTime;
                bool flag = __instance.lastGroup != null;
                if (!VROptions.GetUseGazeBasedCursor())
                {
                    //flag = false;
                }

                Vector3 worldPosition = __instance.lastRaycastResult.worldPosition;
                if (num3 > 0f)
                {
                    if (num3 > num + num2)
                    {
                        flag = false;
                    }
                    else
                    {
                        Vector2 cursorScreenPosition = __instance.GetCursorScreenPosition();
                        if (!FPSInputModule.ScreenToWorldPoint(__instance.lastRaycastResult, cursorScreenPosition, ref worldPosition))
                        {
                            flag = false;
                        }
                    }
                }
                GameObject cursor = __instance.cursor;
                if (cursor && HandReticle_Start_Patch.defaultReticle)
                {
                    cursor.GetComponentInChildren<Image>().overrideSprite = HandReticle_Start_Patch.defaultReticle;
                    if (cursor.transform.localScale.x > 0.002f)
                        cursor.transform.localScale = Vector3.one * 0.002f;
                }
                Vector3 vector;
                Quaternion rotation;
                Vector3 localScale;
                int layer;
                if (flag && FPSInputModule.ExtractParams(__instance.lastRaycastResult.gameObject, worldPosition, out vector, out rotation, out localScale, out layer))
                {
                    cursor.layer = layer;
                    cursor.transform.position =  worldPosition;
                    cursor.transform.rotation = rotation;
                    cursor.transform.localScale = localScale;
                    if ( __instance.cursorGraphic != null)
                    {
                        if(__instance.cursorGraphic.canvas != null)
                        __instance.cursorGraphic.canvas.sortingLayerID = __instance.lastRaycastResult.sortingLayer;
                        Color color = __instance.cursorGraphic.color;
                        color.a = 1f - Mathf.Clamp01((num3 - num) / num2);
                        __instance.cursorGraphic.color = color;
                    }
                }
                if (cursor.activeSelf != flag)
                {
                    cursor.SetActive(flag);
                }
                return false;
            }
        }

 /*       [HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.UpdateCursor))]
        class UpdateCursor_Patch
        {
            static void Prefix()
            {
                //save the original value so we can set it back in the postfix
                actualGazedBasedCursor = VROptions.gazeBasedCursor;
                //trying make flag in UpdateCursor be true if Cursor.lockState != CursorLockMode.Locked
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    //VROptions.gazeBasedCursor = true;
                }

            }
            static void Postfix(FPSInputModule __instance)
            {
                VROptions.gazeBasedCursor = actualGazedBasedCursor;
                //Fix the problem with the cursor rendering behind UI elements.
                Canvas cursorCanvas = __instance._cursor.GetComponentInChildren<Graphic>().canvas;
                //RaycastResult lastRaycastResult = Traverse.Create(__instance).Field("lastRaycastResult").GetValue<RaycastResult>();
                if (cursorCanvas && __instance.lastRaycastResult.isValid)
                {
                    cursorCanvas.sortingLayerID = __instance.lastRaycastResult.sortingLayer;//put the cursor on the same layer as whatever was hit by the cursor raycast.
                }
                //change the VR cursor to look like the default hand reticle cursor for better accuracy when selecting smaller ui elements
                //TODO: Find a way to not do this every frame.
                if (__instance._cursor && HandReticle_Start_Patch.defaultReticle)
                {
                    __instance._cursor.GetComponentInChildren<Image>().overrideSprite = HandReticle_Start_Patch.defaultReticle;
                    if (__instance._cursor.transform.localScale.x > 0.002f)
                        __instance._cursor.transform.localScale = Vector3.one * 0.002f;
                }

            }
        }*/

        static Transform screenCanvas;
        static Transform overlayCanvas;
        static Transform mainMenuUICam;
        static Transform mainMenu;
        [HarmonyPatch(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Awake))]
        class MM_Awake_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
                //GameObject mainCam = GameObject.Find("Main Camera");
                mainMenuUICam = ManagedCanvasUpdate.GetUICamera().transform;
                mainMenu = __instance.transform.Find("Panel/MainMenu");
                screenCanvas = GameObject.Find("ScreenCanvas").transform;
                overlayCanvas = GameObject.Find("OverlayCanvas").transform;

                __instance.canvasScaler.enabled = false;//disabling the canvas scaler to prevent it from messing up the custom distance and scale
                __instance.transform.position = new Vector3(mainMenuUICam.transform.position.x + menuDistance,-0.8f,0);
                __instance.transform.localScale = Vector3.one * menuScale * 2f;
                __instance.gameObject.GetComponent<Canvas>().scaleFactor = 1.25f;//sharpen text
                VRTweaks.VRTweaks.Recenter();
                var hud = GameObject.FindObjectOfType<uGUI_SceneHUD>();
                if (hud != null)
                {
                    if (!hudScene)
                    {
                        hud.Awake();
                        hudScene = true;
                    }
                }

            }
        }

        [HarmonyPatch(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Update))]
        class MM_Update_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
                //keep the main menu tilted towards the camera.
                mainMenu.transform.root.rotation = Quaternion.LookRotation(mainMenu.position - new Vector3(mainMenuUICam.position.x, mainMenuUICam.position.y, mainMenu.position.z));
                //match screen and overlay canvas position and rotation to main menu
                screenCanvas.localPosition = overlayCanvas.localPosition = __instance.transform.localPosition;
                screenCanvas.position = overlayCanvas.position = __instance.transform.position;
                screenCanvas.rotation = overlayCanvas.rotation = __instance.transform.rotation;

                CanvasGroup rect;
                rect = __instance.canvasGroup;
                rect.alpha = VRCustomOptionsMenu.HUD_Alpha;
                //try to keep the main menu visible if the HMD is moved more than 0.5 after starting the game.
                if (mainMenuUICam.localPosition.magnitude > 0.5f)
                    VRTweaks.VRTweaks.Recenter();
                //make sure the cursor remains visible after clicking outside the menu area
                if (!FPSInputModule.current.lastGroup)
                    __instance.Select(false);
            }
        }

        [HarmonyPatch(typeof(MainMenuLoadButton), nameof(MainMenuLoadButton.Start))]
        class MMLoadBtn_Start_Patch
        {
            static void Postfix(MainMenuLoadButton __instance)
            {
                //saved game buttons are not on the UI layer so the cursor disappears when it gets set to default layer after raycasting on those elements
                Transform[] children = __instance.GetComponentsInChildren<Transform>();
                foreach (Transform child in children)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("UI");
                }
            }
        }

        [HarmonyPatch(typeof(IngameMenu), nameof(IngameMenu.Open))]
        class InGameMenu_Open_Patch
        {
            static bool Prefix(IngameMenu __instance)
            {
                uGUI_CanvasScaler canvasScaler = __instance.gameObject.GetComponent<uGUI_CanvasScaler>();
                canvasScaler.distance = menuDistance;
                __instance.transform.localScale = Vector3.one * menuScale *2;
                __instance.gameObject.GetComponent<Canvas>().scaleFactor = 1.5f;//sharpen text
                return true;
            }
        }

        [HarmonyPatch(typeof(uGUI_BuilderMenu), nameof(uGUI_BuilderMenu.Open))]
        class uGUI_BuilderMenu_Open_Patch
        {
            static bool Prefix(uGUI_BuilderMenu __instance)
            {
                uGUI_CanvasScaler canvasScaler = __instance.gameObject.GetComponent<uGUI_CanvasScaler>();
                canvasScaler.distance = menuDistance;
                __instance.transform.localScale = Vector3.one * menuScale * 2;
                __instance.gameObject.GetComponent<Canvas>().scaleFactor = 1.5f;//sharpen text
                return true;
            }
        }

/*        [HarmonyPatch(typeof(uGUI_BuildWatermark), nameof(uGUI_BuildWatermark.UpdateText))]
        class BWM_UpdateText_Patch
        {
            static bool Prefix(uGUI_BuildWatermark __instance)
            {
                //make the version watermark more visible
                string plasticChangeSetOfBuild = SNUtils.GetPlasticChangeSetOfBuild();
                DateTime dateTimeOfBuild = SNUtils.GetDateTimeOfBuild();
                __instance.text.text = Language.main.GetFormat<DateTime, string>("EarlyAccessWatermarkFormat", dateTimeOfBuild, plasticChangeSetOfBuild);
                __instance.text.color = new Vector4(1,1,1,1.0f);
                __instance.transform.localPosition = new Vector3(950, 450, 0);
                return false;

            }
        }*/

        [HarmonyPatch(typeof(uGUI_CanvasScaler), nameof(uGUI_CanvasScaler.SetScaleFactor))]
        class Canvas_ScaleFactor_Patch
        {
            static bool Prefix(ref float scaleFactor)
            {
                //any scale factor less than 1 reduces the quality of UI elements.
                if (scaleFactor < 1)
                    scaleFactor = 1;
                return true;
            }
        }

        [HarmonyPatch(typeof(uGUI_CanvasScaler), nameof(uGUI_CanvasScaler.UpdateTransform))]
        class Canvas_UpdateTransform_Patch
        {
            static bool Prefix(uGUI_CanvasScaler __instance)
            {
                if (__instance.gameObject.name == "ScreenCanvas")
                    __instance.distance = VRCustomOptionsMenu.HUD_Distance;
                return true;
            }
        }

        [HarmonyPatch(typeof(uGUI_CanvasScaler), nameof(uGUI_CanvasScaler.UpdateFrustum))]
        class Canvas_UpdateFrustum_Patch
        {
            //doing this to maintain the original canvas scale after changing the canvas scaler distance
            static float customDistance=1;
            static bool Prefix(uGUI_CanvasScaler __instance)
            {
                if (__instance.gameObject.name == "ScreenCanvas")
                {
                    //save the modified canvas distance
                    customDistance = __instance.distance;
                    //set the canvas distance back to the default 1 before UpdateFrustum calculates the scale
                    __instance.distance = 1;
                }
                return true;
            }
            static void Postfix(uGUI_CanvasScaler __instance)
            {
                //restore the modified canvas distance after the scale has been calculated.
                if (__instance.gameObject.name == "ScreenCanvas")
                    __instance.distance = customDistance;
            }
        }
    }
}