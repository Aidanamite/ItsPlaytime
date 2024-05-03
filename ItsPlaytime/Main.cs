using BepInEx;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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
using Debug = UnityEngine.Debug;
using Label = System.Reflection.Emit.Label;
using UnityEngine.UIElements;
using Microsoft.Cci;
using static UnityEngine.Networking.UnityWebRequest;
using System.Runtime.Serialization;
using ShatterToolkit;
using System.Security.Permissions;

namespace ItsPlaytime
{
    [BepInPlugin("com.aidanamite.ItsPlaytime", "It's Playtime", "1.1.0")]
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
        public static Main instance;
        public void Awake()
        {
            instance = this;
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

        public static (ExperimentType lab, string name)[] Labs = new[]
        {
            (ExperimentType.NORMAL, "Basic Lab"),
            (ExperimentType.GRONCKLE_IRON, "Gronkle Iron Lab"),
            (ExperimentType.SKRILL_LAB, "Skrill Lab"),
            (ExperimentType.MAGNETISM_LAB, "Magnetism Lab"),
            (ExperimentType.SPECTRUM_LAB, "Spectrum Lab"),
            (ExperimentType.TITRATION_LAB, "Titration Lab")
        };
        void OnConfirmFreeplay()
        {
            KAUICursorManager.SetDefaultCursor("Loading", true);
            UiNameSuggestion.mSuggestedNames = Labs.Select(x => x.name).ToArray();
            UiNameSuggestion.mNameSelectedCallback = y =>
            {
                if (string.IsNullOrEmpty(y))
                {
                    OnPopupClose();
                    return;
                }
                Patch_GetLabExperimentByID.Freeplay.UpdateFreeplayMode(Labs.FirstOrDefault(x => x.name == y).lab);
                var use = Patch_GetLabExperimentByID.Freeplay.CanUsePet(SanctuaryManager.pCurPetInstance);
                if (use != ExperimentSuitable.Suitable)
                {
                    GameUtilities.DisplayOKMessage("PfKAUIGenericDB", "Your dragon " + (
                        use == ExperimentSuitable.Species
                        ? "is not a suitable species"
                        : use == ExperimentSuitable.Breath
                        ? Patch_GetLabExperimentByID.Freeplay.BreathType == (int)WeaponTuneData.AmmoType.FIRE ? "must breath fire" : "must breath lightning"
                        : use == ExperimentSuitable.Age
                        ? "is too young"
                        : use == ExperimentSuitable.Missing
                        ? "must exist"
                        : "is not suitable"
                        ) + " for the " + y, gameObject, "OnPopupClose", true);
                }
                else if (Patch_GetLabExperimentByID.Freeplay.Items == null)
                    LabData.Load(new LabData.XMLLoaderCallback(x =>
                    {
                        if (x)
                        {
                            OnPopupClose();
                            ScientificExperiment.pActiveExperimentID = Patch_GetLabExperimentByID.Freeplay.ID;
                            Patch_InitializeExperiment.UseExperimentID = true;
                            labPortal.Item1.SendMessage("LoadLab");
                        }
                        else
                            GameUtilities.DisplayOKMessage("PfKAUIGenericDB", "Lab data failed to load", gameObject, "OnPopupClose", true);
                    }));
                else
                {
                    OnPopupClose();
                    ScientificExperiment.pActiveExperimentID = Patch_GetLabExperimentByID.Freeplay.ID;
                    Patch_InitializeExperiment.UseExperimentID = true;
                    labPortal.Item1.SendMessage("LoadLab");
                }
            };
            RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("NameSuggestionAsset"), (a,b,c,d,e) =>
            {
                if (b == RsResourceLoadEvent.ERROR)
                {
                    UiNameSuggestion.mSuggestedNames = null;
                    UiNameSuggestion.mNameSelectedCallback = null;
                    GameUtilities.DisplayOKMessage("PfKAUIGenericDB", "Unable to load selection UI", gameObject, "OnPopupClose", true);
                }
                else if (b == RsResourceLoadEvent.COMPLETE)
                {
                    KAUICursorManager.SetDefaultCursor("Arrow", true);
                    var ui = Instantiate((GameObject)d).GetComponent<UiNameSuggestion>();
                    ui.FindItem("TxtTitle").SetText("Lab Select");//change ui title and text
                    ui.FindItem("TxtSubHeading").SetText("Which lab would you like to use?");
                }
            }, typeof(GameObject), false, null);

            
        }
        public static (LabPortalTrigger, Collider) labPortal;
        void OnConfirmNotFreeplay()
        {
            Patch_InitializeExperiment.UseExperimentID = false;
            OnPopupClose();
            Patch_TryEnterLab.allow = true;
            try
            {
                labPortal.Item1.OnTriggerEnter(labPortal.Item2);
            }
            finally
            {
                Patch_TryEnterLab.allow = false;
                labPortal = default;
            }
        }
        void OnPopupClose()
        {
            AvAvatar.pState = AvAvatarState.IDLE;
            AvAvatar.SetUIActive(true);
        }
        void DoNothing() { }
    }

    public static class ExtentionMethods
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component => gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        public static T GetOrAddComponent<T>(this Component component) where T : Component => component.GetComponent<T>() ?? component.gameObject.AddComponent<T>();

        public static T MemberwiseClone<T>(this T obj)
        {
            if (obj == null)
                return obj;
            var t = obj.GetType();
            var nObj = (T)FormatterServices.GetUninitializedObject(t);
            var b = typeof(object);
            while (t != b)
            {
                foreach (var f in t.GetFields(~BindingFlags.Default))
                    if (!f.IsStatic)
                        f.SetValue(nObj, f.GetValue(obj));
                t = t.BaseType;
            }
            return nObj;
        }

        public static T DeepMemberwiseClone<T>(this T obj) => obj.DeepMemberwiseClone(new Dictionary<object, object>(), new HashSet<object>());
        static T DeepMemberwiseClone<T>(this T obj, Dictionary<object, object> cloned, HashSet<object> created)
        {
            if (obj == null)
                return obj;
            if (cloned.TryGetValue(obj, out var clone))
                return (T)clone;
            if (created.Contains(obj))
                return obj;
            var t = obj.GetType();
            if (t.IsPrimitive || t == typeof(string))
                return obj;
            if (t.IsArray && obj is Array a)
            {
                var c = t.GetConstructors()[0];
                var o = new object[t.GetArrayRank()];
                for (int i = 0; i < o.Length; i++)
                    o[i] = a.GetLength(i);
                var na = (Array)c.Invoke(o);
                created.Add(na);
                cloned[a] = na;
                for (int i = 0; i < o.Length; i++)
                    if ((int)o[i] == 0)
                        return (T)(object)na;
                var ind = new int[o.Length];
                var flag = true;
                while (flag)
                {
                    na.SetValue(a.GetValue(ind).DeepMemberwiseClone(cloned, created), ind);
                    for (int i = 0; i < ind.Length; i++)
                    {
                        ind[i]++;
                        if (ind[i] == (int)o[i])
                        {
                            if (i == ind.Length - 1)
                                flag = false;
                            ind[i] = 0;
                        }
                        else
                            break;
                    }
                }
                return (T)(object)na;
            }
            var nObj = (T)FormatterServices.GetUninitializedObject(t);
            created.Add(nObj);
            cloned[obj] = nObj;
            var b = typeof(object);
            while (t != b)
            {
                foreach (var f in t.GetFields(~BindingFlags.Default))
                    if (!f.IsStatic)
                        f.SetValue(nObj, f.GetValue(obj).DeepMemberwiseClone(cloned, created));
                t = t.BaseType;
            }
            return nObj;
        }

        public static Y GetOrCreate<X,Y>(this IDictionary<X,Y> d, X value) where Y : new()
        {
            if (d.TryGetValue(value, out var r))
                return r;
            return d[value] = new Y();
        }

        public static Y GetOrDefault<X, Y>(this IDictionary<X, Y> d, X value, Y fallback = default)
        {
            if (d.TryGetValue(value, out var r))
                return r;
            return fallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<T, int> Counts<T>(this IEnumerable<T> c) => c.Counts(x => x);
        public static Dictionary<Y,int> Counts<X,Y>(this IEnumerable<X> c, Func<X,Y> cast)
        {
            var d = new Dictionary<Y, int>();
            if (c != null)
                foreach (var i in c)
                {
                    var n = cast(i);
                    d.TryGetValue(n, out var r);
                    d[n] = r + 1;
                }
            return d;
        }
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
    }

    [HarmonyPatch(typeof(LabPortalTrigger),"OnTriggerEnter")]
    static class Patch_TryEnterLab
    {
        public static bool allow = false;
        static bool Prefix(LabPortalTrigger __instance, Collider inCollider)
        {
            if (allow)
                return true;
            if (!AvAvatar.IsCurrentPlayer(inCollider.gameObject))
                return false;
            AvAvatar.SetUIActive(false);
            AvAvatar.pState = AvAvatarState.PAUSED;
            var ui = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Entering Lab");
            ui.SetButtonVisibility(true, true, false, true);
            Main.labPortal = (__instance,inCollider);
            ui.SetMessage(Main.instance.gameObject, "OnConfirmFreeplay", "OnConfirmNotFreeplay", null, "OnPopupClose");
            ui.SetDestroyOnClick(true);
            ui.SetButtonLabel("YesBtn", "Freeplay");
            ui.SetButtonLabel("NoBtn", "Experiment");
            ui.SetText("Would you like to use the lab in freeplay or conduct an experiment?",true);
            KAUI.SetExclusive(ui, true);
            return false;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            code.Insert(code.FindIndex(x => x.opcode == OpCodes.Ldsfld && x.operand is FieldInfo f && f.Name == "pUseExperimentCheat") + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_InitializeExperiment), "UseId")));
            return code;
        }
    }

    [HarmonyPatch(typeof(ScientificExperiment),"Initialize")]
    static class Patch_InitializeExperiment
    {
        public static bool UseExperimentID = false;
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            code.Insert(code.FindIndex(x => x.opcode == OpCodes.Ldsfld && x.operand is FieldInfo f && f.Name == "pUseExperimentCheat") + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_InitializeExperiment), nameof(UseId))));
            var ind = code.FindIndex(code.FindIndex(x => x.opcode == OpCodes.Ldfld && x.operand is FieldInfo f && f.Name == "mCreatedDragon"), x => x.opcode.FlowControl == FlowControl.Cond_Branch) + 1;
            code.RemoveRange(ind, code.FindIndex(ind, x => x.labels.Contains((Label)code[ind - 1].operand)) - ind);
            code.InsertRange(ind, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_InitializeExperiment),nameof(HandleDragonInitialize)))
            });
            return code;
        }
        static bool UseId(bool original) => original || UseExperimentID;
        static void HandleDragonInitialize(ScientificExperiment instance)
        {
            void PlaceDragonAside()
            {
                SanctuaryManager.pCurPetInstance.PlayAnimation("IdleSit", WrapMode.Loop, 1f, 0.2f);
                Transform dragonMarker = instance.GetDragonMarker(SanctuaryManager.pCurPetInstance.pTypeInfo._Name, SanctuaryManager.pCurPetInstance.pCurAgeData._Name, true);
                if (dragonMarker != null)
                {
                    SanctuaryManager.pCurPetInstance.SetPosition(dragonMarker.position);
                    SanctuaryManager.pCurPetInstance.transform.rotation = dragonMarker.rotation;
                    SanctuaryManager.pCurPetInstance.transform.localScale = Vector3.one * SanctuaryManager.pCurPetInstance.pCurAgeData._LabScale;
                }
            }
            void InitCustomGronckleExp()
            {
                instance._Gronckle.gameObject.SetActive(true);
                instance._Gronckle.Init(instance);
                var component = instance._Gronckle.GetComponent<SanctuaryPet>();
                component.Init(SanctuaryManager.pCurPetInstance.pData, false, null);
                instance.SetCurrentDragon(component);
            }
            if (SanctuaryManager.pCurPetData != null && !SanctuaryManager.pCurPetInstance)
                return;
            var use = instance.mExperiment.CanUsePet(SanctuaryManager.pCurPetInstance);
            if (use == ExperimentSuitable.Suitable)
            {
                if (instance.mExperiment.Type == (int)ExperimentType.GRONCKLE_IRON)
                    InitCustomGronckleExp();
                else
                    instance.SetCurrentDragon(SanctuaryManager.pCurPetInstance);
            }
            else
            {
                if (use != ExperimentSuitable.Missing && SanctuaryManager.pCurPetInstance.pAge == 0)
                    PlaceDragonAside();
                if (instance.mExperiment.Type == (int)ExperimentType.GRONCKLE_IRON)
                    instance.InitGronckleExp();
                else
                    instance.CreateDragon(instance.mExperiment.DragonType, instance.mExperiment.DragonStage, instance.mExperiment.DragonGender);
            }
            instance.mCreatedDragon = true;
        }
        public static ExperimentSuitable CanUsePet(this Experiment experiment, SanctuaryPet pet)
        {
            if (!pet)
                return ExperimentSuitable.Missing;
            if (experiment != null)
            {
                if (experiment.Type == (int)ExperimentType.GRONCKLE_IRON && pet.pData.PetTypeID != 13)
                    return ExperimentSuitable.Species;
                if (experiment.ForceDefaultDragon && pet.pData.PetTypeID != experiment.DragonType)
                    return ExperimentSuitable.Species;
                if (experiment.BreathType != (int)pet.pWeaponManager.GetCurrentWeapon()._AmmoType && !(experiment.BreathType == (int)WeaponTuneData.AmmoType.FIRE && pet.pWeaponManager.GetCurrentWeapon()._AmmoType == WeaponTuneData.AmmoType.ELECTRIC))
                    return ExperimentSuitable.Breath;
            }
            if (pet.pAge == 0)
                return ExperimentSuitable.Age;
            return ExperimentSuitable.Suitable;
        }
        public static void UpdateFreeplayMode(this Experiment exp, ExperimentType type)
        {
            exp.Type = (int)type;
            exp.BreathType = (int)(type == ExperimentType.SKRILL_LAB ? WeaponTuneData.AmmoType.ELECTRIC : WeaponTuneData.AmmoType.FIRE);
        }
    }

    public enum ExperimentSuitable
    {
        Suitable,
        Species,
        Breath,
        Age,
        Missing
    }

    [HarmonyPatch(typeof(RsResourceManager), "LoadAssetFromBundle", typeof(string), typeof(string), typeof(RsResourceEventHandler), typeof(Type), typeof(bool), typeof(object))]
    static class Patch_LabItemLoad
    {
        static void Prefix(ref RsResourceEventHandler inCallback)
        {
            if (!ScientificExperiment.pInstance)
                return;
            var original = inCallback;
            inCallback = (a, b, c, d, e) =>
            {
                if (b == RsResourceLoadEvent.ERROR)
                {
                    b = RsResourceLoadEvent.COMPLETE;
                    d = null;
                }
                original?.Invoke(a, b, c, d, e);
            };
        }
    }

    [HarmonyPatch(typeof(RsResourceManager), "Load")]
    static class Patch_LogAsset
    {
        static void Prefix(ref RsResourceEventHandler inCallback)
        {
            if (!ScientificExperiment.pInstance)
                return;
            var original = inCallback;
            inCallback = (a, b, c, d, e) =>
            {
                if (b == RsResourceLoadEvent.ERROR)
                {
                    b = RsResourceLoadEvent.COMPLETE;
                    d = null;
                }
                original?.Invoke(a, b, c, d, e);
            };
        }
    }

    [HarmonyPatch(typeof(ScientificExperiment), "SetCurrentDragon")]
    static class Patch_SetExperimentDragon
    {
        static void Postfix(ScientificExperiment __instance)
        {
            if (__instance.mCurrentDragon && Patch_GetDragonLabData.unsupported.Contains(__instance.mCurrentDragon.pTypeInfo._Name))
                GameUtilities.DisplayOKMessage("PfKAUIGenericDB", "The dragon you're using is unsupported. You may notice some bugs.\n\nSorry :(", Main.instance.gameObject, "DoNothing", true);
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            code.InsertRange(code.FindIndex(x => x.opcode == OpCodes.Ldfld && x.operand is FieldInfo f && f.Name == "_PetSkinMapping") + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_SetExperimentDragon),nameof(GetMapping)))
            });
            return code;
        }

        static List<ScientificExperiment.PetSkinMapping> GetMapping(List<ScientificExperiment.PetSkinMapping> original, ScientificExperiment instance) => instance.mCurrentDragon == SanctuaryManager.pCurPetInstance ? null : original;
    }

    [HarmonyPatch(typeof(LabData))]
    static class Patch_GetLabExperimentByID
    {
        public static Experiment Freeplay = new Experiment()
        {
            ID = -0xABCDE,
            Name = "Freeplay",
            Tasks = new LabTask[0],
            ThermometerMax = 5000,
            ThermometerMin = -5000
        };

        [HarmonyPatch("GetLabExperimentByID")]
        static bool Prefix(int inID, ref Experiment __result)
        {
            if (inID != Freeplay.ID)
                return true;
            __result = Freeplay;
            return false;
        }

        [HarmonyPatch("Initialize")]
        static void Postfix(LabData __instance)
        {
            var h = new Dictionary<string,string>();
            foreach (var i in __instance.Items)
            {
                if (i?.Name == "FirewoodAsh")
                    i.Icon = "RS_DATA/CollectDWIcons.unity3d/IcoCollectDWHempFiberPile"; // Fix for an item trying to load an invalid icon
                if (!string.IsNullOrEmpty(i?.Name) && !string.IsNullOrEmpty(i.DisplayNameText?.GetLocalizedString()) && !string.IsNullOrWhiteSpace(i.Icon) && !string.IsNullOrEmpty(i.Prefab) && i.Prefab.ToLowerInvariant() != "RS_DATA/PfMBLiquid.unity3d/PfMBLiquid".ToLowerInvariant())
                    h[i.Name] = i.DisplayNameText.GetLocalizedString();
            }

            var sorted = new SortedDictionary<string, string>(new NeverEqual<string>());
            foreach (var p in h)
                sorted[p.Value] = p.Key;
            /*{
                var valid = new HashSet<string>();
                var items = new SortedDictionary<string, LabItemHolder>(new NeverEqual<string>());
                foreach (var i in __instance.Items)
                    if (!string.IsNullOrEmpty(i?.Name) && !string.IsNullOrEmpty(i.Prefab) && valid.Add(i.Name))
                    {
                        var v = new LabItemHolder(i);
                        v.InInventory = h.ContainsKey(v.Id);
                        items[v.Name] = v;
                    }
                var combos = new HashSet<LabCombinationHolder>();
                foreach (var cl in __instance.pCombinations)
                    if (cl.Value != null)
                        foreach (var c in cl.Value)
                            if (!string.IsNullOrEmpty(c.ResultItemName) && valid.Contains(c.ResultItemName) && c.ItemNames != null && c.ItemNames.All(x => !string.IsNullOrEmpty(x) && valid.Contains(x)))
                                combos.Add(new LabCombinationHolder(c));
                Debug.Log($"Freeplay item count: {h.Count}\n{Newtonsoft.Json.JsonConvert.SerializeObject(new ArrayHolder<LabItemHolder>() { array = items.Values.ToArray() })}\n{Newtonsoft.Json.JsonConvert.SerializeObject(new ArrayHolder<LabCombinationHolder>() { array = combos.ToArray() })}");
            }*/
            Freeplay.Items = sorted.Values.ToArray();
        }
    }

    public class NeverEqual<T> : IComparer<T> where T : IComparable<T>
    {
        int IComparer<T>.Compare(T x, T y) => x.CompareTo(y) == 0 ? -1 : x.CompareTo(y);
    }

    [Serializable]
    public class ArrayHolder<T>
    {
        public T[] array;
    }

    [Serializable]
    public class LabItemHolder
    {
        public string Id;
        public string Name;
        public bool InInventory;
        public LabItemHolder(LabItem item)
        {
            Id = item.Name;
            Name = item.DisplayNameText?.GetLocalizedString();
        }
    }

    [Serializable]
    public class LabCombinationHolder
    {
        public string Action;
        public string[] Items;
        public string Result;
        public LabCombinationHolder(LabItemCombination combination)
        {
            Action = combination.Action;
            Items = combination.ItemNames;
            Result = combination.ResultItemName;
        }
        public override bool Equals(object obj)
        {
            if (obj is LabCombinationHolder c)
                return c.Action == Action && c.Items.SequenceEqual(Items) && c.Result == Result;
            return base.Equals(obj);
        }
        public override int GetHashCode() => Action.GetHashCode() ^ ~Result.GetHashCode() ^ Items.Length.GetHashCode();
    }

    [HarmonyPatch(typeof(UiScienceExperiment))]
    static class Patch_UiScienceExperiment
    {
        [HarmonyPatch("InitializeExperiment")]
        static void Prefix(UiScienceExperiment __instance)
        {
            __instance._ExperimentItemMenu._DefaultGrid.maxPerLine = 255;
        }
    }

    [HarmonyPatch(typeof(ScientificExperiment), "Initialize")]
    static class Patch_GetDragonLabData
    {
        static Dictionary<string, string> copy = new Dictionary<string, string>()
        {
            {"LightFuryGeneric","LightFury"},
            {"CrimsonHowler","WoollyHowl"},
            {"Zipplewraith","Zippleback"},
            {"Goregripper","DeathGripper"},
            {"Graveknapper","ScreamingDeath"},
            {"Abomibumble","Gronckle"},
            {"Galeslash","DeadlyNadder"},
            {"Ridgesnipper","Snafflefang"},
            {"Bonestormer","Boneknapper"},
            {"Chimeragon","Razorwhip"},
            {"Slitherwing","Slithersong"},
            {"Seastormer","Thunderdrum"},
            {"CavernCrasher","FlameWhipper"},
            {"Humbanger","WhisperingDeath"},
            {"GoldenDragon","Scauldron"},
            {"Hushboggle","Scuttleclaw"},
            {"Frostmare","Groncicle"},
            {"Songwing","GrimGnasher"},
            {"Sandbuster","Windwalker"},
            {"SwordStealer","ArmorWing"}
        };
        public static HashSet<string> unsupported = new HashSet<string>();
        static ScientificExperiment instance;
        static void Prefix(ScientificExperiment __instance)
        {
            if (instance == __instance)
                return;
            instance = __instance;
            unsupported = new HashSet<string>();
            foreach (var t in SanctuaryData.pInstance._PetTypes)
                unsupported.Add(t._Name);
            var l = __instance._DragonData.ToList();
            var d = new Dictionary<string, ScientificExperiment.LabDragonData>();
            foreach (var i in __instance._DragonData)
                if (unsupported.Remove(i._Name))
                    d[i._Name] = i;
            foreach (var i in unsupported)
            {
                var n = (copy.TryGetValue(i, out var t) ? d[t] : d["Nightmare"]).MemberwiseClone();
                n._Name = i;
                l.Add(n);
            }
            __instance._DragonData = l.ToArray();
        }
    }

    [HarmonyPatch(typeof(ScientificExperiment), "PlayDragonAnim", typeof(string), typeof(bool), typeof(bool), typeof(float), typeof(Transform))]
    static class Patch_PlayLabAnimation
    {
        static void Postfix(ScientificExperiment __instance,string inAnimName, bool inPlayOnce, bool playIdleNext, float animSpeed, Transform lookAtObject, ref bool __result)
        {
            if (!__result
                && __instance.mCurrentDragon != null
                && !string.IsNullOrEmpty(inAnimName)
                && __instance.mCurrentDragon.animation != null
                && __instance.mCurrentDragon.animation[inAnimName] == null
                && !__instance.pWaitingForAnimEvent)
            {
                string anim = null;
                if (inAnimName == "LabIdle")
                    anim = "IdleSit";
                else if (inAnimName == "LabApprovalResponce")
                    anim = "Celebrate";
                else if (inAnimName == "LabNegativeResponce")
                    anim = "Refuse";
                else if (inAnimName == "LabBlowFire")
                    anim = "Attack01";
                else if (inAnimName == "LabExcited")
                    anim = "IdleHappy";
                else if (inAnimName == "LabPullChain")
                    anim = "Gulp";
                if (anim != null && __instance.PlayDragonAnim(anim, inPlayOnce, playIdleNext, animSpeed, lookAtObject))
                    __instance.mCurrentDragon.StartCoroutine(PlayEvents( __instance.mDragonData._AnimEvents,inAnimName));
            }
        }

        static IEnumerator PlayEvents(AvAvatarAnimEvent[] events, string anim)
        {
            var sort = new List<(AvAvatarAnimEvent a, AnimData b)>();
            foreach (var a in events)
                if (a._Animation == anim)
                    foreach (var e in a._Times)
                        sort.Add((a, e));
            sort.Sort((a,b) => a.b._Time.CompareTo(b.b._Time));
            var p = 0;
            while (p < sort.Count)
            {
                var t = sort[p].b._Time - (p > 0 ? sort[p - 1].b._Time : 0);
                if (t > 0)
                    yield return new WaitForSeconds(t);
                sort[p].a.mData = sort[p].b;
                sort[p].a._Target.SendMessage(sort[p].a._Function, sort[p].a);
                p++;
            }
            yield break;
        }
    }
}
