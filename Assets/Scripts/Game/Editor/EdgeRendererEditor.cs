using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using KRU.Game;

[CustomEditor(typeof(PlanetRenderer))]
public class EdgeRendererEditor : Editor
{
    private PlanetRenderer planetRenderer;

    private Editor terrainEditor;
    private Editor oceanEditor;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                planetRenderer.GenerateTerrain();
            }
        }
        CreateButton("Generate Terrain", planetRenderer.GenerateTerrain);

        DrawSettingsEditor(planetRenderer.terrainSettings, () => planetRenderer.GenerateTerrain(), ref planetRenderer.terrainSettingsFoldout, ref terrainEditor);
        DrawSettingsEditor(planetRenderer.oceanSettings, () => planetRenderer.GenerateOcean(), ref planetRenderer.oceanSettingsFoldout, ref oceanEditor);
    }

    private void CreateButton(string name, System.Action onButtonPressed)
    {
        GUILayout.Space(10);
        if (GUILayout.Button(name))
        {
            onButtonPressed();
        }
        GUILayout.Space(10);
    }

    private void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using var check = new EditorGUI.ChangeCheckScope();
            if (foldout)
            {
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();

                if (check.changed)
                    onSettingsUpdated?.Invoke();
            }
        }
    }

    private void OnEnable()
    {
        planetRenderer = (PlanetRenderer)target;
    }
}