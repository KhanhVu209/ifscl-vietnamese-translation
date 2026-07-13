using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace IFSCL.LocalizationPlugin
{
    [BepInPlugin("com.ifscl.vietnam.localization", "IFSCL Vietnam Localization", "1.2.6")]
    public class LocalizationPlugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        public static Font nativeFont;
        public static Font ubuntuFont;
        public static Font zektonFont;
        public static TMP_FontAsset vnFontAsset;
        public static TMP_FontAsset ubuntuFontAsset;
        public static TMP_FontAsset zektonFontAsset;
        public static TMP_FontAsset osFontAsset;
        public static readonly HashSet<int> PatchedFontInstanceIds = new HashSet<int>();

        void Awake()
        {
            Log = Logger;
            Log.LogInfo("=== [IFSCL Localization] Loader Awake (Version 1.2.6) ===");
            
            try
            {
                var harmony = new Harmony("com.ifscl.vietnam.localization");
                
                // 1. Hook tiền khởi tạo game
                var preInitMethod = AccessTools.Method(typeof(IFSCL.GamePreInit), "LoadAll");
                if (preInitMethod != null)
                {
                    harmony.Patch(preInitMethod, null, new HarmonyMethod(typeof(GamePreInitPatch), "Postfix"));
                    Log.LogInfo("  [✓ Hook OK] IFSCL.GamePreInit.LoadAll");
                }

                // 2. Hook cụ thể vào TextMeshPro (3D) OnEnable
                var onEnable3D = AccessTools.Method(typeof(TextMeshPro), "OnEnable");
                if (onEnable3D != null)
                {
                    harmony.Patch(onEnable3D, null, new HarmonyMethod(typeof(TMP3DOnEnablePatch), "Postfix"));
                    Log.LogInfo("  [✓ Hook OK] TMPro.TextMeshPro.OnEnable");
                }

                // 3. Hook cụ thể vào TextMeshProUGUI (Canvas UI) OnEnable
                var onEnableUI = AccessTools.Method(typeof(TextMeshProUGUI), "OnEnable");
                if (onEnableUI != null)
                {
                    harmony.Patch(onEnableUI, null, new HarmonyMethod(typeof(TMPUIOnEnablePatch), "Postfix"));
                    Log.LogInfo("  [✓ Hook OK] TMPro.TextMeshProUGUI.OnEnable");
                }

                // 4. Hook vào Setter của TMP_Text.text để bắt sự kiện thay đổi nội dung chữ
                var textSetter = AccessTools.PropertySetter(typeof(TMP_Text), "text");
                if (textSetter != null)
                {
                    harmony.Patch(textSetter, null, new HarmonyMethod(typeof(TMPTextSetTextPatch), "Postfix"));
                    Log.LogInfo("  [✓ Hook OK] TMPro.TMP_Text.set_text");
                }

                // 5. Hook vào bộ dịch cốt lõi của game để tự xử lý các khóa lỗi và tiêu đề menu chính
                var getTranslationMethod = AccessTools.Method(typeof(IFSCL.MSG), "GetTranslation");
                if (getTranslationMethod != null)
                {
                    harmony.Patch(getTranslationMethod, new HarmonyMethod(typeof(MSGGetTranslationPatch), "Prefix"));
                    Log.LogInfo("  [✓ Hook OK] IFSCL.MSG.GetTranslation");
                }
            }
            catch (Exception ex)
            {
                Log.LogError("  [✗ Hook Failed] Harmony initialization error: " + ex);
            }
        }
    }

    class MSGGetTranslationPatch
    {
        // Sử dụng tên tham số chính xác tuyệt đối theo game: database, mot, lang, notfound, shortError
        static bool Prefix(Translations.DB database, string mot, string lang, string notfound, bool shortError, ref string __result)
        {
            if (mot != null)
            {
                string idLower = mot.ToLower().Trim();
                
                // Fix lỗi NOTFOUND cho Options
                if (idLower == "options")
                {
                    __result = "CÀI ĐẶT";
                    return false; // Bỏ qua hàm gốc
                }
                
                // Fix lỗi NOTFOUND cho Quitter (Quit)
                if (idLower == "quitter")
                {
                    __result = "THOÁT GAME";
                    return false; // Bỏ qua hàm gốc
                }
                
                // Sửa chế độ cốt truyện theo yêu cầu
                if (idLower == "storymode")
                {
                    __result = "CỐT TRUYỆN";
                    return false; // Bỏ qua hàm gốc
                }
                
                // Sửa chế độ tự chọn theo yêu cầu
                if (idLower == "custommode")
                {
                    __result = "TỰ CHỌN";
                    return false; // Bỏ qua hàm gốc
                }
            }
            return true; // Chạy tiếp hàm dịch gốc cho các khóa khác
        }
    }

    class TMPTextSetTextPatch
    {
        static void Postfix(TMP_Text __instance)
        {
            if (ReferenceEquals(__instance, null)) return;

            try
            {
                string text = __instance.text;
                if (!string.IsNullOrEmpty(text) && 
                    (text.IndexOf("GIAO", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     text.IndexOf("hư cấu", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     text.IndexOf("CHIẾN", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    DiagnosticsHelper.LogTextDetails(__instance);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    class TMP3DOnEnablePatch
    {
        static void Postfix(TextMeshPro __instance)
        {
            if (ReferenceEquals(__instance, null)) return;
            
            try
            {
                var font = __instance.font;
                if (!ReferenceEquals(font, null))
                {
                    GamePreInitPatch.ApplyFallbackToFont(font);
                }
            }
            catch {}
        }
    }

    class TMPUIOnEnablePatch
    {
        static void Postfix(TextMeshProUGUI __instance)
        {
            if (ReferenceEquals(__instance, null)) return;
            
            try
            {
                var font = __instance.font;
                if (!ReferenceEquals(font, null))
                {
                    GamePreInitPatch.ApplyFallbackToFont(font);
                }
            }
            catch {}
        }
    }

    class DiagnosticsHelper
    {
        private static readonly HashSet<string> LoggedTexts = new HashSet<string>();

        public static void LogTextDetails(TMP_Text txt)
        {
            if (ReferenceEquals(txt, null)) return;

            try
            {
                string txtContent = txt.text;
                if (LoggedTexts.Contains(txtContent)) return;
                LoggedTexts.Add(txtContent);

                LocalizationPlugin.Log.LogInfo(string.Format("=== [TMP_Text Material Diagnostics] Text: '{0}' ===", txtContent));
                
                var font = txt.font;
                LocalizationPlugin.Log.LogInfo(string.Format("  * FontAsset: {0}", !ReferenceEquals(font, null) ? font.name : "NULL"));
                
                var sharedMat = txt.fontSharedMaterial;
                if (!ReferenceEquals(sharedMat, null))
                {
                    LocalizationPlugin.Log.LogInfo(string.Format("  * fontSharedMaterial: {0} | Shader: {1}", 
                        sharedMat.name, 
                        !ReferenceEquals(sharedMat.shader, null) ? sharedMat.shader.name : "NULL"));
                }

                var instMat = txt.fontMaterial;
                if (!ReferenceEquals(instMat, null))
                {
                    LocalizationPlugin.Log.LogInfo(string.Format("  * fontMaterial: {0} | Shader: {1}", 
                        instMat.name, 
                        !ReferenceEquals(instMat.shader, null) ? instMat.shader.name : "NULL"));
                }

                var sprite = txt.spriteAsset;
                LocalizationPlugin.Log.LogInfo(string.Format("  * spriteAsset: {0}", !ReferenceEquals(sprite, null) ? sprite.name : "NULL"));
                LocalizationPlugin.Log.LogInfo(string.Format("  * isTextObjectScaleStatic: {0}", txt.isTextObjectScaleStatic));

                txt.ForceMeshUpdate();
                TMP_TextInfo info = txt.textInfo;
                if (info == null) return;

                LocalizationPlugin.Log.LogInfo(string.Format("  * Char Count: {0}", info.characterCount));
                for (int i = 0; i < info.characterCount; i++)
                {
                    TMP_CharacterInfo cInfo = info.characterInfo[i];
                    if (cInfo.isVisible)
                    {
                        string fName = cInfo.fontAsset != null ? cInfo.fontAsset.name : "NULL";
                        string mName = cInfo.material != null ? cInfo.material.name : "NULL";
                        string sName = (cInfo.material != null && cInfo.material.shader != null) ? cInfo.material.shader.name : "NULL";
                        
                        object cInfoObj = (object)cInfo;
                        object gIndexVal = GetPropertyValue(cInfoObj, "glyphIndex") ?? GetFieldValue(cInfoObj, "m_GlyphIndex") ?? GetFieldValue(cInfoObj, "glyphIndex") ?? "N/A";

                        LocalizationPlugin.Log.LogInfo(string.Format("    [{0}] '{1}' (U+{2:X4}) -> FontAsset: {3} | Material: {4} | Shader: {5} | GlyphIndex: {6}",
                            i, cInfo.character, (int)cInfo.character, fName, mName, sName, gIndexVal));
                    }
                }
                LocalizationPlugin.Log.LogInfo("==================================================");
            }
            catch (Exception ex)
            {
                LocalizationPlugin.Log.LogError("[LogTextDetails] Error: " + ex);
            }
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return prop != null ? prop.GetValue(obj, null) : null;
            }
            catch { return null; }
        }

        private static object GetFieldValue(object obj, string fieldName)
        {
            try
            {
                var field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return field != null ? field.GetValue(obj) : null;
            }
            catch { return null; }
        }
    }

    class GamePreInitPatch
    {
        private const string VI_CHARS = "àáâãèéêìíòóôõùúýăđơưạảấầẩẫậắặằẳẵặẹẻẽệếềểễịỉĩịọỏốồổỗộớờởỡợụủũứừửữựỳỷỹỵÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝĂĐƠƯẠẢẤẦẨẪẬẮẶẰẲẴẶẸẺẼỆẾỀỂỄỊỈĨỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦŨỨỪỬỮỰỲỶỸỴ";

        public static void Postfix()
        {
            LocalizationPlugin.Log.LogInfo("=== [IFSCL Localization] GamePreInit.LoadAll POSTFIX called! ===");
            
            EnsureFallbackInitialized();
            EnsureUbuntuFallbackInitialized();
            EnsureZektonFallbackInitialized();
            EnsureOSFallbackInitialized();
            
            ApplyFallbackToAllFonts();
            
            RunDiagnostics();
        }

        private static void EnsureFallbackInitialized()
        {
            if (LocalizationPlugin.vnFontAsset != null) return;

            try
            {
                string fontPath = Path.Combine(Paths.GameRootPath, "Gunship IFSCL.ttf");
                if (File.Exists(fontPath))
                {
                    LocalizationPlugin.nativeFont = new Font(fontPath);
                    LocalizationPlugin.nativeFont.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    LocalizationPlugin.nativeFont.RequestCharactersInTexture(VI_CHARS, 24, FontStyle.Normal);

                    LocalizationPlugin.vnFontAsset = TMP_FontAsset.CreateFontAsset(LocalizationPlugin.nativeFont);
                    if (LocalizationPlugin.vnFontAsset != null)
                    {
                        LocalizationPlugin.vnFontAsset.name = "Gunship_VietHoa";
                        LocalizationPlugin.vnFontAsset.hideFlags = HideFlags.DontUnloadUnusedAsset;

                        SetFontAssetDynamic(LocalizationPlugin.vnFontAsset);
                        
                        bool added = LocalizationPlugin.vnFontAsset.TryAddCharacters(VI_CHARS);
                        LocalizationPlugin.Log.LogInfo("[IFSCL Localization] Force-added VI characters to Gunship: " + added);

                        RegisterGlobalFallback(LocalizationPlugin.vnFontAsset);
                    }
                }
                else
                {
                    LocalizationPlugin.Log.LogError("[IFSCL Localization] Gunship font file not found at game root: " + fontPath);
                }
            }
            catch (Exception ex)
            {
                LocalizationPlugin.Log.LogError("[IFSCL Localization] Failed to initialize fallback font: " + ex);
            }
        }

        private static void EnsureUbuntuFallbackInitialized()
        {
            if (LocalizationPlugin.ubuntuFontAsset != null) return;

            try
            {
                string fontPath = Path.Combine(Paths.GameRootPath, "Ubuntu-R.ttf");
                if (File.Exists(fontPath))
                {
                    LocalizationPlugin.ubuntuFont = new Font(fontPath);
                    LocalizationPlugin.ubuntuFont.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    LocalizationPlugin.ubuntuFont.RequestCharactersInTexture(VI_CHARS, 24, FontStyle.Normal);

                    LocalizationPlugin.ubuntuFontAsset = TMP_FontAsset.CreateFontAsset(LocalizationPlugin.ubuntuFont);
                    if (LocalizationPlugin.ubuntuFontAsset != null)
                    {
                        LocalizationPlugin.ubuntuFontAsset.name = "Ubuntu_VietHoa";
                        LocalizationPlugin.ubuntuFontAsset.hideFlags = HideFlags.DontUnloadUnusedAsset;

                        SetFontAssetDynamic(LocalizationPlugin.ubuntuFontAsset);
                        
                        bool added = LocalizationPlugin.ubuntuFontAsset.TryAddCharacters(VI_CHARS);
                        LocalizationPlugin.Log.LogInfo("[IFSCL Localization] Force-added VI characters to Ubuntu: " + added);

                        RegisterGlobalFallback(LocalizationPlugin.ubuntuFontAsset);
                    }
                }
                else
                {
                    LocalizationPlugin.Log.LogError("[IFSCL Localization] Ubuntu-R font file not found at game root: " + fontPath);
                }
            }
            catch (Exception ex)
            {
                LocalizationPlugin.Log.LogError("[IFSCL Localization] Failed to initialize Ubuntu fallback font: " + ex);
            }
        }

        private static void EnsureZektonFallbackInitialized()
        {
            if (LocalizationPlugin.zektonFontAsset != null) return;

            try
            {
                string fontPath = Path.Combine(Paths.GameRootPath, "Zekton.ttf");
                if (File.Exists(fontPath))
                {
                    LocalizationPlugin.zektonFont = new Font(fontPath);
                    LocalizationPlugin.zektonFont.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    LocalizationPlugin.zektonFont.RequestCharactersInTexture(VI_CHARS, 24, FontStyle.Normal);

                    LocalizationPlugin.zektonFontAsset = TMP_FontAsset.CreateFontAsset(LocalizationPlugin.zektonFont);
                    if (LocalizationPlugin.zektonFontAsset != null)
                    {
                        LocalizationPlugin.zektonFontAsset.name = "Zekton_VietHoa";
                        LocalizationPlugin.zektonFontAsset.hideFlags = HideFlags.DontUnloadUnusedAsset;

                        SetFontAssetDynamic(LocalizationPlugin.zektonFontAsset);
                        
                        bool added = LocalizationPlugin.zektonFontAsset.TryAddCharacters(VI_CHARS);
                        LocalizationPlugin.Log.LogInfo("[IFSCL Localization] Force-added VI characters to Zekton: " + added);

                        RegisterGlobalFallback(LocalizationPlugin.zektonFontAsset);
                    }
                }
                else
                {
                    LocalizationPlugin.Log.LogError("[IFSCL Localization] Zekton font file not found at game root: " + fontPath);
                }
            }
            catch (Exception ex)
            {
                LocalizationPlugin.Log.LogError("[IFSCL Localization] Failed to initialize Zekton fallback font: " + ex);
            }
        }

        private static void EnsureOSFallbackInitialized()
        {
            if (LocalizationPlugin.osFontAsset != null) return;

            try
            {
                string arialPath = @"C:\Windows\Fonts\arial.ttf";
                if (File.Exists(arialPath))
                {
                    Font osFont = new Font(arialPath);
                    osFont.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    osFont.RequestCharactersInTexture(VI_CHARS, 24, FontStyle.Normal);

                    LocalizationPlugin.osFontAsset = TMP_FontAsset.CreateFontAsset(osFont);
                    if (LocalizationPlugin.osFontAsset != null)
                    {
                        LocalizationPlugin.osFontAsset.name = "Arial_OS_Fallback";
                        LocalizationPlugin.osFontAsset.hideFlags = HideFlags.DontUnloadUnusedAsset;

                        SetFontAssetDynamic(LocalizationPlugin.osFontAsset);
                        
                        bool added = LocalizationPlugin.osFontAsset.TryAddCharacters(VI_CHARS);
                        LocalizationPlugin.Log.LogInfo("[IFSCL Localization] Force-added VI characters to Arial OS: " + added);

                        RegisterGlobalFallback(LocalizationPlugin.osFontAsset);
                    }
                }
            }
            catch (Exception ex)
            {
                LocalizationPlugin.Log.LogError("[IFSCL Localization] Failed to initialize Arial OS fallback: " + ex);
            }
        }

        private static void SetFontAssetDynamic(TMP_FontAsset asset)
        {
            try
            {
                var prop = typeof(TMP_FontAsset).GetProperty("atlasPopulationMode",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(asset, 1, null);
                }
            }
            catch (Exception ex)
            {
                LocalizationPlugin.Log.LogError("[IFSCL Localization] Failed to set dynamic mode for " + asset.name + ": " + ex.Message);
            }
        }

        private static void RegisterGlobalFallback(TMP_FontAsset fallback)
        {
            if (fallback == null) return;
            
            if (TMP_Settings.fallbackFontAssets == null)
            {
                var field = typeof(TMP_Settings).GetField("m_fallbackFontAssets", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null && TMP_Settings.instance != null)
                {
                    List<TMP_FontAsset> list = field.GetValue(TMP_Settings.instance) as List<TMP_FontAsset>;
                    if (list == null)
                    {
                        list = new List<TMP_FontAsset>();
                        field.SetValue(TMP_Settings.instance, list);
                    }
                    if (!list.Contains(fallback)) list.Insert(0, fallback);
                }
            }
            else
            {
                if (!TMP_Settings.fallbackFontAssets.Contains(fallback))
                {
                    TMP_Settings.fallbackFontAssets.Insert(0, fallback);
                }
            }
        }

        public static void ApplyFallbackToAllFonts()
        {
            try
            {
                TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                foreach (TMP_FontAsset fa in allFonts)
                {
                    ApplyFallbackToFont(fa);
                }
            }
            catch (Exception ex)
            {
                LocalizationPlugin.Log.LogError("[IFSCL Localization] Error during ApplyFallbackToAllFonts: " + ex);
            }
        }

        public static void ApplyFallbackToFont(TMP_FontAsset fa)
        {
            if (fa == null) return;
            if (fa == LocalizationPlugin.vnFontAsset || fa == LocalizationPlugin.ubuntuFontAsset || fa == LocalizationPlugin.zektonFontAsset || fa == LocalizationPlugin.osFontAsset) return;
            if (fa.name == "Gunship_VietHoa" || fa.name == "Ubuntu_VietHoa" || fa.name == "Zekton_VietHoa" || fa.name == "Arial_OS_Fallback") return;

            int instanceId = fa.GetInstanceID();
            if (!LocalizationPlugin.PatchedFontInstanceIds.Contains(instanceId))
            {
                if (fa.fallbackFontAssetTable == null)
                {
                    fa.fallbackFontAssetTable = new List<TMP_FontAsset>();
                }

                if (fa.name.IndexOf("Zekton", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (LocalizationPlugin.zektonFontAsset != null && !fa.fallbackFontAssetTable.Contains(LocalizationPlugin.zektonFontAsset))
                    {
                        fa.fallbackFontAssetTable.Insert(0, LocalizationPlugin.zektonFontAsset);
                    }
                    if (LocalizationPlugin.osFontAsset != null && !fa.fallbackFontAssetTable.Contains(LocalizationPlugin.osFontAsset))
                    {
                        fa.fallbackFontAssetTable.Add(LocalizationPlugin.osFontAsset);
                    }
                }
                else if (fa.name.IndexOf("Ubuntu", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (LocalizationPlugin.ubuntuFontAsset != null && !fa.fallbackFontAssetTable.Contains(LocalizationPlugin.ubuntuFontAsset))
                    {
                        fa.fallbackFontAssetTable.Insert(0, LocalizationPlugin.ubuntuFontAsset);
                    }
                    if (LocalizationPlugin.osFontAsset != null && !fa.fallbackFontAssetTable.Contains(LocalizationPlugin.osFontAsset))
                    {
                        fa.fallbackFontAssetTable.Add(LocalizationPlugin.osFontAsset);
                    }
                }
                else
                {
                    if (LocalizationPlugin.vnFontAsset != null && !fa.fallbackFontAssetTable.Contains(LocalizationPlugin.vnFontAsset))
                    {
                        fa.fallbackFontAssetTable.Insert(0, LocalizationPlugin.vnFontAsset);
                    }
                    if (LocalizationPlugin.zektonFontAsset != null && !fa.fallbackFontAssetTable.Contains(LocalizationPlugin.zektonFontAsset))
                    {
                        fa.fallbackFontAssetTable.Add(LocalizationPlugin.zektonFontAsset);
                    }
                    if (LocalizationPlugin.ubuntuFontAsset != null && !fa.fallbackFontAssetTable.Contains(LocalizationPlugin.ubuntuFontAsset))
                    {
                        fa.fallbackFontAssetTable.Add(LocalizationPlugin.ubuntuFontAsset);
                    }
                    if (LocalizationPlugin.osFontAsset != null && !fa.fallbackFontAssetTable.Contains(LocalizationPlugin.osFontAsset))
                    {
                        fa.fallbackFontAssetTable.Add(LocalizationPlugin.osFontAsset);
                    }
                }

                LocalizationPlugin.PatchedFontInstanceIds.Add(instanceId);
            }
        }

        public static void RunDiagnostics()
        {
            try
            {
                LocalizationPlugin.Log.LogInfo("--- [Detailed Diagnostic Report] ---");
                TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                LocalizationPlugin.Log.LogInfo("Active Fonts in RAM: " + fonts.Length);

                foreach (var f in fonts)
                {
                    if (f == null) continue;
                    
                    LocalizationPlugin.Log.LogInfo(string.Format("Font: {0} | Chars: {1} | Glyphs: {2} | Atlas: {3}x{4}",
                        f.name,
                        f.characterTable != null ? f.characterTable.Count : 0,
                        f.glyphTable != null ? f.glyphTable.Count : 0,
                        f.atlasWidth,
                        f.atlasHeight));
                }
                LocalizationPlugin.Log.LogInfo("====================================");
            }
            catch (Exception ex)
            {
                LocalizationPlugin.Log.LogError("[Diagnostic] Error: " + ex);
            }
        }
    }
}
