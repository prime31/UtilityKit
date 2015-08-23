/* Copyright (c) 2015 Benjamin Bennett 
 * Easy Sprite Maker 1.0
 * 
 * This is an editor script that should live in your Editor folder.
 */

using UnityEngine;
using UnityEditor;
using System.IO;

public class EasySpriteMaker: EditorWindow 
{  
	//private variables used for this script
	int size = 256;
	int outlineWidth = 1;
	bool outline = false;
	bool overrideColor = false;
	//bool bugTest = false;
	string objName = "";
	string path = "/";
	Color objColor = Color.white;
	Color prevColor = Color.white;
	Color newColor = Color.white;
	//Color debugColor = Color.black;
    GameObject gameObject;
	GameObject prevObject;
	Editor gameObjectEditor;
	SpriteType spriteType = SpriteType.Sprite;
	OutlineColor outlineColor = OutlineColor.Black;

	//static variables used for the editor window
	static GUISkin skin;
	static Texture2D backgroundColor;

	//Which format type to use
	enum SpriteType
	{
		Sprite,
		Legacy
	};

	enum OutlineColor
	{
		Black,
		Blue,
		Red,
		Green,
		Yellow,
		Cyan,
		Magenta,
		White
	};

	/// <summary>
	/// Called when the window first gets created
	/// </summary>
    [MenuItem("Window/Easy Icon Maker")]
    static void ShowWindow() 
	{
        EasySpriteMaker window = GetWindow<EasySpriteMaker>("Easy Icon Maker");
		skin = ScriptableObject.CreateInstance<GUISkin>();
		window.position = new Rect(150, 150, 400, 660);
		backgroundColor = new Texture2D(1, 1);
		backgroundColor.SetPixel(0, 0, Color.white);
		backgroundColor.Apply();
    }
    
    void OnGUI() 
	{
		/*GUI Elements*/
		skin.box.normal.background = backgroundColor;//modify the UI skin
		
		//GUI Fields
        gameObject = (GameObject) EditorGUILayout.ObjectField("Object", gameObject, typeof(GameObject), true);
		path = EditorGUILayout.TextField("Sprite Location", path);
		size = EditorGUILayout.IntField("Width", size);
		objColor = EditorGUILayout.ColorField("Background Color", objColor);
		spriteType = (SpriteType)EditorGUILayout.EnumPopup("Sprite Type", spriteType);
		outline = EditorGUILayout.Toggle("Outline", outline);
		overrideColor = EditorGUILayout.Toggle("Override Color", overrideColor);

		//GUI fields for outlining
		if(outline)
		{
			outlineWidth = EditorGUILayout.IntField("Outline Width", outlineWidth);
			outlineColor = (OutlineColor)EditorGUILayout.EnumPopup("Outline Color", outlineColor);
			EditorGUILayout.HelpBox("Currently there is a bug in the UnityEngine where custom colors result in unexpected errors in the Texture2D", MessageType.Warning);

			//DEBUG
			/*
			if(bugTest)
			{
				debugColor = EditorGUILayout.ColorField("Outline Color", debugColor);
			}
			else
			{
				outlineColor = (OutlineColor)EditorGUILayout.EnumPopup("Outline Color", outlineColor);
				EditorGUILayout.HelpBox("Currently there is a bug in the UnityEngine where custom colors result in unexpected errors in the Texture2D", MessageType.Warning);
			}
			 * */
		}

		//GUI field for overriding the color
		if(overrideColor)
		{
			newColor = EditorGUILayout.ColorField("Diffuse Color", newColor);
		}
			

		//If you change the color the background will show the change
		if(objColor != prevColor)
		{
			SetBackground();
		}

		//Create a new editor if the object changes
        if(gameObject != null) 
		{
			CheckEditor();//Check to see if a new editor needs to be created
			
			//GUI fields
			objName = EditorGUILayout.TextField("Name", objName);
            gameObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(Mathf.Min(size, 256), Mathf.Min(size, 256)), skin.box);//Render the window

			//Converts the background color to a clear color then converts it to a sprite
			if(GUILayout.Button("Render to Sprite"))
			{
				Texture2D renderImage = Render();

				if(outline)
					Outline(renderImage);

				//Save and write the file
				Save(renderImage);
			}

			//DEBUG BUTTON FOR UNITY BUG
			//bugTest = EditorGUILayout.Toggle("Test for Unity Bug", bugTest);
        }

		//Set the prev object
		if(prevObject != gameObject)
			prevObject = gameObject;
    }


	/// <summary>
	/// Creates a new editor preview for the game object if one doesn't exist for the current game object
	/// </summary>
	private void CheckEditor()
	{
		if (gameObjectEditor == null)
		{
			gameObjectEditor = Editor.CreateEditor(gameObject);
			objName = gameObject.name;
		}
		else if (prevObject != gameObject)
		{
			gameObjectEditor = Editor.CreateEditor(gameObject);
			objName = gameObject.name;
		}
	}

	/// <summary>
	/// Renders out the color or the override color to a new image
	/// </summary>
	/// <returns></returns>
	private Texture2D Render()
	{
		Texture2D image = gameObjectEditor.RenderStaticPreview(AssetDatabase.GetAssetPath(gameObject), null, size, size);
		Texture2D renderImage = new Texture2D(image.width, image.height);
		Color trans = image.GetPixel(0, 0);

		//Create transparent color
		for (int i = 0; i < image.width; i++)
		{
			for (int j = 0; j < image.height; j++)
			{
				Color c = image.GetPixel(i, j);

				if (c == trans)
				{
					renderImage.SetPixel(i, j, Color.clear);
				}
				else
				{
					if (overrideColor)
					{
						renderImage.SetPixel(i, j, newColor);
					}
					else
					{
						renderImage.SetPixel(i, j, c);
					}
				}
			}
		}

		renderImage.Apply();
		return renderImage;
	}

	//Writes the Texture2D to a png formatted file and saves to the asset database
	private void Save(Texture2D renderImage)
	{
		//Encode the image to a png and import to asset database
		var bytes = renderImage.EncodeToPNG();
		File.WriteAllBytes(Application.dataPath + path + objName + ".png", bytes);
		AssetDatabase.Refresh(ImportAssetOptions.Default);
		TextureImporter import = TextureImporter.GetAtPath("Assets" + path + objName + ".png") as TextureImporter;

		//Import the file depending on the kind of sprite you want
		if (spriteType == SpriteType.Sprite)
		{
			import.textureType = TextureImporterType.Sprite;
			import.spriteImportMode = SpriteImportMode.Single;
		}
		else if (spriteType == SpriteType.Legacy)
		{
			import.textureType = TextureImporterType.GUI;
		}

		//Import Assets to database
		AssetDatabase.ImportAsset("Assets" + path + objName + ".png", ImportAssetOptions.Default);
		AssetDatabase.Refresh(ImportAssetOptions.Default);
		AssetDatabase.SaveAssets();
	}

	/// <summary>
	/// Sets the background color of the preview window area
	/// </summary>
	private void SetBackground()
	{
		backgroundColor = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		backgroundColor.SetPixel(0, 0, objColor);
		backgroundColor.Apply();
		prevColor = objColor;
		skin.box.normal.background = backgroundColor;
	}

	//Converts the enum selection to a UnityEngine.Color
	private Color GetColor()
	{
		//if(bugTest)
			//return debugColor;

		if(outlineColor == OutlineColor.Black)
			return Color.black;

		if(outlineColor == OutlineColor.Blue)
			return Color.blue;

		if(outlineColor == OutlineColor.Cyan)
			return Color.cyan;

		if(outlineColor == OutlineColor.Green)
			return Color.green;

		if(outlineColor == OutlineColor.Magenta)
			return Color.magenta;

		if(outlineColor == OutlineColor.Red)
			return Color.red;

		if(outlineColor == OutlineColor.White)
			return Color.white;

		if(outlineColor == OutlineColor.Yellow)
			return Color.yellow;

		return Color.black;
	}

	/// <summary>
	/// Runs through the render image and creates an outline around the object
	/// </summary>
	/// <param name="renderImage"></param>
	private void Outline(Texture2D renderImage)
	{
		for (int i = 0; i < renderImage.width; i++)
		{
			for (int j = 0; j < renderImage.height; j++)
			{
				Color c = renderImage.GetPixel(i, j);

				if (c != Color.clear && c != GetColor())
				{
					for (int k = 1; k <= outlineWidth; k++)
					{
						//Outline north pixels
						if(j + k < renderImage.height && renderImage.GetPixel(i, j + k) == Color.clear)
							renderImage.SetPixel(i, j + k, GetColor());

						//Outline south pixels
						if (j - k >= 0 && renderImage.GetPixel(i, j - k) == Color.clear)
							renderImage.SetPixel(i, j - k, GetColor());

						//OUtline east pixels
						if (i + k < renderImage.width && renderImage.GetPixel(i + k, j) == Color.clear)
							renderImage.SetPixel(i + k, j, GetColor());

						//Outline west pixels
						if (i - k >= 0 && renderImage.GetPixel(i - k, j) == Color.clear)
							renderImage.SetPixel(i - k, j, GetColor());

						//Outline north-east pixels
						if(i + k < renderImage.width && j + k < renderImage.height && renderImage.GetPixel(i + k, j + k) == Color.clear)
							renderImage.SetPixel(i + k, j + k, GetColor());

						//Outline north-west pixels
						if(i - k >= 0 && j + k < renderImage.height && renderImage.GetPixel(i - k, j + k) == Color.clear)
							renderImage.SetPixel(i - k, j + k, GetColor());

						//Outline south-east pixels
						if(i + k < renderImage.width && j - k >= 0 && renderImage.GetPixel(i + k, j - k) == Color.clear)
							renderImage.SetPixel(i + k, j - k, GetColor());

						//Outline south-west pixels
						if(i - k >= 0 && j - k >= 0 && renderImage.GetPixel(i - k, j - k) == Color.clear)
							renderImage.SetPixel(i - k, j - k, GetColor());
					}
				}
			}
		}
	}
}