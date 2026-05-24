using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudioIyan.TextureVault
{
    public static class TextureVaultScanner
    {
        public static TextureVaultScanResult Scan(GameObject rootObject, TextureVaultScanOptions options)
        {
            if (rootObject == null)
            {
                throw new ArgumentNullException(nameof(rootObject));
            }

            options ??= new TextureVaultScanOptions();

            var result = new TextureVaultScanResult
            {
                rootObject = rootObject
            };

            var textureByGuid = new Dictionary<string, TextureVaultScannedTexture>();
            var uniqueMaterials = new HashSet<int>();
            var usageKeys = new HashSet<string>();
            Renderer[] renderers = rootObject.GetComponentsInChildren<Renderer>(options.includeInactiveChildren);
            result.foundRenderers = renderers.Length;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                string rendererPath = GetTransformPath(rootObject.transform, renderer.transform);
                Material[] materials = renderer.sharedMaterials;
                foreach (Material material in materials)
                {
                    if (material == null)
                    {
                        continue;
                    }

                    uniqueMaterials.Add(material.GetInstanceID());
                    ScanShaderTextureProperties(rootObject, rendererPath, material, options, result, textureByGuid, usageKeys);

                    if (options.includeSerializedMaterialTextureFallback)
                    {
                        ScanSerializedTextureFallback(rootObject, rendererPath, material, options, result, textureByGuid, usageKeys);
                    }
                }
            }

            result.foundMaterials = uniqueMaterials.Count;
            result.textures.AddRange(textureByGuid.Values);
            result.textures.Sort((left, right) => string.Compare(left.assetPath, right.assetPath, StringComparison.OrdinalIgnoreCase));

            TextureVaultLogger.Log($"Scanned {result.textures.Count} textures from {rootObject.name}.");
            return result;
        }

        private static void ScanShaderTextureProperties(
            GameObject rootObject,
            string rendererPath,
            Material material,
            TextureVaultScanOptions options,
            TextureVaultScanResult result,
            Dictionary<string, TextureVaultScannedTexture> textureByGuid,
            HashSet<string> usageKeys)
        {
            Shader shader = material.shader;
            if (shader == null)
            {
                return;
            }

            int propertyCount;
            try
            {
                propertyCount = shader.GetPropertyCount();
            }
            catch (Exception exception)
            {
                TextureVaultLogger.Warning($"Could not inspect shader properties on {shader.name}: {exception.Message}");
                return;
            }

            for (int index = 0; index < propertyCount; index++)
            {
                if (shader.GetPropertyType(index) != ShaderPropertyType.Texture)
                {
                    continue;
                }

                string propertyName = shader.GetPropertyName(index);
                string propertyDescription = shader.GetPropertyDescription(index);
                Texture texture;
                try
                {
                    texture = material.GetTexture(propertyName);
                }
                catch (Exception exception)
                {
                    TextureVaultLogger.Warning($"Could not read texture property {propertyName} on material {material.name}: {exception.Message}");
                    continue;
                }

                TryAddTexture(rootObject, rendererPath, material, shader, propertyName, propertyDescription, texture, "ShaderProperty", options, result, textureByGuid, usageKeys);
            }
        }

        private static void ScanSerializedTextureFallback(
            GameObject rootObject,
            string rendererPath,
            Material material,
            TextureVaultScanOptions options,
            TextureVaultScanResult result,
            Dictionary<string, TextureVaultScannedTexture> textureByGuid,
            HashSet<string> usageKeys)
        {
            try
            {
                var serializedObject = new SerializedObject(material);
                SerializedProperty texEnvs = serializedObject.FindProperty("m_SavedProperties.m_TexEnvs");
                if (texEnvs == null || !texEnvs.isArray)
                {
                    return;
                }

                for (int index = 0; index < texEnvs.arraySize; index++)
                {
                    SerializedProperty element = texEnvs.GetArrayElementAtIndex(index);
                    SerializedProperty nameProperty = element.FindPropertyRelative("first");
                    SerializedProperty textureProperty = element.FindPropertyRelative("second.m_Texture");
                    string propertyName = nameProperty != null ? nameProperty.stringValue : string.Empty;
                    Texture texture = textureProperty != null ? textureProperty.objectReferenceValue as Texture : null;
                    TryAddTexture(rootObject, rendererPath, material, material.shader, propertyName, propertyName, texture, "SerializedFallback", options, result, textureByGuid, usageKeys);
                }
            }
            catch (Exception exception)
            {
                TextureVaultLogger.Warning($"Serialized material texture fallback skipped for {material.name}: {exception.Message}");
            }
        }

        private static void TryAddTexture(
            GameObject rootObject,
            string rendererPath,
            Material material,
            Shader shader,
            string propertyName,
            string propertyDescription,
            Texture texture,
            string source,
            TextureVaultScanOptions options,
            TextureVaultScanResult result,
            Dictionary<string, TextureVaultScannedTexture> textureByGuid,
            HashSet<string> usageKeys)
        {
            if (texture == null)
            {
                return;
            }

            if (texture is not Texture2D texture2D)
            {
                result.skippedTextures++;
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(texture2D);
            if (string.IsNullOrEmpty(assetPath))
            {
                result.skippedTextures++;
                return;
            }

            if (options.skipBuiltInOrNonAssetsTextures && !assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                result.skippedTextures++;
                return;
            }

            if (options.skipPackageCacheTextures &&
                (assetPath.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase) ||
                 assetPath.IndexOf("PackageCache", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                result.skippedTextures++;
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                result.skippedTextures++;
                return;
            }

            if (!textureByGuid.TryGetValue(guid, out TextureVaultScannedTexture scannedTexture))
            {
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    result.skippedTextures++;
                    return;
                }

                scannedTexture = new TextureVaultScannedTexture
                {
                    texture = texture2D,
                    guid = guid,
                    assetPath = assetPath,
                    textureName = texture2D.name,
                    width = texture2D.width,
                    height = texture2D.height,
                    importer = importer,
                    standaloneSettings = importer.GetPlatformTextureSettings(TextureVaultConstants.StandalonePlatformName)
                };

                textureByGuid.Add(guid, scannedTexture);
            }

            string materialPath = AssetDatabase.GetAssetPath(material);
            string usageKey = $"{guid}|{rendererPath}|{material.GetInstanceID()}|{propertyName}";
            if (!usageKeys.Add(usageKey))
            {
                return;
            }

            scannedTexture.usages.Add(new TextureVaultUsageRecord
            {
                rendererPath = rendererPath,
                materialAssetPath = materialPath,
                materialName = material.name,
                shaderName = shader != null ? shader.name : string.Empty,
                texturePropertyName = propertyName,
                texturePropertyDescription = propertyDescription,
                source = source
            });
        }

        private static string GetTransformPath(Transform root, Transform current)
        {
            if (root == null || current == null)
            {
                return string.Empty;
            }

            if (root == current)
            {
                return root.name;
            }

            var names = new Stack<string>();
            Transform cursor = current;
            while (cursor != null)
            {
                names.Push(cursor.name);
                if (cursor == root)
                {
                    break;
                }

                cursor = cursor.parent;
            }

            return string.Join("/", names.ToArray());
        }
    }
}
