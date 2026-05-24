using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace StudioIyan.TextureVault
{
    public sealed class TextureVaultWindow : EditorWindow
    {
        private GameObject _rootObject;
        private bool _includeInactiveChildren = true;
        private bool _skipPackageCacheTextures = true;
        private bool _skipBuiltInOrNonAssetsTextures = true;
        private bool _showConfirmationBeforeReimport = true;
        private bool _includeSerializedFallback = true;

        private TextureVaultScanResult _scanResult;
        private TextureVaultSnapshotData _loadedSnapshot;
        private string _lastSnapshotPath = string.Empty;
        private bool _snapshotSavedForCurrentScan;
        private Vector2 _scrollPosition;

        [MenuItem("Studio Iyan/Tools/Texture Vault")]
        public static void Open()
        {
            var window = GetWindow<TextureVaultWindow>("Texture Vault");
            window.minSize = new Vector2(780, 480);
            window.Show();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawRootAndOptions();
            DrawActions();
            DrawSummary();
            DrawResultTable();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Studio Iyan Texture Vault", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Save texture import settings, apply a temporary Standalone BC7 / Max Size 4096 profile, and restore the original settings.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(8);
        }

        private void DrawRootAndOptions()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var newRootObject = (GameObject)EditorGUILayout.ObjectField("Root GameObject", _rootObject, typeof(GameObject), true);
                if (newRootObject != _rootObject)
                {
                    _rootObject = newRootObject;
                    _scanResult = null;
                    _snapshotSavedForCurrentScan = false;
                }
                _includeInactiveChildren = EditorGUILayout.ToggleLeft("Include inactive children", _includeInactiveChildren);
                _skipPackageCacheTextures = EditorGUILayout.ToggleLeft("Skip PackageCache textures", _skipPackageCacheTextures);
                _skipBuiltInOrNonAssetsTextures = EditorGUILayout.ToggleLeft("Skip built-in or non-Assets textures", _skipBuiltInOrNonAssetsTextures);
                _showConfirmationBeforeReimport = EditorGUILayout.ToggleLeft("Show confirmation before reimport", _showConfirmationBeforeReimport);
                _includeSerializedFallback = EditorGUILayout.ToggleLeft("Use safe serialized material texture fallback", _includeSerializedFallback);
            }
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan", GUILayout.Height(28)))
                {
                    Scan();
                }

                if (GUILayout.Button("Save Snapshot JSON", GUILayout.Height(28)))
                {
                    SaveSnapshotWithDialog();
                }

                if (GUILayout.Button("Load Snapshot JSON", GUILayout.Height(28)))
                {
                    LoadSnapshotWithDialog();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply BC7 / Max Size 4096", GUILayout.Height(30)))
                {
                    ApplyProfile();
                }

                if (GUILayout.Button("Restore From Snapshot", GUILayout.Height(30)))
                {
                    RestoreFromSnapshot();
                }
            }
        }

        private void DrawSummary()
        {
            int foundTextures = _scanResult != null ? _scanResult.textures.Count : 0;
            int foundRenderers = _scanResult != null ? _scanResult.foundRenderers : 0;
            int foundMaterials = _scanResult != null ? _scanResult.foundMaterials : 0;
            int skippedTextures = _scanResult != null ? _scanResult.skippedTextures : 0;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Found renderers", foundRenderers.ToString());
                EditorGUILayout.LabelField("Found materials", foundMaterials.ToString());
                EditorGUILayout.LabelField("Found textures", foundTextures.ToString());
                EditorGUILayout.LabelField("Skipped textures", skippedTextures.ToString());
                EditorGUILayout.LabelField("Last snapshot path", string.IsNullOrEmpty(_lastSnapshotPath) ? "(none)" : _lastSnapshotPath);
            }
        }

        private void DrawResultTable()
        {
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_scanResult == null || _scanResult.textures.Count == 0)
            {
                EditorGUILayout.HelpBox("No scan results yet.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            foreach (TextureVaultScannedTexture texture in _scanResult.textures)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    string formatLabel = texture.standaloneSettings.overridden
                        ? texture.standaloneSettings.format.ToString()
                        : $"Default ({texture.standaloneSettings.format})";
                    string sizeLabel = texture.standaloneSettings.overridden
                        ? texture.standaloneSettings.maxTextureSize.ToString()
                        : $"Default ({texture.standaloneSettings.maxTextureSize})";

                    EditorGUILayout.LabelField(texture.textureName, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Asset path", texture.assetPath);
                    EditorGUILayout.LabelField("Width x height", $"{texture.width} x {texture.height}");
                    EditorGUILayout.LabelField("Current Standalone max size", sizeLabel);
                    EditorGUILayout.LabelField("Current Standalone format", formatLabel);
                    EditorGUILayout.LabelField("Used by", BuildUsageSummary(texture));
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void Scan()
        {
            if (_rootObject == null)
            {
                EditorUtility.DisplayDialog("Texture Vault", "Assign a root GameObject before scanning.", "OK");
                return;
            }

            var options = new TextureVaultScanOptions
            {
                includeInactiveChildren = _includeInactiveChildren,
                skipPackageCacheTextures = _skipPackageCacheTextures,
                skipBuiltInOrNonAssetsTextures = _skipBuiltInOrNonAssetsTextures,
                includeSerializedMaterialTextureFallback = _includeSerializedFallback
            };

            _scanResult = TextureVaultScanner.Scan(_rootObject, options);
            _snapshotSavedForCurrentScan = false;
            Repaint();
        }

        private bool SaveSnapshotWithDialog()
        {
            if (!EnsureScanExists())
            {
                return false;
            }

            string defaultPath = TextureVaultJson.BuildDefaultSnapshotAssetPath(_rootObject.name, DateTime.UtcNow);
            EnsureSnapshotFolderExists();

            string savePath = EditorUtility.SaveFilePanelInProject(
                "Save Texture Vault Snapshot",
                Path.GetFileName(defaultPath),
                "json",
                "Choose where to save the Texture Vault snapshot JSON.",
                TextureVaultConstants.SnapshotFolder);

            if (string.IsNullOrEmpty(savePath))
            {
                return false;
            }

            return SaveSnapshot(savePath);
        }

        private bool SaveSnapshot(string assetPath)
        {
            var snapshot = TextureVaultSnapshot.Create(_rootObject, _scanResult);
            TextureVaultJson.Save(snapshot, assetPath);
            _loadedSnapshot = snapshot;
            _lastSnapshotPath = assetPath;
            _snapshotSavedForCurrentScan = true;
            return true;
        }

        private void LoadSnapshotWithDialog()
        {
            string selectedPath = EditorUtility.OpenFilePanel("Load Texture Vault Snapshot", "Assets", "json");
            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            string projectPath = Directory.GetCurrentDirectory().Replace("\\", "/");
            string normalizedSelectedPath = selectedPath.Replace("\\", "/");
            string assetPath = normalizedSelectedPath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase)
                ? normalizedSelectedPath.Substring(projectPath.Length).TrimStart('/')
                : selectedPath;

            try
            {
                _loadedSnapshot = TextureVaultJson.Load(assetPath);
                _lastSnapshotPath = assetPath;
                TextureVaultLogger.Log($"Snapshot loaded explicitly for restore: {assetPath}");
            }
            catch (Exception exception)
            {
                EditorUtility.DisplayDialog("Texture Vault", $"Failed to load snapshot JSON:\n{exception.Message}", "OK");
                TextureVaultLogger.Error($"Failed to load snapshot JSON: {exception.Message}");
            }
        }

        private void ApplyProfile()
        {
            if (!EnsureScanExists())
            {
                return;
            }

            int textureCount = _scanResult.textures.Count;
            if (textureCount == 0)
            {
                EditorUtility.DisplayDialog("Texture Vault", "No textures were found in the current scan.", "OK");
                return;
            }

            if (!_snapshotSavedForCurrentScan)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Texture Vault",
                    $"No snapshot has been saved for the current scan.\n\nThis will reimport {textureCount} textures and modify Texture Import Settings. Source image files will not be edited.",
                    "Save Snapshot First",
                    "Apply Without Snapshot",
                    "Cancel");

                if (choice == 0)
                {
                    string defaultPath = TextureVaultJson.BuildDefaultSnapshotAssetPath(_rootObject.name, DateTime.UtcNow);
                    EnsureSnapshotFolderExists();
                    if (!SaveSnapshot(defaultPath))
                    {
                        return;
                    }
                }
                else if (choice == 1)
                {
                    TextureVaultLogger.Warning("User explicitly chose to apply BC7 / Max Size 4096 without a current snapshot.");
                }
                else
                {
                    return;
                }
            }

            if (_showConfirmationBeforeReimport &&
                !EditorUtility.DisplayDialog(
                    "Apply BC7 / Max Size 4096",
                    $"This will reimport {textureCount} textures.\n\nThis modifies Texture Import Settings only. Source image files will not be edited.",
                    "Apply BC7 / Max Size 4096",
                    "Cancel"))
            {
                return;
            }

            TextureVaultOperationSummary summary = TextureVaultApplier.ApplyBc7MaxSize4096(_scanResult.textures);
            EditorUtility.DisplayDialog("Texture Vault", $"Apply finished.\nApplied: {summary.AppliedOrRestored}\nSkipped: {summary.Skipped}\nFailed: {summary.Failed}", "OK");
            Scan();
        }

        private void RestoreFromSnapshot()
        {
            if (_loadedSnapshot == null)
            {
                LoadSnapshotWithDialog();
                if (_loadedSnapshot == null)
                {
                    return;
                }
            }

            int textureCount = _loadedSnapshot.textures != null ? _loadedSnapshot.textures.Count : 0;
            if (textureCount == 0)
            {
                EditorUtility.DisplayDialog("Texture Vault", "The loaded snapshot does not contain texture records.", "OK");
                return;
            }

            if (_showConfirmationBeforeReimport &&
                !EditorUtility.DisplayDialog(
                    "Restore Import Settings",
                    $"This will reimport {textureCount} textures from the loaded snapshot.\n\nThis modifies Texture Import Settings only. Source image files will not be edited.",
                    "Restore Import Settings",
                    "Cancel"))
            {
                return;
            }

            TextureVaultOperationSummary summary = TextureVaultRestore.RestoreFromSnapshot(_loadedSnapshot);
            EditorUtility.DisplayDialog("Texture Vault", $"Restore finished.\nRestored: {summary.AppliedOrRestored}\nSkipped: {summary.Skipped}\nFailed: {summary.Failed}", "OK");
            if (_rootObject != null)
            {
                Scan();
            }
        }

        private bool EnsureScanExists()
        {
            if (_scanResult != null)
            {
                return true;
            }

            if (_rootObject == null)
            {
                EditorUtility.DisplayDialog("Texture Vault", "Assign a root GameObject first.", "OK");
                return false;
            }

            Scan();
            return _scanResult != null;
        }

        private static void EnsureSnapshotFolderExists()
        {
            string fullPath = TextureVaultJson.ToFullPath(TextureVaultConstants.SnapshotFolder);
            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }

        private static string BuildUsageSummary(TextureVaultScannedTexture texture)
        {
            if (texture == null || texture.usages == null || texture.usages.Count == 0)
            {
                return "(no usage records)";
            }

            var builder = new StringBuilder();
            for (int index = 0; index < texture.usages.Count; index++)
            {
                TextureVaultUsageRecord usage = texture.usages[index];
                if (index > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(usage.rendererPath);
                builder.Append(" | ");
                builder.Append(usage.materialName);
                builder.Append(" | ");
                builder.Append(usage.texturePropertyName);
                if (!string.IsNullOrEmpty(usage.texturePropertyDescription) && usage.texturePropertyDescription != usage.texturePropertyName)
                {
                    builder.Append(" (");
                    builder.Append(usage.texturePropertyDescription);
                    builder.Append(")");
                }
            }

            return builder.ToString();
        }
    }
}
