#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class AudioDataCreator
{
    // 1. The actual function that runs when you click the button
    [MenuItem("Assets/Create/Swingscape/Auto-Create Audio Data", false, 10)]
    public static void CreateAudioDataFromClip()
    {
        // Grab the currently selected object
        AudioClip selectedClip = Selection.activeObject as AudioClip;
        if (selectedClip == null) return;

        // Create a blank instance of your ScriptableObject in memory
        AudioDataSO newAudioData = ScriptableObject.CreateInstance<AudioDataSO>();
        
        // Auto-fill the essential data!
        newAudioData.clip = selectedClip;
        newAudioData.audioName = selectedClip.name; // Automatically uses the file name for your dictionary key
        
        // Figure out exactly where the audio clip lives in your project folders
        string clipPath = AssetDatabase.GetAssetPath(selectedClip);
        string folderPath = clipPath.Substring(0, clipPath.LastIndexOf('/'));
        
        // Generate a safe file path so it doesn't overwrite anything
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + selectedClip.name + "_SO.asset");
        
        // Physically save the new ScriptableObject to your hard drive
        AssetDatabase.CreateAsset(newAudioData, assetPath);
        AssetDatabase.SaveAssets();
        
        // Flash the new file in the editor so you can immediately tweak volume or pitch
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newAudioData;
    }

    // 2. The Validation Function
    [MenuItem("Assets/Create/Swingscape/Auto-Create Audio Data", true)]
    public static bool ValidateCreateAudioData()
    {
        // This ensures the button is only clickable if the user is actually clicking on an AudioClip
        return Selection.activeObject is AudioClip;
    }
}
#endif