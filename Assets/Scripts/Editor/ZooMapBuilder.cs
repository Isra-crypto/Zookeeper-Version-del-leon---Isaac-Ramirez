using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZooGame.CameraControl;
using ZooGame.Core;
using ZooGame.Player;
using ZooGame.UI;
using ZooGame.World;

namespace ZooGame.Editor
{
    public static class ZooMapBuilder
    {
        private const string SPRITES_FOLDER = "Assets/Sprites";
        private const string ELEPHANT_SPRITES = "Assets/Sprites/Animals/Elefante";
        private const string OSTRICH_M_SPRITES = "Assets/Sprites/Animals/Avestruz/Macho";
        private const string OSTRICH_F_SPRITES = "Assets/Sprites/Animals/Avestruz/Hembra";
        private const string HABITAT_SPRITES = "Assets/Sprites/Habitat";
        private const string TILESET_SPRITES = "Assets/Sprites/Tilesets/Enclosure";
        private const string FENCE_SPRITES = "Assets/Sprites/Fences";
        private const string NATURE_SPRITES = "Assets/Sprites/Nature";
        private const string TROUGH_SPRITES = "Assets/Sprites/Troughs";
        private const string EMOTE_SPRITES = "Assets/Sprites/Emotes";
        private const string ITEM_SPRITES = "Assets/Sprites/Items";

        [MenuItem("ZooGame/Setup Starter Zoo Scene")]
        public static void SetupStarterZooScene()
        {
            EnsureFolderExists(SPRITES_FOLDER);
            ElephantSetupUtility.ImportAndSetupElephantAssets();

            // 1. Base Sprites (Grass, Path, Water, Cursor, Zookeeper)
            Sprite grassSprite = CreatePixelSprite("Grass_Tile", 16, 16, (x, y) => {
                Color baseCol = new Color(0.38f, 0.65f, 0.28f);
                if ((x + y * 3) % 7 == 0) return new Color(0.44f, 0.72f, 0.32f);
                if ((x * 2 + y) % 9 == 0) return new Color(0.32f, 0.55f, 0.23f);
                return baseCol;
            });

            Sprite pathSprite = CreatePixelSprite("Path_Tile", 16, 16, (x, y) => {
                Color baseCol = new Color(0.82f, 0.71f, 0.53f);
                if (x == 0 || x == 15 || y == 0 || y == 15) return new Color(0.68f, 0.56f, 0.38f);
                if ((x + y) % 5 == 0) return new Color(0.88f, 0.78f, 0.60f);
                return baseCol;
            });

            Sprite waterSprite = CreatePixelSprite("Water_Tile", 16, 16, (x, y) => {
                Color baseCol = new Color(0.22f, 0.55f, 0.78f);
                if (y == 15 || (x % 4 == 0 && y == 14)) return new Color(0.65f, 0.85f, 0.95f);
                if ((x + y * 2) % 6 == 0) return new Color(0.18f, 0.45f, 0.68f);
                return baseCol;
            });

            Sprite zookeeperSprite = CreatePixelSprite("Zookeeper_Sprite", 16, 24, (x, y) => {
                if (y <= 3)
                {
                    if ((x >= 4 && x <= 6) || (x >= 9 && x <= 11)) return new Color(0.25f, 0.18f, 0.12f);
                    return Color.clear;
                }
                if (y <= 8)
                {
                    if (x >= 4 && x <= 11) return new Color(0.58f, 0.52f, 0.38f);
                    return Color.clear;
                }
                if (y <= 15)
                {
                    if (x >= 3 && x <= 12)
                    {
                        if (x == 7 || x == 8) return new Color(0.24f, 0.38f, 0.20f);
                        return new Color(0.32f, 0.48f, 0.26f);
                    }
                    return Color.clear;
                }
                if (y <= 19)
                {
                    if (x >= 5 && x <= 10)
                    {
                        if (y == 17 && (x == 6 || x == 9)) return new Color(0.1f, 0.1f, 0.1f);
                        return new Color(0.96f, 0.78f, 0.65f);
                    }
                    return Color.clear;
                }
                if (y <= 23)
                {
                    if (y == 20 && x >= 2 && x <= 13) return new Color(0.85f, 0.72f, 0.45f);
                    if (y == 21 && x >= 4 && x <= 11) return new Color(0.35f, 0.25f, 0.15f);
                    if (y >= 22 && x >= 5 && x <= 10) return new Color(0.85f, 0.72f, 0.45f);
                }
                return Color.clear;
            }, 16, new Vector2(0.5f, 0.15f));

            Sprite signSprite = CreatePixelSprite("Sign_Sprite", 16, 16, (x, y) => {
                Color wood = new Color(0.65f, 0.45f, 0.25f);
                Color woodBorder = new Color(0.42f, 0.26f, 0.12f);
                Color woodPost = new Color(0.48f, 0.32f, 0.16f);
                if (y <= 6 && x >= 7 && x <= 8) return woodPost;
                if (y >= 7 && y <= 14 && x >= 2 && x <= 13)
                {
                    if (x == 2 || x == 13 || y == 7 || y == 14) return woodBorder;
                    return wood;
                }
                return Color.clear;
            });

            Sprite cursorSprite = CreatePixelSprite("Cursor_Tile", 16, 16, (x, y) => {
                Color border = new Color(1f, 0.92f, 0.23f, 0.85f);
                bool isCorner = ((x <= 2 || x >= 13) && (y <= 2 || y >= 13));
                bool isEdge = (x == 0 || x == 15 || y == 0 || y == 15);
                if (isCorner && isEdge) return border;
                return Color.clear;
            });

            // 2. Setup Hierarchy & Map
            GameObject zooRoot = GameObject.Find("Zoo_World");
            if (zooRoot != null) Object.DestroyImmediate(zooRoot);

            zooRoot = new GameObject("Zoo_World");
            GameObject groundParent = new GameObject("Ground");
            groundParent.transform.SetParent(zooRoot.transform);
            GameObject environmentParent = new GameObject("Environment");
            environmentParent.transform.SetParent(zooRoot.transform);

            // Load Enclosure Dirt Tileset (3x3 autotile)
            Sprite[] encTiles = new Sprite[9];
            for (int i = 0; i < 9; i++)
            {
                encTiles[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{TILESET_SPRITES}/sprite_{i}.png");
            }

            // Map Size 34x24
            int width = 34;
            int height = 24;
            int startX = -17;
            int startY = -12;

            // Ostrich enclosure bounds: x [5, 14], y [3, 10]
            int ostMinX = 5, ostMaxX = 14, ostMinY = 3, ostMaxY = 10;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int wx = startX + x;
                    int wy = startY + y;

                    // Central crossroads paths
                    bool isPath = (wx >= -2 && wx <= 2) || (wy >= -1 && wy <= 1 && wx >= -15 && wx <= 15);
                    
                    // Pond area (bottom left)
                    bool isWater = (wx >= -15 && wx <= -9 && wy >= -10 && wy <= -5);

                    // Ostrich enclosure dirt floor
                    bool isOstrichFloor = (wx >= ostMinX && wx <= ostMaxX && wy >= ostMinY && wy <= ostMaxY);

                    GameObject tile = new GameObject($"Tile_{wx}_{wy}");
                    tile.transform.SetParent(groundParent.transform);
                    tile.transform.position = new Vector3(wx, wy, 0);

                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = -100; // Ground is always at base layer -100

                    if (isWater)
                    {
                        sr.sprite = waterSprite;
                        BoxCollider2D bc = tile.AddComponent<BoxCollider2D>();
                        bc.size = new Vector2(1f, 1f);
                    }
                    else if (isOstrichFloor)
                    {
                        int tileIdx = GetAutotileIndex(wx, wy, ostMinX, ostMaxX, ostMinY, ostMaxY);
                        sr.sprite = encTiles[tileIdx] ?? grassSprite;
                    }
                    else if (isPath)
                    {
                        sr.sprite = pathSprite;
                    }
                    else
                    {
                        sr.sprite = grassSprite;
                    }
                }
            }

            // Build Ostrich Enclosure Fences, Troughs, and Animals
            BuildOstrichEnclosure(environmentParent.transform, ostMinX, ostMaxX, ostMinY, ostMaxY, signSprite);

            // Build Elephant Savanna (x -15 to -4, y 3 to 10)
            BuildElephantEnclosure(environmentParent.transform, -15, -4, 3, 10, signSprite);

            // Add Nature Details across Park (Trees, Bushes, Grass Tufts)
            AddParkNatureDecorations(environmentParent.transform, groundParent.transform);

            // 3. Create Player (Zookeeper)
            GameObject player = GameObject.Find("Player");
            if (player != null) Object.DestroyImmediate(player);

            player = new GameObject("Player");
            player.transform.position = new Vector3(0, 0, 0);

            SpriteRenderer playerSr = player.AddComponent<SpriteRenderer>();
            playerSr.sprite = zookeeperSprite;
            player.AddComponent<YSortRenderer>();

            CircleCollider2D playerCol = player.AddComponent<CircleCollider2D>();
            playerCol.radius = 0.35f;
            playerCol.offset = new Vector2(0, 0.2f);

            Rigidbody2D playerRb = player.AddComponent<Rigidbody2D>();
            playerRb.gravityScale = 0;
            playerRb.freezeRotation = true;

            PlayerController playerController = player.AddComponent<PlayerController>();
            player.AddComponent<PlayerInteraction>();

            // Target selector cursor
            GameObject cursor = new GameObject("TargetCursor");
            cursor.transform.SetParent(player.transform);
            cursor.transform.localPosition = new Vector3(0, -1f, 0);
            SpriteRenderer cursorSr = cursor.AddComponent<SpriteRenderer>();
            cursorSr.sprite = cursorSprite;
            cursorSr.sortingOrder = 250;

            SerializedObject pcSo = new SerializedObject(playerController);
            pcSo.FindProperty("targetCursor").objectReferenceValue = cursor.transform;
            pcSo.FindProperty("spriteRenderer").objectReferenceValue = playerSr;
            pcSo.ApplyModifiedProperties();

            // 4. Setup Main Camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5f;
            mainCam.backgroundColor = new Color(0.2f, 0.2f, 0.22f);

            CameraFollow2D camFollow = mainCam.GetComponent<CameraFollow2D>();
            if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow2D>();
            camFollow.Target = player.transform;

            // 5. Setup UI HUD & Toolbar
            SetupHUD(player);

            AssetDatabase.SaveAssets();
            Debug.Log("<color=#44FF44><b>[Zoo Game]</b> ¡Mapa enriquecido con Avestruces, Hábitats, Naturaleza y Comederos creado con éxito!</color>");
        }

        private static int GetAutotileIndex(int x, int y, int minX, int maxX, int minY, int maxY)
        {
            bool isLeft = (x == minX);
            bool isRight = (x == maxX);
            bool isTop = (y == maxY);
            bool isBottom = (y == minY);

            // Corners
            if (isTop && isLeft) return 1;     // sprite_1 (Top-Left corner)
            if (isTop && isRight) return 3;    // sprite_3 (Top-Right corner)
            if (isBottom && isLeft) return 7;  // sprite_7 (Bottom-Left corner)
            if (isBottom && isRight) return 6; // sprite_6 (Bottom-Right corner)

            // Edges
            if (isTop) return 5;               // sprite_5 (Top edge)
            if (isBottom) return 8;            // sprite_8 (Bottom edge)
            if (isLeft) return 2;              // sprite_2 (Left edge)
            if (isRight) return 4;             // sprite_4 (Right edge)

            // Inner Fill (center ground)
            return 0;                          // sprite_0 (Fill)
        }

        private static void BuildOstrichEnclosure(Transform parent, int minX, int maxX, int minY, int maxY, Sprite signSprite)
        {
            Sprite fencePost = AssetDatabase.LoadAssetAtPath<Sprite>($"{FENCE_SPRITES}/sprite_fence0.png");
            Sprite fenceRailH = AssetDatabase.LoadAssetAtPath<Sprite>($"{FENCE_SPRITES}/sprite_fence2.png");
            Sprite fenceRailV = AssetDatabase.LoadAssetAtPath<Sprite>($"{FENCE_SPRITES}/sprite_fence1.png");

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    bool isPerimeter = (x == minX || x == maxX || y == minY || y == maxY);
                    if (isPerimeter)
                    {
                        if (x == minX && y == 5) continue; // Gate opening

                        GameObject fence = new GameObject($"Ostrich_Fence_{x}_{y}");
                        fence.transform.SetParent(parent);
                        fence.transform.position = new Vector3(x, y, 0);

                        SpriteRenderer sr = fence.AddComponent<SpriteRenderer>();
                        bool isCorner = (x == minX || x == maxX) && (y == minY || y == maxY);
                        bool isHorizontal = (y == minY || y == maxY);

                        if (isCorner) sr.sprite = fencePost;
                        else if (isHorizontal) sr.sprite = fenceRailH;
                        else sr.sprite = fenceRailV;

                        fence.AddComponent<YSortRenderer>();

                        BoxCollider2D bc = fence.AddComponent<BoxCollider2D>();
                        bc.size = new Vector2(0.9f, 0.9f);
                    }
                }
            }

            // Signpost
            BuildSign(parent, signSprite, minX - 1, 5, "Recinto de Avestruces 🪶", "Avestruces veloces y curiosas. Acércate con [E] para alimentarlas o acariciarlas.");

            // Troughs (Food and Water)
            Sprite foodEmpty = AssetDatabase.LoadAssetAtPath<Sprite>($"{TROUGH_SPRITES}/FOOD_EMPTY/sprite_4.png");
            Sprite foodFull = AssetDatabase.LoadAssetAtPath<Sprite>($"{TROUGH_SPRITES}/FOOD/sprite_fullfood4.png");
            CreateTrough(parent, new Vector3(minX + 2, maxY - 1, 0), TroughType.Food, foodEmpty, foodFull, true);

            Sprite waterEmpty = AssetDatabase.LoadAssetAtPath<Sprite>($"{TROUGH_SPRITES}/WATER_EMPTY/sprite_04.png");
            Sprite waterFull = AssetDatabase.LoadAssetAtPath<Sprite>($"{TROUGH_SPRITES}/WATER/sprite_fullwater4.png");
            CreateTrough(parent, new Vector3(minX + 4, maxY - 1, 0), TroughType.Water, waterEmpty, waterFull, true);

            // Spawn Male Ostrich (Paco)
            Sprite[] maleFrames = LoadSpriteArray(OSTRICH_M_SPRITES, 8);
            SpawnOstrich(parent, "Ostrich_Macho", new Vector3(minX + 3.5f, minY + 3.5f, 0), "Paco", OstrichGender.Macho, maleFrames);

            // Spawn Female Ostrich (Pepa)
            Sprite[] femaleFrames = LoadSpriteArray(OSTRICH_F_SPRITES, 9);
            SpawnOstrich(parent, "Ostrich_Hembra", new Vector3(minX + 6.5f, minY + 2.5f, 0), "Pepa", OstrichGender.Hembra, femaleFrames);
        }

        private static void BuildElephantEnclosure(Transform parent, int minX, int maxX, int minY, int maxY, Sprite signSprite)
        {
            Sprite fencePost = AssetDatabase.LoadAssetAtPath<Sprite>($"{FENCE_SPRITES}/sprite_fence0.png");
            Sprite fenceRailH = AssetDatabase.LoadAssetAtPath<Sprite>($"{FENCE_SPRITES}/sprite_fence2.png");
            Sprite fenceRailV = AssetDatabase.LoadAssetAtPath<Sprite>($"{FENCE_SPRITES}/sprite_fence1.png");

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    bool isPerimeter = (x == minX || x == maxX || y == minY || y == maxY);
                    if (isPerimeter)
                    {
                        if (x == maxX && y == 5) continue; // Gate opening

                        GameObject fence = new GameObject($"Elephant_Fence_{x}_{y}");
                        fence.transform.SetParent(parent);
                        fence.transform.position = new Vector3(x, y, 0);

                        SpriteRenderer sr = fence.AddComponent<SpriteRenderer>();
                        bool isCorner = (x == minX || x == maxX) && (y == minY || y == maxY);
                        bool isHorizontal = (y == minY || y == maxY);

                        if (isCorner) sr.sprite = fencePost;
                        else if (isHorizontal) sr.sprite = fenceRailH;
                        else sr.sprite = fenceRailV;

                        fence.AddComponent<YSortRenderer>();

                        BoxCollider2D bc = fence.AddComponent<BoxCollider2D>();
                        bc.size = new Vector2(0.9f, 0.9f);
                    }
                }
            }

            BuildSign(parent, signSprite, maxX + 1, 5, "Sabana de Elefantes 🐘", "Familia de elefantes africanos. Atiende sus necesidades con tus herramientas o acarícialos.");

            // Habitat background layer
            Sprite habitatBg = AssetDatabase.LoadAssetAtPath<Sprite>($"{HABITAT_SPRITES}/habitat sin piedras.png");
            if (habitatBg != null)
            {
                GameObject bgObj = new GameObject("Elephant_Habitat_Ground");
                bgObj.transform.SetParent(parent);
                bgObj.transform.position = new Vector3(-9.5f, 6.5f, 0);
                SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
                bgSr.sprite = habitatBg;
                bgSr.sortingOrder = -90;
            }

            // Savanna Tree
            Sprite treeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{HABITAT_SPRITES}/arbol.png");
            if (treeSprite != null)
            {
                GameObject treeObj = new GameObject("Savanna_Tree");
                treeObj.transform.SetParent(parent);
                treeObj.transform.position = new Vector3(-13f, 8.5f, 0);
                SpriteRenderer treeSr = treeObj.AddComponent<SpriteRenderer>();
                treeSr.sprite = treeSprite;
                treeObj.AddComponent<YSortRenderer>();

                CircleCollider2D treeCol = treeObj.AddComponent<CircleCollider2D>();
                treeCol.radius = 0.5f;
                treeCol.offset = new Vector2(0, -0.3f);
            }

            // Rocks
            Sprite rockSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{HABITAT_SPRITES}/piedras.png");
            if (rockSprite != null)
            {
                GameObject rockObj = new GameObject("Savanna_Rocks");
                rockObj.transform.SetParent(parent);
                rockObj.transform.position = new Vector3(-6.5f, 8.5f, 0);
                SpriteRenderer rockSr = rockObj.AddComponent<SpriteRenderer>();
                rockSr.sprite = rockSprite;
                rockObj.AddComponent<YSortRenderer>();

                BoxCollider2D rockCol = rockObj.AddComponent<BoxCollider2D>();
                rockCol.size = new Vector2(3f, 0.8f);
            }

            // Spawn Elephant Family
            Sprite machoIdle = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteMparado.png");
            Sprite machoWalk1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteMcaminando1.png");
            Sprite machoWalk2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteMcaminando2.png");

            Sprite hembraIdle = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteHparado.png");
            Sprite hembraWalk1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteHcaminando1.png");
            Sprite hembraWalk2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteHcaminando2.png");

            Sprite bebeIdle = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteBparado.png");
            Sprite bebeWalk1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteBcaminando1.png");
            Sprite bebeWalk2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ELEPHANT_SPRITES}/elefanteBcaminando2.png");

            CreateElephantInstance("Elefante_Macho", new Vector3(-10.5f, 6.5f, 0), parent, "Tantor",
                ElephantStage.Macho, ElephantNeed.Hambre, machoIdle, machoWalk1, machoWalk2, 1.0f);

            CreateElephantInstance("Elefante_Hembra", new Vector3(-7f, 6.5f, 0), parent, "Ella",
                ElephantStage.Hembra, ElephantNeed.Sed, hembraIdle, hembraWalk1, hembraWalk2, 0.9f);

            CreateElephantInstance("Elefante_Bebe", new Vector3(-8.8f, 4.5f, 0), parent, "Babu",
                ElephantStage.Bebe, ElephantNeed.Sucio, bebeIdle, bebeWalk1, bebeWalk2, 0.65f);
        }

        private static void AddParkNatureDecorations(Transform envParent, Transform groundParent)
        {
            Sprite tree0 = AssetDatabase.LoadAssetAtPath<Sprite>($"{NATURE_SPRITES}/sprite_tree0.png");
            Sprite tree1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{NATURE_SPRITES}/sprite_tree1.png");
            Sprite bush = AssetDatabase.LoadAssetAtPath<Sprite>($"{NATURE_SPRITES}/bush (1).png");
            Sprite tuft0 = AssetDatabase.LoadAssetAtPath<Sprite>($"{NATURE_SPRITES}/sprite_grasstufts0.png");
            Sprite tuft1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{NATURE_SPRITES}/sprite_grasstufts1.png");
            Sprite tuft2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{NATURE_SPRITES}/sprite_grasstufts2.png");

            // Place Trees along walkways and park borders
            Vector3[] treePositions = {
                new Vector3(-4f, -4f, 0), new Vector3(4f, -4f, 0),
                new Vector3(-4f, -8f, 0), new Vector3(4f, -8f, 0),
                new Vector3(-4f, 2f, 0),  new Vector3(4f, 2f, 0),
                new Vector3(15f, -6f, 0), new Vector3(15f, 0f, 0)
            };

            for (int i = 0; i < treePositions.Length; i++)
            {
                Sprite sp = (i % 2 == 0) ? tree0 : tree1;
                if (sp != null)
                {
                    GameObject t = new GameObject($"Park_Tree_{i}");
                    t.transform.SetParent(envParent);
                    t.transform.position = treePositions[i];
                    SpriteRenderer sr = t.AddComponent<SpriteRenderer>();
                    sr.sprite = sp;
                    t.AddComponent<YSortRenderer>();

                    CircleCollider2D col = t.AddComponent<CircleCollider2D>();
                    col.radius = 0.4f;
                    col.offset = new Vector2(0, 0.2f);
                }
            }

            // Bushes
            Vector3[] bushPositions = {
                new Vector3(3.5f, 5.5f, 0), new Vector3(15.5f, 2.5f, 0),
                new Vector3(-2.5f, -3.5f, 0), new Vector3(2.5f, -3.5f, 0)
            };

            for (int i = 0; i < bushPositions.Length; i++)
            {
                if (bush != null)
                {
                    GameObject b = new GameObject($"Bush_{i}");
                    b.transform.SetParent(envParent);
                    b.transform.position = bushPositions[i];
                    SpriteRenderer sr = b.AddComponent<SpriteRenderer>();
                    sr.sprite = bush;
                    b.AddComponent<YSortRenderer>();
                }
            }

            // Grass Tufts (Ground decor)
            Sprite[] tufts = { tuft0, tuft1, tuft2 };
            Vector3[] tuftPositions = {
                new Vector3(-1f, -5f, 0), new Vector3(1f, -6f, 0),
                new Vector3(-6f, -2f, 0), new Vector3(7f, -3f, 0),
                new Vector3(10f, -6f, 0), new Vector3(12f, -4f, 0),
                new Vector3(-12f, -2f, 0), new Vector3(-8f, 2f, 0)
            };

            for (int i = 0; i < tuftPositions.Length; i++)
            {
                Sprite tuft = tufts[i % tufts.Length];
                if (tuft != null)
                {
                    GameObject tg = new GameObject($"GrassTuft_{i}");
                    tg.transform.SetParent(groundParent);
                    tg.transform.position = tuftPositions[i];
                    SpriteRenderer sr = tg.AddComponent<SpriteRenderer>();
                    sr.sprite = tuft;
                    sr.sortingOrder = -80;
                }
            }
        }

        private static void CreateTrough(Transform parent, Vector3 pos, TroughType type, Sprite empty, Sprite full, bool isFull)
        {
            string goName = type == TroughType.Food ? "Food_Trough" : "Water_Trough";
            GameObject troughObj = new GameObject(goName);
            troughObj.transform.SetParent(parent);
            troughObj.transform.position = pos;

            SpriteRenderer sr = troughObj.AddComponent<SpriteRenderer>();
            sr.sprite = isFull ? full : empty;
            troughObj.AddComponent<YSortRenderer>();

            BoxCollider2D bc = troughObj.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(0.9f, 0.7f);

            TroughInteractable ti = troughObj.AddComponent<TroughInteractable>();
            SerializedObject so = new SerializedObject(ti);
            so.FindProperty("type").enumValueIndex = (int)type;
            so.FindProperty("isFull").boolValue = isFull;
            so.FindProperty("emptySprite").objectReferenceValue = empty;
            so.FindProperty("fullSprite").objectReferenceValue = full;
            so.ApplyModifiedProperties();
        }

        private static void SpawnOstrich(Transform parent, string goName, Vector3 pos, string name, OstrichGender gender, Sprite[] frames)
        {
            GameObject obj = new GameObject(goName);
            obj.transform.SetParent(parent);
            obj.transform.position = pos;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            if (frames != null && frames.Length > 0) sr.sprite = frames[0];
            obj.AddComponent<YSortRenderer>();

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            col.offset = new Vector2(0, 0.3f);

            OstrichAnimal oa = obj.AddComponent<OstrichAnimal>();
            SerializedObject so = new SerializedObject(oa);
            so.FindProperty("ostrichName").stringValue = name;
            so.FindProperty("gender").enumValueIndex = (int)gender;

            SerializedProperty framesProp = so.FindProperty("animFrames");
            framesProp.arraySize = frames != null ? frames.Length : 0;
            for (int i = 0; i < framesProp.arraySize; i++)
            {
                framesProp.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            so.ApplyModifiedProperties();
        }

        private static Sprite[] LoadSpriteArray(string folder, int count)
        {
            List<Sprite> list = new List<Sprite>();
            for (int i = 0; i < count; i++)
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/sprite_{i}.png");
                if (s != null) list.Add(s);
            }
            return list.ToArray();
        }

        private static void BuildSign(Transform parent, Sprite signSprite, float x, float y, string title, string description)
        {
            GameObject sign = new GameObject($"Sign_{title}");
            sign.transform.SetParent(parent);
            sign.transform.position = new Vector3(x, y, 0);

            SpriteRenderer signSr = sign.AddComponent<SpriteRenderer>();
            signSr.sprite = signSprite;
            sign.AddComponent<YSortRenderer>();

            BoxCollider2D signCol = sign.AddComponent<BoxCollider2D>();
            signCol.size = new Vector2(0.8f, 0.8f);

            ZooEnclosureSign signComponent = sign.AddComponent<ZooEnclosureSign>();
            SerializedObject signSo = new SerializedObject(signComponent);
            signSo.FindProperty("enclosureName").stringValue = title;
            signSo.FindProperty("description").stringValue = description;
            signSo.ApplyModifiedProperties();
        }

        private static void CreateElephantInstance(string goName, Vector3 pos, Transform parent, string name,
            ElephantStage stage, ElephantNeed need, Sprite idle, Sprite w1, Sprite w2, float scale)
        {
            GameObject obj = new GameObject(goName);
            obj.transform.SetParent(parent);
            obj.transform.position = pos;
            obj.transform.localScale = Vector3.one * scale;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = idle;
            obj.AddComponent<YSortRenderer>();

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.6f;
            col.offset = new Vector2(0, 0.4f);

            ElephantAnimal ea = obj.AddComponent<ElephantAnimal>();
            SerializedObject so = new SerializedObject(ea);
            so.FindProperty("elephantName").stringValue = name;
            so.FindProperty("stage").enumValueIndex = (int)stage;
            so.FindProperty("currentNeed").enumValueIndex = (int)need;
            so.FindProperty("idleSprite").objectReferenceValue = idle;
            so.FindProperty("walk1Sprite").objectReferenceValue = w1;
            so.FindProperty("walk2Sprite").objectReferenceValue = w2;
            so.ApplyModifiedProperties();
        }

        private static void SetupHUD(GameObject player)
        {
            GameObject canvasObj = GameObject.Find("Zoo_Canvas");
            if (canvasObj != null) Object.DestroyImmediate(canvasObj);

            canvasObj = new GameObject("Zoo_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Instruction Banner at Top
            GameObject bannerObj = new GameObject("InstructionsBanner");
            bannerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bannerRect = bannerObj.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0f, 1f);
            bannerRect.anchorMax = new Vector2(1f, 1f);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.anchoredPosition = new Vector2(0, -10);
            bannerRect.sizeDelta = new Vector2(-40, 40);

            Image bannerBg = bannerObj.AddComponent<Image>();
            bannerBg.color = new Color(0.1f, 0.12f, 0.15f, 0.85f);

            GameObject bannerTextObj = new GameObject("Text");
            bannerTextObj.transform.SetParent(bannerObj.transform, false);
            Text bannerText = bannerTextObj.AddComponent<Text>();
            bannerText.text = "🌿 <b>Zoo Simulator</b> | [WASD / Flechas] Moverse | [Shift] Correr | [E] Interactuar y Cuidar";
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.fontSize = 15;
            bannerText.color = Color.white;
            bannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            RectTransform btRect = bannerTextObj.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.sizeDelta = Vector2.zero;

            // Interaction Prompt Box
            GameObject promptBox = new GameObject("InteractionPromptBox");
            promptBox.transform.SetParent(canvasObj.transform, false);
            RectTransform promptRect = promptBox.AddComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.5f, 0.16f);
            promptRect.anchorMax = new Vector2(0.5f, 0.16f);
            promptRect.pivot = new Vector2(0.5f, 0.5f);
            promptRect.sizeDelta = new Vector2(460, 48);

            Image promptBg = promptBox.AddComponent<Image>();
            promptBg.color = new Color(0.12f, 0.15f, 0.2f, 0.92f);

            GameObject promptTextObj = new GameObject("PromptText");
            promptTextObj.transform.SetParent(promptBox.transform, false);
            Text pText = promptTextObj.AddComponent<Text>();
            pText.text = "[E] Interactuar";
            pText.alignment = TextAnchor.MiddleCenter;
            pText.fontSize = 17;
            pText.color = new Color(1f, 0.9f, 0.3f);
            pText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            RectTransform ptRect = promptTextObj.GetComponent<RectTransform>();
            ptRect.anchorMin = Vector2.zero;
            ptRect.anchorMax = Vector2.one;
            ptRect.sizeDelta = Vector2.zero;

            // Bottom Stardew-Style Toolbar (Hotbar)
            SetupHotbar(canvasObj.transform);

            // Add InteractionHUD component
            InteractionHUD hud = canvasObj.AddComponent<InteractionHUD>();
            SerializedObject hudSo = new SerializedObject(hud);
            hudSo.FindProperty("promptContainer").objectReferenceValue = promptBox;
            hudSo.FindProperty("promptText").objectReferenceValue = pText;
            hudSo.FindProperty("playerInteraction").objectReferenceValue = player.GetComponent<PlayerInteraction>();
            hudSo.ApplyModifiedProperties();
        }

        private static void SetupHotbar(Transform parent)
        {
            GameObject hotbarObj = new GameObject("Zoo_Hotbar");
            hotbarObj.transform.SetParent(parent, false);
            RectTransform hr = hotbarObj.AddComponent<RectTransform>();
            hr.anchorMin = new Vector2(0.5f, 0.04f);
            hr.anchorMax = new Vector2(0.5f, 0.04f);
            hr.pivot = new Vector2(0.5f, 0.5f);
            hr.sizeDelta = new Vector2(340, 60);

            Image hotbarBg = hotbarObj.AddComponent<Image>();
            hotbarBg.color = new Color(0.15f, 0.12f, 0.09f, 0.9f);

            HorizontalLayoutGroup hlg = hotbarObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(10, 10, 6, 6);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ITEM_SPRITES}/marquito.png");
            string[] toolNames = { "comida.png", "cubeta.png", "escoba.png", "guante.png", "medicina.png" };

            for (int i = 0; i < toolNames.Length; i++)
            {
                GameObject slot = new GameObject($"Slot_{i + 1}");
                slot.transform.SetParent(hotbarObj.transform, false);
                RectTransform slotRt = slot.AddComponent<RectTransform>();
                slotRt.sizeDelta = new Vector2(48, 48);

                Image slotBg = slot.AddComponent<Image>();
                if (frameSprite != null) slotBg.sprite = frameSprite;
                else slotBg.color = new Color(0.28f, 0.22f, 0.16f);

                // Tool Icon inside
                Sprite toolSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ITEM_SPRITES}/{toolNames[i]}");
                if (toolSprite != null)
                {
                    GameObject iconObj = new GameObject("Icon");
                    iconObj.transform.SetParent(slot.transform, false);
                    RectTransform iconRt = iconObj.AddComponent<RectTransform>();
                    iconRt.sizeDelta = new Vector2(34, 34);

                    Image iconImg = iconObj.AddComponent<Image>();
                    iconImg.sprite = toolSprite;
                    iconImg.preserveAspect = true;
                }
            }
        }

        private static Sprite CreatePixelSprite(string name, int width, int height, System.Func<int, int, Color> colorGen, float ppu = 16f, Vector2? pivot = null)
        {
            string path = $"{SPRITES_FOLDER}/{name}.png";
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, colorGen(x, y));
                }
            }
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = ppu;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                if (pivot.HasValue)
                {
                    importer.spritePivot = pivot.Value;
                    importer.spriteImportMode = SpriteImportMode.Single;
                }
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = Path.GetDirectoryName(folderPath).Replace("\\", "/");
                string folderName = Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
