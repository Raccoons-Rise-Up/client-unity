using UnityEngine;
using KRU.Game;

/*!
 * AssetModificationProcessor Events
 * 
 * CanOpenForEdit	   - This is called by Unity when inspecting assets to determine if they can potentially be opened for editing.
 * FileModeChanged     - Unity calls this method when file mode has been changed for one or more files.
 * IsOpenForEdit       - This is called by Unity when inspecting assets to determine if an editor should be disabled.
 * MakeEditable	       - Unity calls this method when one or more files need to be opened for editing.
 * OnWillCreateAsset   - Unity calls this method when it is about to create an Asset you haven't imported (for example, .meta files).
 * OnWillDeleteAsset   - This is called by Unity when it is about to delete an asset from disk.
 * OnWillMoveAsset     - Unity calls this method when it is about to move an Asset on disk.
 * OnWillSaveAssets    - This is called by Unity when it is about to write serialized assets or Scene files to disk.
 * 
 * Code snippets with examples of these methods can be found here https://docs.unity3d.com/ScriptReference/AssetModificationProcessor.html
 */
public class UnityEvents : UnityEditor.AssetModificationProcessor
{
	static string[] OnWillSaveAssets(string[] paths)
	{
		UnityEventsHandler.HandlePlanetSaving();
		return paths;
	}
}

public class UnityEventsHandler
{
	public static void HandlePlanetSaving(){
		var planets = Object.FindObjectsOfType<PlanetRenderer>();

		foreach (var planet in planets)
		{
			var curLocalPos = planet.transform.localPosition;
			planet.Destroy(); // Destroy all planets before save
			UnityEditor.EditorApplication.delayCall += planet.GenerateTerrain; // Regenerate all planets after save
			UnityEditor.EditorApplication.delayCall += () => { planet.transform.localPosition = curLocalPos; };
		}
	}
}
