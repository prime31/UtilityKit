using UnityEngine;
using System.Collections.Generic;
using System;
using Prime31;


public class DemoUI : MonoBehaviour
{
	void OnGUI()
	{
		if( GUILayout.Button( "Serialize Object to Desktop" ) )
		{
			var obj = new ThingToSerialize();
			Debug.Log( obj );
			SerializationUtil.serializeObjectToFile( obj, "Thingy.bin", Environment.GetFolderPath( Environment.SpecialFolder.Desktop ) );
		}
			
		if( GUILayout.Button( "Deserialize Object from Desktop" ) )
		{
			var obj = SerializationUtil.deserializeObjectFromFile<ThingToSerialize>( "Thingy.bin", Environment.GetFolderPath( Environment.SpecialFolder.Desktop ) );
			Debug.Log( obj );
		}

		if( GUILayout.Button( "map01 Example" ) )
		{
			// maps a number (75 in this case) from a range of 5 - 250 to a range of 0 - 1
			Debug.Log( "mapped: " + MathHelpers.map01( 75f, 5f, 250f ) );
		}

		if( GUILayout.Button( "--- RESERVED SPACE FOR OTHER STUFF" ) )
		{

		}

		if( GUILayout.Button( "" ) )
		{

		}

		if( GUILayout.Button( "" ) )
		{

		}

		if( GUILayout.Button( "" ) )
		{

		}

		if( GUILayout.Button( "" ) )
		{

		}
	}

}




[Serializable]
class ThingToSerialize
{
	public Vector2 vec2 = new Vector2( 2f, 4f );
	public Vector3 vec3 = new Vector3( 3f, 5f, 7f );
	public string str = "stringy time";


	public override string ToString()
	{
		return string.Format( "[ThingToSerialize] vec2: {0}, vec3: {1}, str: {2}", vec2, vec3, str );
	}
}