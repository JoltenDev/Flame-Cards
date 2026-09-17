#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ObjectUtilities
{
    public static void CloneAsset<T>(T source, string targetPath) where T : Object
    {
        string assetPath = AssetDatabase.GetAssetPath(source);
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(targetPath);

        AssetDatabase.CopyAsset(assetPath, uniquePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif