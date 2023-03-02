using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


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
            // Project Settings里面看到的页签名
            label = "Custom Game Elements",
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

                // 数据绑定，创建属性控件
                properties.Add(new PropertyField(settings.FindProperty("m_SomeString")));
                properties.Add(new PropertyField(settings.FindProperty("m_Number")));
                properties.Add(new PropertyField(settings.FindProperty("m_MinPlayerNumber")));

                rootElement.Bind(settings);
            },

            // keywords暂时不知道有什么用，看注释是编辑器搜索用的
            // Populate the search keywords to enable smart search filtering and label highlighting:
            keywords = new HashSet<string>(new[] { "Number", "Some String", "MinPlayerNumber" })
        };

        return provider;
    }
}
