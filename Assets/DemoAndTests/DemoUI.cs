using UnityEngine;
using System.Collections.Generic;
using System;
using Prime31;


public class DemoUI : MonoBehaviour
{
	SpriteAnimator _spriteAnimator;


	void Awake()
	{
		_spriteAnimator = FindObjectOfType<SpriteAnimator>();
		_spriteAnimator.onAnimationCompletedEvent += onAnimationEvent;
	}


	void OnGUI()
	{
		GUILayout.Label( "SerializationUtil" );
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


		GUILayout.Label( "Utils" );
		if( GUILayout.Button( "map01 Example" ) )
		{
			// maps a number from a range of 5 - 250 to a range of 0 - 1
			var num = UnityEngine.Random.Range( 5f, 250f );
			Debug.LogFormat( "mapped {0} in range 5 - 250 to range 0 - 1: {1}", num, MathHelpers.map01( num, 5f, 250f ) );
		}


		GUILayout.Label( "SpriteAnimator" );
		if( GUILayout.Button( "Sprite: Play walk-left Animation (flipping each iteration)" ) )
		{
			var animationIndex = _spriteAnimator.animationIndexForAnimationName( "walk-left" );
			_spriteAnimator.play( animationIndex );
		}

		if( GUILayout.Button( "Sprite: Play blue-dude-walk-right Animation" ) )
		{
			//blue-dude-walk-right
			var animationIndex = _spriteAnimator.animationIndexForAnimationName( "blue-dude-walk-right" );
			_spriteAnimator.play( animationIndex );
		}

		if( GUILayout.Button( "Sprite: Play blood-splatter Animation" ) )
		{
			//blood-splatter
			var animationIndex = _spriteAnimator.animationIndexForAnimationName( "blood-splatter" );
			_spriteAnimator.play( animationIndex );
		}
	}


	void onAnimationEvent( int animationIndex )
	{
		Debug.Log( "an animation event fired for animation with animationIndex " + animationIndex );
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