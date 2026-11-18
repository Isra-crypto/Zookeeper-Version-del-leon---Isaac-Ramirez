using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZooGame.Editor
{
    public static class ElephantSetupUtility
    {
        private const string SOURCE_ELEPHANTS = "Elefantes animacion";
        private const string SOURCE_HABITAT = "Habitat";
        private const string SOURCE_OSTRICH_MALE = "AVESTRUZ/AVESTRUZ/ostrich";
        private const string SOURCE_OSTRICH_FEMALE = "AVESTRUZ/AVESTRUZ HEMBRA/FEMALE";
        private const string SOURCE_MISC_TILESET = "MISC/ENCLOSURE/TILESET";
        private const string SOURCE_MISC_FENCES = "MISC/ENCLOSURE";
        private const string SOURCE_MISC_NATURE = "MISC/NATURE";
        private const string SOURCE_MISC_FOOD_WATER = "MISC/FOOD-WATER";

        private const string DEST_ANIMALS_ELEPHANT = "Assets/Sprites/Animals/Elefante";
        private const string DEST_ANIMALS_OSTRICH_M = "Assets/Sprites/Animals/Avestruz/Macho";
        private const string DEST_ANIMALS_OSTRICH_F = "Assets/Sprites/Animals/Avestruz/Hembra";
        private const string DEST_HABITAT = "Assets/Sprites/Habitat";
        private const string DEST_TILESET = "Assets/Sprites/Tilesets/Enclosure";
        private const string DEST_FENCES = "Assets/Sprites/Fences";
        private const string DEST_NATURE = "Assets/Sprites/Nature";
        private const string DEST_TROUGHS = "Assets/Sprites/Troughs";
        private const string DEST_EMOTES = "Assets/Sprites/Emotes";
        private const string DEST_ITEMS = "Assets/Sprites/Items";

        [MenuItem("ZooGame/Import and Setup Elephant Assets")]
        public static void ImportAndSetupElephantAssets()
        {
            EnsureFolder("Assets/Sprites");
            EnsureFolder("Assets/Sprites/Animals");
            EnsureFolder("Assets/Sprites/Animals/Avestruz");
            EnsureFolder("Assets/Sprites/Tilesets");
            EnsureFolder(DEST_ANIMALS_ELEPHANT);
            EnsureFolder(DEST_ANIMALS_OSTRICH_M);
            EnsureFolder(DEST_ANIMALS_OSTRICH_F);
            EnsureFolder(DEST_HABITAT);
            EnsureFolder(DEST_TILESET);
            EnsureFolder(DEST_FENCES);
            EnsureFolder(DEST_NATURE);
            EnsureFolder(DEST_TROUGHS);
            EnsureFolder(DEST_EMOTES);
            EnsureFolder(DEST_ITEMS);

            // 1. Elephants
            ImportFolder(SOURCE_ELEPHANTS, DEST_ANIMALS_ELEPHANT, 180f, new Vector2(0.5f, 0.28f));

            // 2. Habitat
            ImportHabitatFolder(SOURCE_HABITAT, DEST_HABITAT);

            // 3. Ostriches (Native 32x32)
            ImportFolder(SOURCE_OSTRICH_MALE, DEST_ANIMALS_OSTRICH_M, 16f, new Vector2(0.5f, 0.12f));
            ImportFolder(SOURCE_OSTRICH_FEMALE, DEST_ANIMALS_OSTRICH_F, 16f, new Vector2(0.5f, 0.12f));

            // 4. Enclosure Tileset (Native 16x16)
            ImportFolder(SOURCE_MISC_TILESET, DEST_TILESET, 16f, new Vector2(0.5f, 0.5f));

            // 5. Fences (32x32)
            ImportFolder(SOURCE_MISC_FENCES, DEST_FENCES, 16f, new Vector2(0.5f, 0.15f));

            // 6. Nature (Trees, Bush, Grass Tufts)
            ImportFolder(SOURCE_MISC_NATURE, DEST_NATURE, 16f, new Vector2(0.5f, 0.15f));

            // 7. Food and Water Troughs
            if (Directory.Exists(SOURCE_MISC_FOOD_WATER))
            {
                ImportFolder(Path.Combine(SOURCE_MISC_FOOD_WATER, "FOOD"), Path.Combine(DEST_TROUGHS, "FOOD").Replace("\\", "/"), 16f, new Vector2(0.5f, 0.5f));
                ImportFolder(Path.Combine(SOURCE_MISC_FOOD_WATER, "FOOD TROUGH EMPTY"), Path.Combine(DEST_TROUGHS, "FOOD_EMPTY").Replace("\\", "/"), 16f, new Vector2(0.5f, 0.5f));
                ImportFolder(Path.Combine(SOURCE_MISC_FOOD_WATER, "WATER"), Path.Combine(DEST_TROUGHS, "WATER").Replace("\\", "/"), 16f, new Vector2(0.5f, 0.5f));
                ImportFolder(Path.Combine(SOURCE_MISC_FOOD_WATER, "WATER TROUGH EMPTY"), Path.Combine(DEST_TROUGHS, "WATER_EMPTY").Replace("\\", "/"), 16f, new Vector2(0.5f, 0.5f));
            }

            AssetDatabase.Refresh();
            Debug.Log("<color=#44FF44><b>[Zoo Game]</b> ¡Todos los assets (Avestruces, Tilesets, Vallas, Naturaleza y Comederos) importados y calibrados!</color>");
        }

        private static void ImportFolder(string source, string dest, float ppu, Vector2 pivot)
        {
            if (!Directory.Exists(source)) return;
            EnsureFolder(dest);

            string[] files = Directory.GetFiles(source, "*.png");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string targetPath = Path.Combine(dest, fileName).Replace("\\", "/");
                File.Copy(file, targetPath, true);
                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.spritePixelsPerUnit = ppu;
                    importer.spritePivot = pivot;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void ImportHabitatFolder(string source, string dest)
        {
            if (!Directory.Exists(source)) return;
            EnsureFolder(dest);

            string[] files = Directory.GetFiles(source, "*.png");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string targetPath = Path.Combine(dest, fileName).Replace("\\", "/");
                File.Copy(file, targetPath, true);
                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;

                    if (fileName.ToLowerInvariant().Contains("arbol"))
                    {
                        importer.spritePixelsPerUnit = 70f;
                        importer.spritePivot = new Vector2(0.5f, 0.62f);
                    }
                    else if (fileName.ToLowerInvariant().Contains("piedras"))
                    {
                        importer.spritePixelsPerUnit = 70f;
                        importer.spritePivot = new Vector2(0.5f, 0.4f);
                    }
                    else if (fileName.ToLowerInvariant().Contains("habitat"))
                    {
                        importer.spritePixelsPerUnit = 60f;
                        importer.spritePivot = new Vector2(0.5f, 0.48f);
                    }
                    else
                    {
                        importer.spritePixelsPerUnit = 100f;
                        importer.spritePivot = new Vector2(0.5f, 0.5f);
                    }

                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    EnsureFolder(parent);
                }
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
