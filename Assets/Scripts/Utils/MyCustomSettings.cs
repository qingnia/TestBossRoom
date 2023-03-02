using UnityEditor;
using UnityEngine;

public class MyCustomSettings : ScriptableObject
{
    // 配置文件路径
    public const string k_MyCustomSettingsPath = "Assets/Editor/MyCustomSettings.asset";

    [SerializeField]
    private int m_Number;

    [SerializeField]
    private int m_MinPlayerNumber;

    [SerializeField]
    private string m_SomeString;

    internal static MyCustomSettings GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<MyCustomSettings>(k_MyCustomSettingsPath);
        // 如果找不到配置文件，就生成一个新的
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<MyCustomSettings>();
            settings.m_Number = 42;
            settings.m_SomeString = "The answer to the universe";
            AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }

    public static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
}

