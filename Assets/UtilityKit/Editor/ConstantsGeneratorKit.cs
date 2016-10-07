//#define DISABLE_AUTO_GENERATION
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Reflection;


namespace Prime31Editor
{
	// Note: This class uses UnityEditorInternal which is an undocumented internal feature
	public class ConstantsGeneratorKit : MonoBehaviour
	{
		private const string FOLDER_LOCATION = "scripts/auto-generated/";
		private const string NAMESPACE = "k";
		private static ConstantNamingStyle CONSTANT_NAMING_STYLE = ConstantNamingStyle.UppercaseWithUnderscores;
		private const string DIGIT_PREFIX = "k";
		private static string[] IGNORE_RESOURCES_IN_SUBFOLDERS = new string[] { "ProCore", "2DToolkit" };
		private static bool SHOW_SUCCESS_MESSAGE = true;

		private const string TAGS_FILE_NAME = "Tags.cs";
		private const string LAYERS_FILE_NAME = "Layers.cs";
		private const string SORTING_LAYERS_FILE_NAME = "SortingLayers.cs";
		private const string SCENES_FILE_NAME = "Scenes.cs";
		private const string RESOURCE_PATHS_FILE_NAME = "Resources.cs";

		private static string TOTAL_SCENES_CONSTANT_NAME = CONSTANT_NAMING_STYLE == ConstantNamingStyle.UppercaseWithUnderscores ? "TOTAL_SCENES" : "TotalScenes";


		[MenuItem( "Edit/Generate Constants Classes..." )]
		static void rebuildConstantsClassesMenuItem()
		{
			rebuildConstantsClasses();
		}


		public static void rebuildConstantsClasses( bool buildResources = true, bool buildScenes = true, bool buildTagsAndLayers = true, bool buildSortingLayers = true )
		{
			var folderPath = Application.dataPath + "/" + FOLDER_LOCATION;
			if( !Directory.Exists( folderPath ) )
				Directory.CreateDirectory( folderPath );

			if( buildTagsAndLayers )
			{
				File.WriteAllText( folderPath + TAGS_FILE_NAME, getClassContent( TAGS_FILE_NAME.Replace( ".cs", string.Empty ), UnityEditorInternal.InternalEditorUtility.tags ) );
				File.WriteAllText( folderPath + LAYERS_FILE_NAME, getLayerClassContent( LAYERS_FILE_NAME.Replace( ".cs", string.Empty ), UnityEditorInternal.InternalEditorUtility.layers ) );

				AssetDatabase.ImportAsset( "Assets/" + FOLDER_LOCATION + TAGS_FILE_NAME, ImportAssetOptions.ForceUpdate );
				AssetDatabase.ImportAsset( "Assets/" + FOLDER_LOCATION + LAYERS_FILE_NAME, ImportAssetOptions.ForceUpdate );
			}

			if( buildSortingLayers )
			{
				var sortingLayers = getSortingLayers();
				var layerIds = getSortingLayerIds( sortingLayers.Length );
				File.WriteAllText( folderPath + SORTING_LAYERS_FILE_NAME, getSortingLayerClassContent( SORTING_LAYERS_FILE_NAME.Replace( ".cs", string.Empty ), sortingLayers, layerIds ) );
				AssetDatabase.ImportAsset( "Assets/" + FOLDER_LOCATION + SORTING_LAYERS_FILE_NAME, ImportAssetOptions.ForceUpdate );
			}

			// handle resources and scenes only when asked
			if( buildScenes )
			{
				File.WriteAllText( folderPath + SCENES_FILE_NAME, getClassContent( SCENES_FILE_NAME.Replace( ".cs", string.Empty ), editorBuildSettingsScenesToNameStrings( EditorBuildSettings.scenes ) ) );
				AssetDatabase.ImportAsset( "Assets/" + FOLDER_LOCATION + SCENES_FILE_NAME, ImportAssetOptions.ForceUpdate );
			}

			if( buildResources )
			{
				File.WriteAllText( folderPath + RESOURCE_PATHS_FILE_NAME, getResourcePathsContent( RESOURCE_PATHS_FILE_NAME.Replace( ".cs", string.Empty ) ) );
				AssetDatabase.ImportAsset( "Assets/" + FOLDER_LOCATION + RESOURCE_PATHS_FILE_NAME, ImportAssetOptions.ForceUpdate );
			}

			if( SHOW_SUCCESS_MESSAGE && buildResources && buildScenes && buildTagsAndLayers )
				Debug.Log( "ConstantsGeneratorKit complete. Constants classes built to " + FOLDER_LOCATION );
		}


		private static string[] editorBuildSettingsScenesToNameStrings( EditorBuildSettingsScene[] scenes )
		{
			var sceneNames = new string[scenes.Length];
			for( var n = 0; n < sceneNames.Length; n++ )
				sceneNames[n] = Path.GetFileNameWithoutExtension( scenes[n].path );

			return sceneNames;
		}


		private static string[] getSortingLayers()
		{
			var type = typeof( UnityEditorInternal.InternalEditorUtility );
			var prop = type.GetProperty( "sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic );

			return prop.GetValue( null, null ) as string[];
		}


		private static int[] getSortingLayerIds( int totalSortingLayers )
		{
			var type = typeof( UnityEditorInternal.InternalEditorUtility );

			// the behaviour is different here between Unity 4 and Unity 5.
			// Unity 4 uses "user layers", while Unity 5 uses only unique sorting layer IDs
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
			var layerIds = new int[totalSortingLayers];

			var method = type.GetMethod( "GetSortingLayerUserID", BindingFlags.Static | BindingFlags.NonPublic );
			for( var n = 0; n < totalSortingLayers; n++ )
				layerIds[n] = (int)method.Invoke( null, new object[] { n } );
#else
			var layerIds = type.GetProperty( "sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic ).GetValue( null, null ) as int[];
#endif
			return layerIds;
		}


		private static string getClassContent( string className, string[] labelsArray )
		{
			var output = "";
			output += "//This class is auto-generated do not modify\n";
			output += "namespace " + NAMESPACE + "\n";
			output += "{\n";
			output += "\tpublic static class " + className + "\n";
			output += "\t{\n";

			foreach( var label in labelsArray )
				output += "\t\t" + buildConstVariable( label ) + "\n";

			if( className == SCENES_FILE_NAME.Replace( ".cs", string.Empty ) )
			{
				output += "\n\t\tpublic const int " + TOTAL_SCENES_CONSTANT_NAME + " = " + labelsArray.Length + ";\n\n\n";

				output += "\t\tpublic static int nextSceneIndex()\n";
				output += "\t\t{\n";
				output += "\t\t\tvar currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;\n";
				output += "\t\t\tif( currentSceneIndex + 1 == " + TOTAL_SCENES_CONSTANT_NAME + " )\n";
				output += "\t\t\t\treturn 0;\n";
				output += "\t\t\treturn currentSceneIndex + 1;\n";
				output += "\t\t}\n";
			}

			output += "\t}\n";
			output += "}";

			return output;
		}


		private class Resource
		{
			public string name;
			public string path;


			public Resource( string path )
			{
				// get the path from the Resources folder root with normalized slashes
				string fullAssetsPath = Path.GetFullPath( "Assets" ).Replace( '\\', '/' );
				path = path.Replace( '\\', '/' );
				path = path.Replace( fullAssetsPath, "" );
				var parts = path.Split( new string[] { "Resources/" }, StringSplitOptions.RemoveEmptyEntries );

				// strip the extension from the path
				this.path = parts[1].Replace( Path.GetFileName( parts[1] ), Path.GetFileNameWithoutExtension( parts[1] ) );
				this.name = Path.GetFileNameWithoutExtension( parts[1] );
			}
		}


		private static string getResourcePathsContent( string className )
		{
			var output = "";
			output += "//This class is auto-generated do not modify\n";
			output += "namespace " + NAMESPACE + "\n";
			output += "{\n";
			output += "\tpublic static class " + className + "\n";
			output += "\t{\n";


			// find all our Resources folders
			var dirs = Directory.GetDirectories( Application.dataPath, "Resources", SearchOption.AllDirectories );
			var resources = new List<Resource>();

			foreach( var dir in dirs )
			{
				// limit our ignored folders
				var shouldAddFolder = true;
				foreach( var ignoredDir in IGNORE_RESOURCES_IN_SUBFOLDERS )
				{
					if( dir.Contains( ignoredDir ) )
					{
						Debug.LogWarning( "DONT ADD FOLDER + " + dir );
						shouldAddFolder = false;
						continue;
					}
				}

				if( shouldAddFolder )
					resources.AddRange( getAllResourcesAtPath( dir ) );
			}

			var resourceNamesAdded = new List<string>();
			var constantNamesAdded = new List<string>();
			foreach( var res in resources )
			{
				if( resourceNamesAdded.Contains( res.name ) )
				{
					Debug.LogWarning( "multiple resources with name " + res.name + " found. Skipping " + res.path );
					continue;
				}

				string constantName = formatConstVariableName( res.name );
				if( constantNamesAdded.Contains( constantName ) )
				{
					Debug.LogWarning( "multiple resources with constant name " + constantName + " found. Skipping " + res.path );
					continue;
				}


				output += "\t\t" + buildConstVariable( res.name, "", res.path ) + "\n";
				resourceNamesAdded.Add( res.name );
				constantNamesAdded.Add( constantName );
			}


			output += "\t}\n";
			output += "}";

			return output;
		}


		private static List<Resource> getAllResourcesAtPath( string path )
		{
			var resources = new List<Resource>();

			// handle files
			var files = Directory.GetFiles( path, "*", SearchOption.AllDirectories );
			foreach( var f in files )
			{
				if( f.EndsWith( ".meta" ) || f.EndsWith( ".db" ) || f.EndsWith( ".DS_Store" ) )
					continue;

				resources.Add( new Resource( f ) );
			}

			return resources;
		}


		private static string getLayerClassContent( string className, string[] labelsArray )
		{
			var output = "";
			output += "// This class is auto-generated do not modify\n";
			output += "namespace " + NAMESPACE + "\n";
			output += "{\n";
			output += "\tpublic static class " + className + "\n";
			output += "\t{\n";

			foreach( var label in labelsArray )
				output += "\t\t" + "public const int " + formatConstVariableName( label ) + " = " + LayerMask.NameToLayer( label ) + ";\n";

			output += "\n\n";
			output += @"		public static int onlyIncluding( params int[] layers )
		{
			int mask = 0;
			for( var i = 0; i < layers.Length; i++ )
				mask |= ( 1 << layers[i] );

			return mask;
		}


		public static int everythingBut( params int[] layers )
		{
			return ~onlyIncluding( layers );
		}";

			output += "\n";
			output += "\t}\n";
			output += "}";

			return output;
		}


		private static string getSortingLayerClassContent( string className, string[] sortingLayers, int[] layerIds )
		{
			var output = "";
			output += "// This class is auto-generated do not modify\n";
			output += "namespace " + NAMESPACE + "\n";
			output += "{\n";
			output += "\tpublic static class " + className + "\n";
			output += "\t{\n";

			for( var i = 0; i < sortingLayers.Length; i++ )
				output += "\t\t" + "public const int " + formatConstVariableName( sortingLayers[i] ) + " = " + layerIds[i] + ";\n";

			output += "\n";
			output += "\t}\n";
			output += "}";

			return output;
		}


		private static string buildConstVariable( string varName, string suffix = "", string value = null )
		{
			value = value ?? varName;
			return "public const string " + formatConstVariableName( varName ) + suffix + " = " + '"' + value + '"' + ";";
		}


		private static string formatConstVariableName( string input )
		{
			switch( CONSTANT_NAMING_STYLE )
			{
			case ConstantNamingStyle.UppercaseWithUnderscores:
				return toUpperCaseWithUnderscores( input );
			case ConstantNamingStyle.CamelCase:
				return toCamelCase( input );
			default:
				return toUpperCaseWithUnderscores( input );
			}
		}

		private static string toCamelCase( string input )
		{
			input = input.Replace( " ", "" );

			if( char.IsLower( input[0] ) )
				input = char.ToUpper( input[0] ) + input.Substring( 1 );

			// uppercase letters before dash or underline
			Func<char,int,string> func = ( x, i ) =>
			{
				if( x == '-' || x == '_' )
					return "";

				if( i > 0 && ( input[i - 1] == '-' || input[i - 1] == '_' ) )
					return x.ToString().ToUpper();

				return x.ToString();
			};
			input = string.Concat( input.Select( func ).ToArray() );

			// digits are a no-no so stick prefix in front
			if( char.IsDigit( input[0] ) )
				return DIGIT_PREFIX + input;
			return input;
		}

		private static string toUpperCaseWithUnderscores( string input )
		{
			input = input.Replace( "-", "_" );
			input = Regex.Replace( input, @"\s+", "_" );

			// make camel-case have an underscore between letters
			Func<char,int,string> func = ( x, i ) =>
			{
				if( i > 0 && char.IsUpper( x ) && char.IsLower( input[i - 1] ) )
					return "_" + x.ToString();
				return x.ToString();
			};
			input = string.Concat( input.Select( func ).ToArray() );

			// digits are a no-no so stick prefix in front
			if( char.IsDigit( input[0] ) )
				return DIGIT_PREFIX + input.ToUpper();
			return input.ToUpper();
		}

		private enum ConstantNamingStyle
		{
			UppercaseWithUnderscores,
			CamelCase
		}
	}


	#if !DISABLE_AUTO_GENERATION
	// this post processor listens for changes to the TagManager and automatically rebuilds all classes if it sees a change
	public class ConstandsGeneratorPostProcessor : AssetPostprocessor
	{
		// for some reason, OnPostprocessAllAssets often gets called multiple times in a row. This helps guard against rebuilding classes
		// when not necessary.
		static DateTime? _lastTagsAndLayersBuildTime;
		static DateTime? _lastScenesBuildTime;


		static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
		{
			var resourcesDidChange = importedAssets.Any( s => Regex.IsMatch( s, @"/Resources/.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase ) );

			if( !resourcesDidChange )
				resourcesDidChange = movedAssets.Any( s => Regex.IsMatch( s, @"/Resources/.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase ) );

			if( !resourcesDidChange )
				resourcesDidChange = deletedAssets.Any( s => Regex.IsMatch( s, @"/Resources/.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase ) );

			if( resourcesDidChange )
				ConstantsGeneratorKit.rebuildConstantsClasses( true, false, false );


			// layers and tags changes
			if( importedAssets.Contains( "ProjectSettings/TagManager.asset" ) )
			{
				if( !_lastTagsAndLayersBuildTime.HasValue || _lastTagsAndLayersBuildTime.Value.AddSeconds( 5 ) < DateTime.Now )
				{
					_lastTagsAndLayersBuildTime = DateTime.Now;
					ConstantsGeneratorKit.rebuildConstantsClasses( false, false );
				}
			}


			// scene changes
			if( importedAssets.Contains( "ProjectSettings/EditorBuildSettings.asset" ) )
			{
				if( !_lastScenesBuildTime.HasValue || _lastScenesBuildTime.Value.AddSeconds( 5 ) < DateTime.Now )
				{
					_lastScenesBuildTime = DateTime.Now;
					ConstantsGeneratorKit.rebuildConstantsClasses( false, true );
				}
			}
		}
	}
	#endif
}
#endif