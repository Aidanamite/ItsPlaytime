using BepInEx;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using System.IO;
using static BundlesData.Category;
using Object = UnityEngine.Object;
using UnityEngine.UIElements;

namespace ItsPlaytime
{
    [BepInPlugin("com.aidanamite.ItsPlaytime", "It's Playtime", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        [Serializable]
        class ScriptingAssemblies_Data
        {
            public string[] names;
            public int[] types;
        }
        static Main()
        {
            var restart = false;
            var file = "DOMain_Data\\ScriptingAssemblies.json";
            var data = JsonUtility.FromJson<ScriptingAssemblies_Data>(Encoding.UTF8.GetString(File.ReadAllBytes(file)));
            if (!data.names.Any( x=> x.ToLowerInvariant() == "eelroastcore.dll"))
            {
                restart = true;
                data.names = data.names.AddToArray("EelRoastCore.dll");
                File.WriteAllBytes(file, Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)));
            }
            file = "DOMain_Data\\Managed\\EelRoastCore.dll";
            if (!File.Exists(file))
            {
                restart = true;
                File.WriteAllBytes(file, GetResource("EelRoastCore.dll"));
            }
            else if (GetEelRoastCoreVersion() < new Version("1.0.0"))
            {
                File.WriteAllBytes(file+".replace", GetResource("EelRoastCore.dll"));
                File.WriteAllBytes("DOMain_Data\\Managed\\PlaytimeUpdater.exe", GetResource("Updater.exe"));
                Process.Start("DOMain_Data\\Managed\\PlaytimeUpdater.exe", $"r {Process.GetCurrentProcess().Id} \"{Path.GetFullPath(file)}\" \"{Environment.CurrentDirectory}\" {Environment.CommandLine}");
            }
            if (restart)
            {
                File.WriteAllBytes("DOMain_Data\\Managed\\PlaytimeUpdater.exe", GetResource("Updater.exe"));
                Process.Start("DOMain_Data\\Managed\\PlaytimeUpdater.exe", $"b {Process.GetCurrentProcess().Id} \"{Environment.CurrentDirectory}\" {Environment.CommandLine}");
            }
        }

        static byte[] GetResource(string name)
        {
            byte[] b;
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("ItsPlaytime." + name))
            {
                b = new byte[s.Length];
                s.Read(b, 0, b.Length);
            }
            return b;
        }

        static Version GetEelRoastCoreVersion()
        {
            try
            {
                return GetEelRoastCoreVersion_Internal();
            } catch
            {
                return new Version(0, 0);
            }
        }

        static Version GetEelRoastCoreVersion_Internal() => typeof(EBGameManager).Assembly.GetName().Version;

        public static Texture sprites = GetTextureResource("ItsPlaytime.play.png");
        public static void SetupAtlas(UIAtlas atlas)
        {
            atlas.spriteMaterial.mainTexture = sprites;
            atlas.spriteList.AddRange(new[] {
                new UISpriteData()
                {
                    name = "play",
                    width = 128,
                    height = 128
                },
                new UISpriteData()
                {
                    name = "play_down",
                    width = 128,
                    height = 128,
                    x = 128
                },
                new UISpriteData()
                {
                    name = "ball",
                    width = 128,
                    height = 128,
                    y = 128
                },
                new UISpriteData()
                {
                    name = "ball_down",
                    width = 128,
                    height = 128,
                    x = 128,
                    y = 128
                },
                new UISpriteData()
                {
                    name = "eel",
                    width = 128,
                    height = 128,
                    y = 256
                },
                new UISpriteData()
                {
                    name = "eel_down",
                    width = 128,
                    height = 128,
                    x = 128,
                    y = 256
                },
                new UISpriteData()
                {
                    name = "element",
                    width = 128,
                    height = 128,
                    y = 384
                },
                new UISpriteData()
                {
                    name = "element_down",
                    width = 128,
                    height = 128,
                    x = 128,
                    y = 384
                }
            });
        }
        public static (string name, string bundle, Func<string> petArea, string sprite)[] PlayOptions = new (string, string, Func<string>, string)[]
        {
            ("Play", null, () => {
                    var n = RsResourceManager.pCurrentLevel.ToLowerInvariant();
                    //if (n.Contains("stable"))
                        //return "RS_DATA/PfPetPlayAreaDragonStableDO.unity3d/PfPetPlayAreaDragonStableDO"; // Disabled due to non-functional toy list
                    if (n.Contains("school") || n.Contains("lookout") || n.Contains("training"))
                        return "RS_DATA/PfPetPlayHubSchoolDO.unity3d/PfPetPlayHubSchoolDO";
                    if (n.Contains("berk") || n.Contains("greathall"))
                        return "RS_DATA/PfPetPlayHubBerkDO.unity3d/PfPetPlayHubBerkDO";
                    if (n.Contains("hiddenworld"))
                        return "RS_DATA/PfPetPlayHubHiddenWorldDO.unity3d/PfPetPlayHubHiddenWorldDO";
                    if (n.Contains("arctic") || n.Contains("glacier") || n.Contains("scuttleclaw") || n.Contains("valka"))
                        return "RS_DATA/PfPetPlayHubArctic01DO.unity3d/PfPetPlayHubArctic01DO";
                    return "RS_DATA/PfPetPlayHubWilderness01DO.unity3d/PfPetPlayHubWilderness01DO";
                }, "ball"),
            ("Eel Roast", "PetPlayDM", () => "RS_DATA/PfPetPlayEelBlastDO.unity3d/PfPetPlayEelBlastDO", "eel"),
            ("Element Match", "ElementMatchDO", null, "element")
        };
        public static Dictionary<string, LoadAsmBundle> BundleOverrides = new Dictionary<string, LoadAsmBundle>();
        public void Awake()
        {
            var a = Assembly.GetExecutingAssembly();
            foreach (var i in a.GetManifestResourceNames())
                if (i.ToLowerInvariant().EndsWith(".bundle"))
                    using (var s = a.GetManifestResourceStream(i))
                    {
                        var b = AssetBundle.LoadFromStream(s);
                        BundleOverrides[b.name.ToLowerInvariant()] = new LoadAsmBundle()
                        {
                            bundle = b,
                            resource = i,
                            url = b.name
                        };
                    }
            new Harmony("com.aidanamite.ItsPlaytime").PatchAll();
            Logger.LogInfo("Loaded");
        }
        public static Texture GetTextureResource(string key)
        {
            var a = Assembly.GetExecutingAssembly();
            byte[] b;
            using (var s = a.GetManifestResourceStream(key))
            {
                b = new byte[s.Length];
                s.Read(b, 0, b.Length);
            }
            var t = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            t.LoadImage(b,false);
            return t;
        }
    }

    public static class ExtentionMethods
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component => gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        public static T GetOrAddComponent<T>(this Component component) where T : Component => component.GetComponent<T>() ?? component.gameObject.AddComponent<T>();
    }

    [HarmonyPatch]
    static class Patch_AddItemCategory
    {
        [HarmonyPatch(typeof(ItemData), "AddToCache")]
        [HarmonyPrefix]
        static void AddToCache(ItemData idata)
        {
            if (idata.ItemID == 8032) // Dragon Nip
            {
                if (!idata.Category.Any(x => x.CategoryId == 326))
                    idata.Category = idata.Category.AddToArray(new ItemDataCategory() { CategoryId = 326, CategoryName = "Pet Play Food" });
                if (!idata.Attribute.Any(x => x.Key == "Type"))
                    idata.Attribute = idata.Attribute.AddToArray(new ItemAttribute() { Key = "Type", Value = "Brush" });
            }
        }
        [HarmonyPatch(typeof(CommonInventoryData), "AddToCategories")]
        [HarmonyPrefix]
        static void AddToCategories(UserItemData item)
        {
            if (item?.Item != null) // Dragon Nip
                AddToCache(item.Item);
        }
    }

    [HarmonyPatch(typeof(UiAvatarCSM))]
    static class Patch_PlayerCSM
    {
        [HarmonyPatch("OpenCSM")]
        [HarmonyPrefix]
        static void OpenCSM(UiAvatarCSM __instance)
        {
            var e = ExtendedCSM.Get(__instance);
            if (!e.BtnPlay)
            {
                e.BtnPlay = __instance.DuplicateWidget(__instance.mBtnCSMMiniGames, __instance.mBtnCSMMiniGames.pAnchor.side) as KAButton;
                __instance.mBtnCSMMiniGames.GetParentItem()?.AddChild(e.BtnPlay);
                e.BtnPlay.SetText("Playtime");
                var icon = e.BtnPlay.transform.Find("Icon").GetComponent<UISprite>();
                __instance._MiniGamePanelUI.gameObject.AddComponent<Tracker>().tracked = new[] { __instance.mBtnCSMDragonTactics.GetParentItem(), __instance.mBtnCSMMiniGameBack }; 
                e.PlayPanelUI = Object.Instantiate(__instance._MiniGamePanelUI, __instance._MiniGamePanelUI.transform.parent);
                Object.Destroy(__instance._MiniGamePanelUI.GetComponent<Tracker>());
                var parent = e.PlayPanelUI.GetComponent<Tracker>().tracked[0] as KAWidget;
                e.BtnBack = e.PlayPanelUI.GetComponent<Tracker>().tracked[1] as KAButton;
                e.PlayPanelUI.gameObject.SetActive(true);
                e.PlayPanelUI.SetVisibility(false);
                e.PlayPanelUI.gameObject.SetActive(false);
                var atlas = Object.Instantiate(e.BtnBack.pBackground.atlas,e.PlayPanelUI.transform);
                atlas.name = "PlayAtlas";
                var nm = Object.Instantiate(atlas.spriteMaterial);
                atlas.replacement = null;
                atlas.spriteMaterial = nm;
                Main.SetupAtlas(atlas);
                icon.atlas = atlas;
                icon.spriteName = "play";
                icon.pOrgSprite = "play";
                e.BtnPlay._PressInfo._SpriteInfo = new KASkinSpriteInfo()
                {
                    _Sprites = new[]
                    {
                            new KASkinSprite() { _ApplyTo = icon, _SpriteName = "play_down" }
                        },
                    _UseSprite = true
                };
                foreach (var i in e.PlayPanelUI.GetComponentsInChildren<KAButton>(true))
                    if (i != e.BtnBack)
                        Object.Destroy(i.gameObject);
                foreach (var i in Main.PlayOptions)
                {
                    var n = e.PlayPanelUI.DuplicateWidget(__instance.mBtnCSMDragonTactics, __instance.mBtnCSMDragonTactics.pAnchor.side) as KAButton;
                    n.name = i.name;
                    parent?.AddChild(n);
                    e.Options[n] = (i.bundle,i.petArea);
                    n.SetText(i.name);
                    icon = n.transform.Find("Icon").GetComponent<UISprite>();
                    icon.atlas = atlas;
                    icon.spriteName = i.sprite;
                    icon.pOrgSprite = i.sprite;
                    n._PressInfo._SpriteInfo = new KASkinSpriteInfo()
                    {
                        _Sprites = new[]
                        {
                            new KASkinSprite() { _ApplyTo = icon,_SpriteName = i.sprite+"_down" }
                        },
                        _UseSprite = true
                    };
                }

                e.PlayPanelUI.pEvents.OnClick += (x) =>
                {
                    if (x == e.BtnBack)
                    {
                        e.PlayPanelUI.SetVisibility(false);
                        e.PlayPanelUI.gameObject.SetActive(false);
                        __instance.mOpenCSM = true;
                        __instance.SetVisibility(false);
                    }
                    else if (e.Options.TryGetValue(x as KAButton,out var scene))
                    {
                        PetPlayAreaLoader._PetPlayAreaResourceName = scene.petArea?.Invoke();
                        if (scene.bundle == null)
                        {
                            SanctuaryManager.LoadTempPet(SanctuaryManager.pCurPetInstance);
                            Patch_LoadPetPlayArea.LoadPetPlayArea();
                            __instance.Close();
                        }
                        else
                        {
                            var avControl = AvAvatar.pObject.GetComponent<AvAvatarController>();
                            if (avControl != null)
                            {
                                if (avControl.IsValidLastPositionOnGround())
                                    AvAvatar.pStartPosition = avControl.pLastPositionOnGround;
                                else
                                    AvAvatar.pStartLocation = null;
                                AvAvatar.pStartRotation = AvAvatar.mTransform.rotation;
                            }
                            PetPlayAreaLoader._ExitToScene = RsResourceManager.pCurrentLevel;
                            RsResourceManager.LoadLevel(scene.bundle, true);
                        }
                    }
                };
            }
        }

        [HarmonyPatch("OnClick",typeof(KAWidget))]
        [HarmonyPostfix]
        static void OnClick(UiAvatarCSM __instance, KAWidget inWidget)
        {
            var e = ExtendedCSM.Get(__instance);
            if (e.BtnPlay && inWidget == e.BtnPlay)
            {
                __instance.EnableButtons(false, false);
                e.PlayPanelUI.SetVisibility(true);
                e.PlayPanelUI.gameObject.SetActive(true);
                var o = __instance.mBtnCSMMiniGameBack;
                __instance.mBtnCSMMiniGameBack = e.BtnBack;
                __instance.RepositionItems(e.PlayPanelUI.gameObject);
                __instance.mBtnCSMMiniGameBack = o;
            }
        }

        [HarmonyPatch("Close")]
        [HarmonyPrefix]
        static void OnClose(UiAvatarCSM __instance)
        {
            var e = ExtendedCSM.Get(__instance);
            if (e.PlayPanelUI && e.PlayPanelUI.GetVisibility())
            {
                e.PlayPanelUI.gameObject.SetActive(false);
                e.PlayPanelUI.SetVisibility(false);
            }
        }
    }

    [HarmonyPatch(typeof(KAUIPetPlaySelectMenu), "SelectItem")]
    static class Patch_SelectPlayItem
    {
        static bool Prefix(KAUIPetPlaySelectMenu __instance, KAWidget item)
        {
            if (__instance.mPetUI.pObjectInHand && __instance.GetType((item.GetUserData() as KAUISelectItemData)._ItemData) == __instance.mPetUI.pObjectinHandType)
            {
                __instance.mPetUI.DropObject(false);
                return false;
            }
            return true;
        }
    }

    public class Tracker : MonoBehaviour
    {
        public MonoBehaviour[] tracked;
    }

    public class ExtendedCSM
    {
        static ConditionalWeakTable<UiAvatarCSM, ExtendedCSM> extra = new ConditionalWeakTable<UiAvatarCSM, ExtendedCSM>();
        public static ExtendedCSM Get(UiAvatarCSM instance) => extra.GetOrCreateValue(instance);

        public KAButton BtnPlay;
        public KAUI PlayPanelUI;
        public KAButton BtnBack;
        public Dictionary<KAButton, (string bundle,Func<string> petArea)> Options = new Dictionary<KAButton, (string, Func<string>)>();
    }

    [HarmonyPatch(typeof(PetPlayAreaLoader),"Load")]
    static class Patch_LoadPetPlayArea
    {
        static bool Prefix()
        {
            if (string.IsNullOrEmpty(PetPlayAreaLoader._PetPlayAreaResourceName))
            {
                UtDebug.LogError("Asset name not set : _PetPlayAreaResourceName is null or empty.");
                return false;
            }
            if (RsResourceManager.pCurrentLevel.Contains(GameConfig.GetKeyData("PetPlayScene")))
            {
                LoadPetPlayArea();
            }
            else
                UtDebug.LogError("invalid name of prefab");
            return false;
        }

        public static void LoadPetPlayArea()
        {
            KAUICursorManager.SetDefaultCursor("Loading", true);
            AvAvatar.SetUIActive(false);
            AvAvatar.pState = AvAvatarState.PAUSED;
            string[] array = PetPlayAreaLoader._PetPlayAreaResourceName.Split(new char[] { '/' });
            RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], (url, load, progress, loaded, data) =>
            {
                if (load == RsResourceLoadEvent.COMPLETE)
                {
                    var gameObject = Object.Instantiate((GameObject)loaded);
                    gameObject.name = RsResourceManager.pCurrentLevel;
                    if (gameObject)
                    {
                        SnChannel.StopPool("VO_Pool");
                        Input.ResetInputAxes();
                        gameObject.SetActive(true);
                        gameObject.GetComponentInChildren<KAUIPetPlaySelect>()?.SetPet(SanctuaryManager.pCurPetInstance);
                        gameObject.GetComponentInChildren<EBGameManager>()?.Initialize(SanctuaryManager.pCurPetInstance);
                    }
                    KAUICursorManager.SetDefaultCursor("Arrow", true);
                    RsResourceManager.DestroyLoadScreen();
                    return;
                }
                if (load != RsResourceLoadEvent.ERROR)
                    return;
                AvAvatar.pState = AvAvatarState.IDLE;
                AvAvatar.SetUIActive(true);
                KAUICursorManager.SetDefaultCursor("Arrow", true);
            }, typeof(GameObject), false, null);
        }
    }
    /*
    [HarmonyPatch(typeof(JSGames.UI.UI), "Awake")]
    static class Patch_UI_Awake
    {
        static void Prefix(JSGames.UI.UI __instance)
        {
            if (!__instance.GetComponent<RectTransform>())
            {
                var r = __instance.gameObject.AddComponent<RectTransform>();
                r.anchorMin = r.anchorMax = r.pivot = Vector2.zero;
                r.offsetMin = -(r.offsetMax = new Vector2(700, 437.5f));
                //r.localScale = Vector3.zero;
                var a = r.Find("Anchor-Center");
                var r2 = a.GetOrAddComponent<RectTransform>();
                r2.anchorMin = r2.anchorMax = r2.pivot = Vector2.one * 0.5f;

            }
            if (!__instance.GetComponent<Canvas>())
            {
                var c = __instance.gameObject.AddComponent<Canvas>();
                c.worldCamera = Camera.main;
                c.scaleFactor = 1;
                c.planeDistance = 100;
                var s = __instance.gameObject.GetOrAddComponent<UnityEngine.UI.CanvasScaler>();
                s.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                s.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
                s.matchWidthOrHeight = 0;
                s.referencePixelsPerUnit = 100;
                s.referenceResolution = new Vector2(1400, 875);
                s.scaleFactor = 1;
            }
        }
    }

    [HarmonyPatch(typeof(JSGames.UI.UI), "CacheWidgets")]
    static class Patch_CursorManager_CacheWidgets
    {
        static void Postfix(JSGames.UI.UI __instance, Transform cacheTransform)
        {
            if (__instance is KAUICursorManager && __instance.transform == cacheTransform)
            {
                if (__instance.mChildWidgets.Count == 0)
                    __instance.mChildWidgets.AddRange(__instance.GetComponentsInChildren<KAWidget>().Where(x => !x.transform.parent?.GetComponent<KAWidget>()).Select(x =>
                    {
                        var w = x.GetOrAddComponent<JSGames.UI.UIWidget>();
                        //w.pVisible = false; // disabled until i fix the weird mouse offset
                        return w;
                    }));
            }
                
        }
    }*/

    [HarmonyPatch(typeof(UtWWWAsync))]
    static class Patch_WWWLoadBundle
    {
        [HarmonyPatch("LoadBundle")]
        [HarmonyPrefix]
        static bool LoadBundle(string url, Hash128 hash, UtWWWEventHandler callback, bool sendProgressEvents, bool disableCache, bool downloadOnly, ref UtIWWWAsync __result)
        {
            var array = url.Split(new[] { '/' });
            if (Main.BundleOverrides.TryGetValue((array[array.Length - 2] + '/' + array[array.Length - 1]).ToLowerInvariant(), out var bundle))
            {
                bundle.fullUrl = url;
                __result = bundle;
                bundle.DownloadBundle(url, hash, callback, sendProgressEvents, disableCache, downloadOnly);
                return false;
            }
            return true;
        }
    }

    public class LoadAsmBundle : UtIWWWAsync
    {
        public AssetBundle bundle;
        public string url;
        public string resource;
        public string fullUrl;
        void UtIWWWAsync.Download(string inURL, RsResourceType inType, UtWWWEventHandler inCallback, bool inSendProgressEvents, bool inDisableCache, bool inDownLoadOnly, bool inIgnoreAssetVersion) { }
        public void DownloadBundle(string url, Hash128 hash, UtWWWEventHandler callback, bool sendProgressEvents, bool disableCache, bool downloadOnly)
        {
            if (!bundle)
                using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    bundle = AssetBundle.LoadFromStream(s);
                }
            callback?.Invoke(UtAsyncEvent.COMPLETE, this);
        }
        void UtIWWWAsync.Kill() { }
        void UtIWWWAsync.OnSceneLoaded(string inLevel) { }
        AssetBundle UtIWWWAsync.pAssetBundle => bundle;
        AudioClip UtIWWWAsync.pAudioClip => null;
        byte[] UtIWWWAsync.pBytes => null;
        string UtIWWWAsync.pData => null;
        string UtIWWWAsync.pError => null;
        bool UtIWWWAsync.pFromCache => true;
        bool UtIWWWAsync.pIsDone => true;
        void UtIWWWAsync.PostForm(string inURL, WWWForm inForm, UtWWWEventHandler inCallback, bool inSendProgressEvents) { }
        float UtIWWWAsync.pProgress => 1;
        RsResourceType UtIWWWAsync.pResourcetype => RsResourceType.ASSET_BUNDLE;
        Texture UtIWWWAsync.pTexture => null;
        string UtIWWWAsync.pURL => fullUrl;
        UnityEngine.Networking.UnityWebRequest UtIWWWAsync.pWebRequest => null;
        bool UtIWWWAsync.Update() => false;
    }
    
    [HarmonyPatch(typeof(RsResourceManager))]
    static class Patch_RsResourceManager
    {
        [HarmonyPatch("LoadAssetFromBundle", typeof(string), typeof(string), typeof(RsResourceEventHandler), typeof(Type), typeof(bool), typeof(object))]
        [HarmonyPrefix]
        static bool LoadAssetFromBundleAsync(string inBundleURL, string inAssetName, ref RsResourceEventHandler inCallback, Type inType, bool inDontDestroy, object inUserData)
        {
            var url = RsResourceManager.FormatBundleURL(inBundleURL).ToLowerInvariant();
            if (url.StartsWith("rs_"))
                url = url.Remove(0, 3);
            if (Main.BundleOverrides.TryGetValue(url,out var bundle))
            {
                bundle.DownloadBundle(null,default,null,false,false,false);
                inCallback?.Invoke(RsResourceManager.FormatBundleURL(inBundleURL) + "/" + inAssetName, RsResourceLoadEvent.COMPLETE, 1, bundle.bundle.LoadAsset(inAssetName,inType), inUserData);
                return false;
            }
            return true;
        }
        /*
        [HarmonyPatch("LoadAssetFromBundle", typeof(string), typeof(string), typeof(Type))]
        [HarmonyPrefix]
        static bool LoadAssetFromBundle_Prefix(string inBundlePath, string inAssetName, Type inType, ref object __result)
        {
            if (Main.BundleOverrides.TryGetValue(inBundlePath.ToLowerInvariant(), out var bundle))
            {
                __result = bundle.LoadAsset(inAssetName, inType);
                return false;
            }
            return true;
        }

        [HarmonyPatch("Load")]
        [HarmonyPrefix]
        static bool Load(string inURL, RsResourceEventHandler inCallback, object inUserData, ref object __result)
        {
            if (Main.BundleOverrides.TryGetValue(inURL.ToLowerInvariant(), out var bundle))
            {
                inCallback(RsResourceManager.FormatBundleURL(inURL), RsResourceLoadEvent.COMPLETE, 1, bundle, inUserData);
                __result = null;
                return false;
            }
            return true;
        }*/
    }
}
