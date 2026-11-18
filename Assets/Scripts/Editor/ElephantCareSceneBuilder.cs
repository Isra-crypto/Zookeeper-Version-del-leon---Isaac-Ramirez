using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZooGame.Care;
using ZooGame.Core;
using ZooGame.World;

namespace ZooGame.Editor
{
    public static class ElephantCareSceneBuilder
    {
        private const string SCENES_FOLDER = "Assets/Scenes";
        private const string SCENE_NAME = "ElephantCareScene";
        private const string SCENE_PATH = "Assets/Scenes/ElephantCareScene.unity";
        private const string MAIN_SCENE_PATH = "Assets/Scenes/SampleScene.unity";

        private const string SPRITES_ROOT = "Assets/Sprites";
        private const string ELEPHANT_SPRITES = "Assets/Sprites/Animals/Elefante";
        private const string HABITAT_SPRITES = "Assets/Sprites/Habitat";
        private const string NATURE_SPRITES = "Assets/Sprites/Nature";
        private const string ITEM_SPRITES = "Assets/Sprites/Items";
        private const string EMOTE_SPRITES = "Assets/Sprites/Emotes";

        [MenuItem("ZooGame/Build Elephant Care Scene")]
        public static void BuildElephantCareScene()
        {
            // 1. Create fresh scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Camera (Clean centered view on the diorama)
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 4.8f;
            // Elegant safari warm sky / soft atmospheric backdrop
            cam.backgroundColor = new Color(0.86f, 0.90f, 0.84f); 
            cam.transform.position = new Vector3(0, 0, -10);
            camObj.AddComponent<AudioListener>();

            // 3. Root GameObject
            GameObject worldRoot = new GameObject("Elephant_Care_Environment");

            // --- A. GENTLE AMBIENT SKY & CLOUDS ---
            GameObject skyRoot = new GameObject("Sky_Clouds");
            skyRoot.transform.SetParent(worldRoot.transform);

            Sprite[] cloudSprites = LoadAllSprites($"{HABITAT_SPRITES}/nubes.png");
            if (cloudSprites != null && cloudSprites.Length > 0)
            {
                Vector3[] cloudPositions = new Vector3[]
                {
                    new Vector3(-6.0f, 3.8f, 0),
                    new Vector3(1.0f, 4.2f, 0),
                    new Vector3(7.0f, 3.5f, 0)
                };
                float[] speeds = new float[] { 0.2f, 0.15f, 0.25f };

                for (int i = 0; i < cloudPositions.Length; i++)
                {
                    GameObject cloud = new GameObject($"Cloud_{i + 1}");
                    cloud.transform.SetParent(skyRoot.transform);
                    cloud.transform.position = cloudPositions[i];
                    SpriteRenderer sr = cloud.AddComponent<SpriteRenderer>();
                    sr.sprite = cloudSprites[i % cloudSprites.Length];
                    sr.sortingOrder = -200;
                    sr.color = new Color(1f, 1f, 1f, 0.65f);
                    cloud.transform.localScale = Vector3.one * 1.0f;

                    CloudMover mover = cloud.AddComponent<CloudMover>();
                    SerializedObject moverSo = new SerializedObject(mover);
                    moverSo.FindProperty("speed").floatValue = speeds[i];
                    moverSo.FindProperty("startX").floatValue = -10f;
                    moverSo.FindProperty("resetX").floatValue = 10f;
                    moverSo.ApplyModifiedProperties();
                }
            }

            // --- B. SOFT BACKGROUND HILLS / GRASS TINT ---
            GameObject groundRoot = new GameObject("Ground_Backdrop");
            groundRoot.transform.SetParent(worldRoot.transform);

            Sprite grassTile = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITES_ROOT}/Grass_Tile.png");
            if (grassTile != null)
            {
                // Soft background grass plane
                GameObject bgPlane = new GameObject("Grass_Floor");
                bgPlane.transform.SetParent(groundRoot.transform);
                bgPlane.transform.position = new Vector3(0, 0, 0);
                bgPlane.transform.localScale = new Vector3(25f, 15f, 1f);
                SpriteRenderer bgSr = bgPlane.AddComponent<SpriteRenderer>();
                bgSr.sprite = grassTile;
                bgSr.sortingOrder = -250;
                bgSr.color = new Color(0.85f, 0.90f, 0.82f, 0.6f);
            }

            // --- C. THE ISOMETRIC HABITAT DIORAMA (CENTERPIECE) ---
            GameObject dioramaRoot = new GameObject("Habitat_Diorama");
            dioramaRoot.transform.SetParent(worldRoot.transform);

            Sprite habitatSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{HABITAT_SPRITES}/habitat con piedras.png")
                                ?? AssetDatabase.LoadAssetAtPath<Sprite>($"{HABITAT_SPRITES}/habitat sin piedras.png");

            if (habitatSprite != null)
            {
                GameObject habObj = new GameObject("Habitat_Enclosure");
                habObj.transform.SetParent(dioramaRoot.transform);
                habObj.transform.position = new Vector3(0, 0.1f, 0);
                habObj.transform.localScale = Vector3.one * 1.35f;
                SpriteRenderer habSr = habObj.AddComponent<SpriteRenderer>();
                habSr.sprite = habitatSprite;
                habSr.sortingOrder = -100;
            }

            // --- D. ELEGANT NATURAL FRAMING (1 Tree & Subtle Rocks) ---
            // Majestic Acacia Tree on top-left of the enclosure
            Sprite acaciaSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{HABITAT_SPRITES}/arbol.png");
            if (acaciaSprite != null)
            {
                GameObject treeObj = new GameObject("Savanna_Acacia_Tree");
                treeObj.transform.SetParent(dioramaRoot.transform);
                treeObj.transform.position = new Vector3(-4.6f, 1.8f, 0);
                treeObj.transform.localScale = Vector3.one * 1.25f;
                SpriteRenderer treeSr = treeObj.AddComponent<SpriteRenderer>();
                treeSr.sprite = acaciaSprite;
                YSortRenderer ysr = treeObj.AddComponent<YSortRenderer>();
                ysr.YOffset = -1.8f;
            }

            // Subtle Rocks on top-right corner outside the fence
            Sprite[] rockSprites = LoadAllSprites($"{HABITAT_SPRITES}/piedras.png");
            if (rockSprites != null && rockSprites.Length > 0)
            {
                GameObject rockObj = new GameObject("Savanna_Rocks_TopRight");
                rockObj.transform.SetParent(dioramaRoot.transform);
                rockObj.transform.position = new Vector3(4.2f, 2.2f, 0);
                rockObj.transform.localScale = Vector3.one * 1.1f;
                SpriteRenderer rSr = rockObj.AddComponent<SpriteRenderer>();
                rSr.sprite = rockSprites[0];
                YSortRenderer ysr = rockObj.AddComponent<YSortRenderer>();
                ysr.YOffset = -0.3f;
            }

            // Subtle Grass Tufts along the back and corners
            Sprite tuft0 = AssetDatabase.LoadAssetAtPath<Sprite>($"{NATURE_SPRITES}/sprite_grasstufts0.png");
            Sprite tuft1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{NATURE_SPRITES}/sprite_grasstufts1.png");
            if (tuft0 != null || tuft1 != null)
            {
                Vector3[] tuftSpots = new Vector3[]
                {
                    new Vector3(-4.0f, -2.5f, 0),
                    new Vector3(4.0f, -2.5f, 0),
                    new Vector3(-5.2f, 0.2f, 0),
                    new Vector3(5.2f, 0.2f, 0)
                };

                for (int i = 0; i < tuftSpots.Length; i++)
                {
                    GameObject t = new GameObject($"GrassTuft_{i + 1}");
                    t.transform.SetParent(dioramaRoot.transform);
                    t.transform.position = tuftSpots[i];
                    t.transform.localScale = Vector3.one * 1.2f;
                    SpriteRenderer sr = t.AddComponent<SpriteRenderer>();
                    sr.sprite = (i % 2 == 0) ? (tuft0 ?? tuft1) : (tuft1 ?? tuft0);
                    sr.sortingOrder = -90;
                }
            }

            // --- E. THE 3 INTERACTIVE ELEPHANTS ---
            Sprite heartEmote = AssetDatabase.LoadAssetAtPath<Sprite>($"{EMOTE_SPRITES}/corazao.png");
            Sprite foodEmote = AssetDatabase.LoadAssetAtPath<Sprite>($"{ITEM_SPRITES}/comida.png");
            Sprite waterEmote = AssetDatabase.LoadAssetAtPath<Sprite>($"{EMOTE_SPRITES}/gota.png");
            Sprite dirtyEmote = AssetDatabase.LoadAssetAtPath<Sprite>($"{EMOTE_SPRITES}/malolor.png");
            Sprite medicineEmote = AssetDatabase.LoadAssetAtPath<Sprite>($"{ITEM_SPRITES}/medicina.png");

            Sprite machoIdle = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteMparado.png");
            Sprite machoWalk1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteMcaminando1.png");
            Sprite machoWalk2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteMcaminando2.png");

            Sprite hembraIdle = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteHparado.png");
            Sprite hembraWalk1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteHcaminando1.png");
            Sprite hembraWalk2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteHcaminando2.png");

            Sprite bebeIdle = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteBparado.png");
            Sprite bebeWalk1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteBcaminando1.png");
            Sprite bebeWalk2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteBcaminando2.png");

            // Tantor (Father)
            CreateCareElephant(dioramaRoot.transform, "Tantor (Papá)", new Vector3(-1.8f, 0.0f, 0),
                ElephantStage.Macho, ElephantNeed.Hambre, 75, machoIdle, machoWalk1, machoWalk2, 1.25f,
                heartEmote, foodEmote, waterEmote, dirtyEmote, medicineEmote);

            // Ella (Mother)
            CreateCareElephant(dioramaRoot.transform, "Ella (Mamá)", new Vector3(1.8f, -0.2f, 0),
                ElephantStage.Hembra, ElephantNeed.Sed, 80, hembraIdle, hembraWalk1, hembraWalk2, 1.15f,
                heartEmote, foodEmote, waterEmote, dirtyEmote, medicineEmote);

            // Babu (Baby)
            CreateCareElephant(dioramaRoot.transform, "Babu (Bebé)", new Vector3(0.0f, -1.4f, 0),
                ElephantStage.Bebe, ElephantNeed.Sucio, 60, bebeIdle, bebeWalk1, bebeWalk2, 0.85f,
                heartEmote, foodEmote, waterEmote, dirtyEmote, medicineEmote);

            // --- F. SETUP MANAGER & UI CANVAS ---
            GameObject mgrObj = new GameObject("ElephantCareManager");
            ElephantCareManager careMgr = mgrObj.AddComponent<ElephantCareManager>();

            SetupCareHUD(mgrObj);

            // Save and update build settings
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);
            UpdateBuildSettings();
            EditorSceneManager.OpenScene(MAIN_SCENE_PATH);

            Debug.Log("<color=#44FF44><b>[Zoo Game]</b> ¡Escenario limpio, diorama elegante y cuidado de elefantes creado con éxito!</color>");
        }

        private static Sprite[] LoadAllSprites(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            List<Sprite> list = new List<Sprite>();
            foreach (Object o in assets)
            {
                if (o is Sprite s) list.Add(s);
            }
            return list.ToArray();
        }

        private static void CreateCareElephant(Transform parent, string name, Vector3 pos, ElephantStage stage,
            ElephantNeed need, int initialHappiness, Sprite idle, Sprite w1, Sprite w2, float scale,
            Sprite heart, Sprite food, Sprite water, Sprite dirty, Sprite medicine)
        {
            GameObject obj = new GameObject($"CareElephant_{name}");
            obj.transform.SetParent(parent);
            obj.transform.position = pos;
            obj.transform.localScale = Vector3.one * scale;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = idle;

            YSortRenderer ysr = obj.AddComponent<YSortRenderer>();
            ysr.YOffset = -0.5f;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.85f;
            col.offset = new Vector2(0, 0.3f);

            // Emote visual above head
            GameObject emoteObj = new GameObject("EmoteBubble");
            emoteObj.transform.SetParent(obj.transform);
            emoteObj.transform.localPosition = new Vector3(0, 1.3f, 0);
            emoteObj.transform.localScale = Vector3.one * 0.9f;
            SpriteRenderer emoteSr = emoteObj.AddComponent<SpriteRenderer>();
            emoteSr.sortingOrder = 9000;

            InteractiveCareElephant careElephant = obj.AddComponent<InteractiveCareElephant>();
            SerializedObject so = new SerializedObject(careElephant);
            so.FindProperty("elephantName").stringValue = name;
            so.FindProperty("stage").enumValueIndex = (int)stage;
            so.FindProperty("currentNeed").enumValueIndex = (int)need;
            so.FindProperty("happiness").intValue = initialHappiness;
            so.FindProperty("idleSprite").objectReferenceValue = idle;
            so.FindProperty("walk1Sprite").objectReferenceValue = w1;
            so.FindProperty("walk2Sprite").objectReferenceValue = w2;
            so.FindProperty("emoteRenderer").objectReferenceValue = emoteSr;
            so.FindProperty("heartEmote").objectReferenceValue = heart;
            so.FindProperty("foodEmote").objectReferenceValue = food;
            so.FindProperty("waterEmote").objectReferenceValue = water;
            so.FindProperty("dirtyEmote").objectReferenceValue = dirty;
            so.FindProperty("medicineEmote").objectReferenceValue = medicine;
            so.ApplyModifiedProperties();
        }

        private static void SetupCareHUD(GameObject managerObj)
        {
            // EventSystem
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            GameObject canvasObj = new GameObject("Care_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler cs = canvasObj.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1280, 720);
            cs.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            CareHUD hud = canvasObj.AddComponent<CareHUD>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Top Action Banner
            GameObject bannerObj = new GameObject("TopBanner");
            bannerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bannerRt = bannerObj.AddComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.5f, 1f);
            bannerRt.anchorMax = new Vector2(0.5f, 1f);
            bannerRt.pivot = new Vector2(0.5f, 1f);
            bannerRt.anchoredPosition = new Vector2(0, -14);
            bannerRt.sizeDelta = new Vector2(620, 44);

            Image bannerBg = bannerObj.AddComponent<Image>();
            bannerBg.color = new Color(0.12f, 0.14f, 0.18f, 0.92f);

            GameObject bannerTextObj = new GameObject("FeedbackText");
            bannerTextObj.transform.SetParent(bannerObj.transform, false);
            Text bannerText = bannerTextObj.AddComponent<Text>();
            bannerText.text = "🐘 Selecciona una herramienta abajo y haz clic en un elefante para cuidarlo.";
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.fontSize = 15;
            bannerText.color = Color.white;
            bannerText.font = font;
            RectTransform btr = bannerTextObj.GetComponent<RectTransform>();
            btr.anchorMin = Vector2.zero;
            btr.anchorMax = Vector2.one;
            btr.sizeDelta = Vector2.zero;

            // Top-Left Return Button
            GameObject returnBtnObj = new GameObject("Btn_Return");
            returnBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform btnRt = returnBtnObj.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0f, 1f);
            btnRt.anchorMax = new Vector2(0f, 1f);
            btnRt.pivot = new Vector2(0f, 1f);
            btnRt.anchoredPosition = new Vector2(20, -14);
            btnRt.sizeDelta = new Vector2(160, 44);

            Image btnImg = returnBtnObj.AddComponent<Image>();
            btnImg.color = new Color(0.75f, 0.22f, 0.22f, 0.95f);
            Button returnBtn = returnBtnObj.AddComponent<Button>();

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(returnBtnObj.transform, false);
            Text btnText = btnTextObj.AddComponent<Text>();
            btnText.text = "← Volver al Zoo";
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.fontSize = 14;
            btnText.fontStyle = FontStyle.Bold;
            btnText.color = Color.white;
            btnText.font = font;
            RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.sizeDelta = Vector2.zero;

            // Top-Right Happiness Widget
            GameObject hapObj = new GameObject("HappinessWidget");
            hapObj.transform.SetParent(canvasObj.transform, false);
            RectTransform hapRt = hapObj.AddComponent<RectTransform>();
            hapRt.anchorMin = new Vector2(1f, 1f);
            hapRt.anchorMax = new Vector2(1f, 1f);
            hapRt.pivot = new Vector2(1f, 1f);
            hapRt.anchoredPosition = new Vector2(-20, -14);
            hapRt.sizeDelta = new Vector2(210, 44);

            Image hapBg = hapObj.AddComponent<Image>();
            hapBg.color = new Color(0.12f, 0.28f, 0.18f, 0.92f);

            GameObject hapTextObj = new GameObject("Text");
            hapTextObj.transform.SetParent(hapObj.transform, false);
            Text hapText = hapTextObj.AddComponent<Text>();
            hapText.text = "❤️ Bienestar: <b>75%</b>";
            hapText.alignment = TextAnchor.MiddleCenter;
            hapText.fontSize = 14;
            hapText.color = new Color(0.6f, 1f, 0.6f);
            hapText.font = font;
            RectTransform hapTextRt = hapTextObj.GetComponent<RectTransform>();
            hapTextRt.anchorMin = Vector2.zero;
            hapTextRt.anchorMax = Vector2.one;
            hapTextRt.sizeDelta = Vector2.zero;

            // Bottom Care Hotbar
            GameObject hotbarObj = new GameObject("Care_Hotbar");
            hotbarObj.transform.SetParent(canvasObj.transform, false);
            RectTransform hr = hotbarObj.AddComponent<RectTransform>();
            hr.anchorMin = new Vector2(0.5f, 0.08f);
            hr.anchorMax = new Vector2(0.5f, 0.08f);
            hr.pivot = new Vector2(0.5f, 0.5f);
            hr.sizeDelta = new Vector2(420, 76);

            Image hotbarBg = hotbarObj.AddComponent<Image>();
            hotbarBg.color = new Color(0.10f, 0.09f, 0.08f, 0.94f);

            HorizontalLayoutGroup hlg = hotbarObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(12, 12, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ITEM_SPRITES}/marquito.png");
            string[] toolNames = { "comida.png", "cubeta.png", "escoba.png", "guante.png", "medicina.png" };
            CareToolType[] toolTypes = { CareToolType.Comida, CareToolType.Agua, CareToolType.Escoba, CareToolType.Guante, CareToolType.Medicina };

            List<Image> borders = new List<Image>();

            for (int i = 0; i < toolNames.Length; i++)
            {
                CareToolType tType = toolTypes[i];

                GameObject slot = new GameObject($"CareSlot_{i + 1}");
                slot.transform.SetParent(hotbarObj.transform, false);
                RectTransform slotRt = slot.AddComponent<RectTransform>();
                slotRt.sizeDelta = new Vector2(62, 62);

                Image slotBg = slot.AddComponent<Image>();
                if (frameSprite != null) slotBg.sprite = frameSprite;
                else slotBg.color = new Color(0.28f, 0.22f, 0.16f);
                slotBg.raycastTarget = true;
                borders.Add(slotBg);

                Button slotBtn = slot.AddComponent<Button>();
                ToolSlotButton tsb = slot.AddComponent<ToolSlotButton>();
                SerializedObject tsbSo = new SerializedObject(tsb);
                tsbSo.FindProperty("toolType").enumValueIndex = (int)tType;
                tsbSo.ApplyModifiedProperties();

                // Tool Icon inside
                Sprite toolSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ITEM_SPRITES}/{toolNames[i]}");
                if (toolSprite != null)
                {
                    GameObject iconObj = new GameObject("Icon");
                    iconObj.transform.SetParent(slot.transform, false);
                    RectTransform iconRt = iconObj.AddComponent<RectTransform>();
                    iconRt.sizeDelta = new Vector2(42, 42);

                    Image iconImg = iconObj.AddComponent<Image>();
                    iconImg.sprite = toolSprite;
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;
                }
            }

            SerializedObject hudSo = new SerializedObject(hud);
            hudSo.FindProperty("feedbackText").objectReferenceValue = bannerText;
            hudSo.FindProperty("happinessText").objectReferenceValue = hapText;
            hudSo.FindProperty("returnButton").objectReferenceValue = returnBtn;

            SerializedProperty bordersProp = hudSo.FindProperty("slotBorders");
            bordersProp.arraySize = borders.Count;
            for (int i = 0; i < borders.Count; i++)
            {
                bordersProp.GetArrayElementAtIndex(i).objectReferenceValue = borders[i];
            }
            hudSo.ApplyModifiedProperties();
        }

        private static void UpdateBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

            if (File.Exists(MAIN_SCENE_PATH))
            {
                scenes.Add(new EditorBuildSettingsScene(MAIN_SCENE_PATH, true));
            }

            if (File.Exists(SCENE_PATH))
            {
                scenes.Add(new EditorBuildSettingsScene(SCENE_PATH, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
