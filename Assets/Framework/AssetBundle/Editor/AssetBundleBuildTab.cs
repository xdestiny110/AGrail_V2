﻿using UnityEditor;
using System.Collections.Generic;
using System.IO;

using UnityEngine.AssetBundles.AssetBundleDataSource;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityEngine.AssetBundles
{
    [System.Serializable]
    public class AssetBundleBuildTab
    {
        const string k_BuildPrefPrefix = "ABBBuild:";
        // gui vars
        [SerializeField]
        private ValidBuildTarget m_BuildTarget = ValidBuildTarget.StandaloneWindows;
        [SerializeField]
        private CompressOptions m_Compression = CompressOptions.StandardCompression;
        [SerializeField]
        private string m_protoBinPath = "Assets/ProtoFile";

        private string m_OutputPath = string.Empty;
        [SerializeField]
        private bool m_UseDefaultPath = true;
        private string m_streamingPath = "Assets/StreamingAssets";

        [SerializeField]
        private bool m_AdvancedSettings;

        [SerializeField]
        private Vector2 m_ScrollPosition;

        private string oldCheckFileStr = null;


        class ToggleData
        {
            public ToggleData(bool s,

                string title,

                string tooltip,
                List<string> onToggles,
                BuildAssetBundleOptions opt = BuildAssetBundleOptions.None)
            {
                if (onToggles.Contains(title))
                    state = true;
                else
                    state = s;
                content = new GUIContent(title, tooltip);
                option = opt;
            }
            //public string prefsKey
            //{ get { return k_BuildPrefPrefix + content.text; } }
            public bool state;
            public GUIContent content;
            public BuildAssetBundleOptions option;
        }

        [SerializeField]
        private List<string> m_OnToggles;
        List<ToggleData> m_ToggleData;
        ToggleData m_ForceRebuild;
        ToggleData m_CopyToStreaming;
        GUIContent m_TargetContent;
        GUIContent m_CompressionContent;
        public enum CompressOptions
        {
            Uncompressed = 0,
            StandardCompression,
            ChunkBasedCompression,
        }
        GUIContent[] m_CompressionOptions =
        {
            new GUIContent("No Compression"),
            new GUIContent("Standard Compression (LZMA)"),
            new GUIContent("Chunk Based Compression (LZ4)")
        };
        int[] m_CompressionValues = { 0, 1, 2 };


        public AssetBundleBuildTab()
        {
            m_AdvancedSettings = false;
            m_OnToggles = new List<string>();
            m_UseDefaultPath = true;
        }

        public void OnEnable(Rect pos, EditorWindow parent)
        {
            m_ToggleData = new List<ToggleData>();
            m_ToggleData.Add(new ToggleData(
                false,
                "Exclude Type Information",
                "Do not include type information within the asset bundle (don't write type tree).",
                m_OnToggles,
                BuildAssetBundleOptions.DisableWriteTypeTree));
            m_ToggleData.Add(new ToggleData(
                false,
                "Force Rebuild",
                "Force rebuild the asset bundles",
                m_OnToggles,
                BuildAssetBundleOptions.ForceRebuildAssetBundle));
            m_ToggleData.Add(new ToggleData(
                false,
                "Ignore Type Tree Changes",
                "Ignore the type tree changes when doing the incremental build check.",
                m_OnToggles,
                BuildAssetBundleOptions.IgnoreTypeTreeChanges));
            m_ToggleData.Add(new ToggleData(
                false,
                "Append Hash",
                "Append the hash to the assetBundle name.",
                m_OnToggles,
                BuildAssetBundleOptions.AppendHashToAssetBundleName));
            m_ToggleData.Add(new ToggleData(
                false,
                "Strict Mode",
                "Do not allow the build to succeed if any errors are reporting during it.",
                m_OnToggles,
                BuildAssetBundleOptions.StrictMode));
            m_ToggleData.Add(new ToggleData(
                false,
                "Dry Run Build",
                "Do a dry run build.",
                m_OnToggles,
                BuildAssetBundleOptions.DryRunBuild));


            m_ForceRebuild = new ToggleData(
                false,
                "Clear Folders",
                "Will wipe out all contents of build directory as well as StreamingAssets/AssetBundles if you are choosing to copy build there.",
                m_OnToggles);
            m_CopyToStreaming = new ToggleData(
                false,
                "Copy to StreamingAssets",
                "After build completes, will copy all build content to " + m_streamingPath + " for use in stand-alone player.",
                m_OnToggles);

            m_TargetContent = new GUIContent("Build Target", "Choose target platform to build for.");
            m_CompressionContent = new GUIContent("Compression", "Choose no compress, standard (LZMA), or chunk based (LZ4)");

            if(m_UseDefaultPath)
            {
                ResetPathToDefault();
            }
        }

        public void OnGUI(Rect pos)
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            bool newState = false;

            //basic options
            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            // build target
            using (new EditorGUI.DisabledScope (!AssetBundleModel.Model.DataSource.CanSpecifyBuildTarget)) {
                ValidBuildTarget tgt = (ValidBuildTarget)EditorGUILayout.EnumPopup(m_TargetContent, m_BuildTarget);
                if (tgt != m_BuildTarget)
                {
                    m_BuildTarget = tgt;
                    if(m_UseDefaultPath)
                    {
                        m_OutputPath = "AssetBundles/";
                        m_OutputPath += m_BuildTarget.ToString();
                        EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
                    }
                }
            }

            // proto bin path
            using (new EditorGUI.DisabledScope(false))
            {
                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();
                var newPath = EditorGUILayout.TextField("Proto bin Path", m_protoBinPath);
                GUILayout.EndHorizontal();
            }

            ////output path
            using (new EditorGUI.DisabledScope (!AssetBundleModel.Model.DataSource.CanSpecifyBuildOutputDirectory)) {
                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();
                var newPath = EditorGUILayout.TextField("Output Path", m_OutputPath);
                if (newPath != m_OutputPath)
                {
                    m_UseDefaultPath = false;
                    m_OutputPath = newPath;
                    EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
                    BrowseForFolder();
                if (GUILayout.Button("Reset", GUILayout.MaxWidth(75f)))
                    ResetPathToDefault();
                if (string.IsNullOrEmpty(m_OutputPath))
                    m_OutputPath = EditorUserBuildSettings.GetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath");
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();

                newState = GUILayout.Toggle(
                    m_ForceRebuild.state,
                    m_ForceRebuild.content);
                if (newState != m_ForceRebuild.state)
                {
                    if (newState)
                        m_OnToggles.Add(m_ForceRebuild.content.text);
                    else
                        m_OnToggles.Remove(m_ForceRebuild.content.text);
                    m_ForceRebuild.state = newState;
                }
                newState = GUILayout.Toggle(
                    m_CopyToStreaming.state,
                    m_CopyToStreaming.content);
                if (newState != m_CopyToStreaming.state)
                {
                    if (newState)
                        m_OnToggles.Add(m_CopyToStreaming.content.text);
                    else
                        m_OnToggles.Remove(m_CopyToStreaming.content.text);
                    m_CopyToStreaming.state = newState;
                }
            }

            // advanced options
            using (new EditorGUI.DisabledScope (!AssetBundleModel.Model.DataSource.CanSpecifyBuildOptions)) {
                EditorGUILayout.Space();
                m_AdvancedSettings = EditorGUILayout.Foldout(m_AdvancedSettings, "Advanced Settings");
                if(m_AdvancedSettings)
                {
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    CompressOptions cmp = (CompressOptions)EditorGUILayout.IntPopup(
                        m_CompressionContent,

                        (int)m_Compression,
                        m_CompressionOptions,
                        m_CompressionValues);

                    if (cmp != m_Compression)
                    {
                        m_Compression = cmp;
                    }
                    foreach (var tog in m_ToggleData)
                    {
                        newState = EditorGUILayout.ToggleLeft(
                            tog.content,
                            tog.state);
                        if (newState != tog.state)
                        {

                            if (newState)
                                m_OnToggles.Add(tog.content.text);
                            else
                                m_OnToggles.Remove(tog.content.text);
                            tog.state = newState;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel = indent;
                }
            }

            // build.
            EditorGUILayout.Space();
            if (GUILayout.Button("Build") )
            {
                EditorApplication.delayCall += ExecuteBuild;
            }
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void ExecuteBuild()
        {
            if (AssetBundleModel.Model.DataSource.CanSpecifyBuildOutputDirectory) {
                if (string.IsNullOrEmpty(m_OutputPath))
                    BrowseForFolder();

                if (string.IsNullOrEmpty(m_OutputPath)) //in case they hit "cancel" on the open browser
                {
                    Debug.LogError("AssetBundle Build: No valid output path for build.");
                    return;
                }

                //保留原有的checkfile
                if (File.Exists(Path.Combine(m_OutputPath, "CheckFile")))
                    oldCheckFileStr = File.ReadAllText(Path.Combine(m_OutputPath, "CheckFile"));

                if (m_ForceRebuild.state)
                {
                    string message = "Do you want to delete all files in the directory " + m_OutputPath;
                    if (m_CopyToStreaming.state)
                        message += " and " + m_streamingPath;
                    message += "?";
                    if (EditorUtility.DisplayDialog("File delete confirmation", message, "Yes", "No"))
                    {
                        try
                        {
                            if (Directory.Exists(m_OutputPath))
                                Framework.Tool.DeleteDirContent(m_OutputPath);

                            if (m_CopyToStreaming.state && Directory.Exists(m_streamingPath))
                                Framework.Tool.DeleteDirContent(m_streamingPath);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
                if (!Directory.Exists(m_OutputPath))
                    Directory.CreateDirectory(m_OutputPath);
            }

            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;

            if (AssetBundleModel.Model.DataSource.CanSpecifyBuildOptions) {
                if (m_Compression == CompressOptions.Uncompressed)
                    opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
                else if (m_Compression == CompressOptions.ChunkBasedCompression)
                    opt |= BuildAssetBundleOptions.ChunkBasedCompression;
                foreach (var tog in m_ToggleData)
                {
                    if (tog.state)
                        opt |= tog.option;
                }
            }

            ABBuildInfo buildInfo = new ABBuildInfo();

            buildInfo.outputDirectory = m_OutputPath;
            buildInfo.options = opt;
            buildInfo.buildTarget = (BuildTarget)m_BuildTarget;

            AssetBundleModel.Model.DataSource.BuildAssetBundles (buildInfo);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            //复制pb文件
            if (Directory.Exists(m_protoBinPath))
                Framework.Tool.DirectoryCopy(m_protoBinPath, m_OutputPath, new Regex(@".*\.pb$"));

            generateCheckFile();

            if(m_CopyToStreaming.state)
                Framework.Tool.DirectoryCopy(m_OutputPath, m_streamingPath, new Regex(@".*\.manifest"), true);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        private void BrowseForFolder()
        {
            m_UseDefaultPath = false;
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", m_OutputPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length+1);
                m_OutputPath = newPath;
                EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
            }
        }
        private void ResetPathToDefault()
        {
            m_UseDefaultPath = true;
            m_OutputPath = "AssetBundles/";
            m_OutputPath += m_BuildTarget.ToString();
            EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
        }

        //Note: this is the provided BuildTarget enum with some entries removed as they are invalid in the dropdown
        public enum ValidBuildTarget
        {
            //NoTarget = -2,        --doesn't make sense
            //iPhone = -1,          --deprecated
            //BB10 = -1,            --deprecated
            //MetroPlayer = -1,     --deprecated
            StandaloneOSXUniversal = 2,
            StandaloneOSXIntel = 4,
            StandaloneWindows = 5,
            WebPlayer = 6,
            WebPlayerStreamed = 7,
            iOS = 9,
            PS3 = 10,
            XBOX360 = 11,
            Android = 13,
            StandaloneLinux = 17,
            StandaloneWindows64 = 19,
            WebGL = 20,
            WSAPlayer = 21,
            StandaloneLinux64 = 24,
            StandaloneLinuxUniversal = 25,
            WP8Player = 26,
            StandaloneOSXIntel64 = 27,
            BlackBerry = 28,
            Tizen = 29,
            PSP2 = 30,
            PS4 = 31,
            PSM = 32,
            XboxOne = 33,
            SamsungTV = 34,
            N3DS = 35,
            WiiU = 36,
            tvOS = 37,
            Switch = 38
        }

        private void generateCheckFile()
        {
            //生成用于增量更新的json文件
            var dir = new DirectoryInfo(m_OutputPath);
            var files = dir.GetFiles();
            List<Framework.AssetBundle.CheckFile> ret = new List<Framework.AssetBundle.CheckFile>();
            foreach(var v in files)
                if(!v.Name.EndsWith("manifest"))
                    ret.Add(new Framework.AssetBundle.CheckFile() { name = v.Name, hash = computeMD5(v.FullName) });
            var json = LitJson.JsonMapper.ToJson(ret);

            if (!string.IsNullOrEmpty(oldCheckFileStr))
            {
                var oldCheckFile = LitJson.JsonMapper.ToObject<List<Framework.AssetBundle.CheckFile>>(oldCheckFileStr);
                foreach(var v in ret)
                {
                    if (!oldCheckFile.Exists(t => t.hash == v.hash))
                        Debug.LogFormat("New bundle {0}", v.name);
                }
            }

            using (FileStream fs = new FileStream(Path.Combine(m_OutputPath, "CheckFile"), FileMode.Create, FileAccess.Write))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        private string computeMD5(string fileName)
        {
            string hashMD5 = string.Empty;
            if (File.Exists(fileName))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    System.Security.Cryptography.MD5 calculator = System.Security.Cryptography.MD5.Create();
                    byte[] buffer = calculator.ComputeHash(fs);
                    calculator.Clear();

                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        stringBuilder.Append(buffer[i].ToString("x2"));
                    }
                    hashMD5 = stringBuilder.ToString();
                }
            }
            return hashMD5;
        }
    }
}