using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// Create a new type of Settings Asset.
class MyCustomSettings : ScriptableObject
{
    public const string k_MyCustomSettingsPath = "Assets/Editor/MyCustomSettings.asset";

    [SerializeField]
    private int m_Number;

    [SerializeField]
    private int m_minPlayerNumber;

    [SerializeField]
    private string m_SomeString;

    internal static MyCustomSettings GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<MyCustomSettings>(k_MyCustomSettingsPath);
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

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
}

// Register a SettingsProvider using UIElements for the drawing framework:
static class MyCustomSettingsUIElementsRegister
{
    [SettingsProvider]
    public static SettingsProvider CreateMyCustomSettingsProvider()
    {
        // First parameter is the path in the Settings window.
        // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
        var provider = new SettingsProvider("Project/MyCustomGameSettings", SettingsScope.Project)
        {
            label = "Custom UI Elements",
            // activateHandler is called when the user clicks on the Settings item in the Settings window.
            activateHandler = (searchContext, rootElement) =>
            {
                var settings = MyCustomSettings.GetSerializedSettings();

                // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                // isn't called because the SettingsProvider uses the UIElements drawing framework.
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/settings_ui.uxml");
                rootElement.styleSheets.Add(styleSheet);
                var title = new Label()
                {
                    text = "Custom Game Elements"
                };
                title.AddToClassList("title");
                rootElement.Add(title);

                var properties = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column
                    }
                };
                properties.AddToClassList("property-list");
                rootElement.Add(properties);

                properties.Add(new PropertyField(settings.FindProperty("m_SomeString")));
                properties.Add(new PropertyField(settings.FindProperty("m_Number")));
                properties.Add(new PropertyField(settings.FindProperty("m_minPlayerNumber")));

                rootElement.Bind(settings);
            },

            // Populate the search keywords to enable smart search filtering and label highlighting:
            keywords = new HashSet<string>(new[] { "Number", "Some String" })
        };

        return provider;
    }
}
