using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace EditorToolbar.Editor.EditorBar
{
	[InitializeOnLoad]
	public class SceneSwitchLeftButton
	{
		static SceneSwitchLeftButton()
		{
			ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
		}

		private const string UnknownString = "<-unknown->";
		static void OnToolbarGUI()
		{
			int count = SceneManager.sceneCountInBuildSettings;
			string[] scenePaths = new string[count];
			string[] names = new string[count+1];
			
			int currentSceneId = count;
			string currentSceneName = SceneManager.GetActiveScene().name;

			// Init scene paths and names
			for (var i = 0; i < count; i++)
			{
				scenePaths[i] = SceneUtility.GetScenePathByBuildIndex(i);
				names[i] = System.IO.Path.GetFileNameWithoutExtension(scenePaths[i]);
				
				if (names[i] == currentSceneName)
					currentSceneId = i;
			}

			names[count] = UnknownString;
			
			
			var sceneIndex = EditorGUILayout.Popup(
				currentSceneId, names,GUILayout.Width(100));
			
			if (sceneIndex != currentSceneId && sceneIndex != count)
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					EditorSceneManager.OpenScene(scenePaths[sceneIndex]);
				}
			}
			GUILayout.FlexibleSpace();
		}
	}
}