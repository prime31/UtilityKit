using UnityEngine;
using UnityEditor;
using System.Linq;
using Prime31;
using System.Collections.Generic;


namespace Prime31
{
	[CustomEditor( typeof( SpriteAnimator ) )]
	public class SpriteAnimatorEditor : Editor
	{
		SpriteAnimator _spriteAnimator;
		SpriteRenderer _spriteRenderer;

		// animation selector
		int _selectedAnimationForPreview = 0;
		int _selectedAnimation = 0;
		string[] _animationNames;
		string[] _animationNamesForInspector;


		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			validateData();

			EditorGUI.BeginChangeCheck();
			_selectedAnimation = EditorGUILayout.Popup( "Play Animation on Start", _selectedAnimation, _animationNamesForInspector );
			if( EditorGUI.EndChangeCheck() )
			{
				if( _selectedAnimation == 0 )
					_spriteAnimator.playAnimationOnStart = string.Empty;
				else
					_spriteAnimator.playAnimationOnStart = _animationNames[_selectedAnimation - 1];
			}
		}


		void validateData()
		{
			if( _spriteAnimator == null )
			{
				_spriteAnimator = target as SpriteAnimator;
				_spriteRenderer = _spriteAnimator.GetComponent<SpriteRenderer>();
				_animationNames = _spriteAnimator.animations.Select( a => a.name ).ToArray();

				// prep the Play Animation on Start list which needs a "none"
				var tempNames = new List<string>( _animationNames );
				tempNames.Insert( 0, "None" );
				_animationNamesForInspector = tempNames.ToArray();

				if( _spriteAnimator.playAnimationOnStart != string.Empty )
				{
					for( var i = 0; i < _spriteAnimator.animations.Length; i++ )
					{
						if( _spriteAnimator.animations[i].name == _spriteAnimator.playAnimationOnStart )
						{
							_selectedAnimation = i + 1; // we add one for the None element
							break;
						}
					}
				}
			}
		}


		void OnEnable()
		{
			validateData();
		}


		public override bool HasPreviewGUI()
		{
			return _spriteAnimator.animations.Length > 0;
		}


		public override void OnPreviewGUI( Rect rect, GUIStyle background )
		{
			if( Event.current.type == EventType.Repaint )
			{
				if( _spriteRenderer == null )
				{
					EditorGUI.DropShadowLabel( rect, "Sprite Renderer Required" );
					return;
				}
					
				var sprite = _spriteRenderer.sprite;
				var texture = sprite.texture;
				var textureRect = sprite.textureRect;
				var textureCoords = new Rect( textureRect.x / texture.width, textureRect.y / texture.height, textureRect.width / texture.width, textureRect.height / texture.height );
				var textureAspect = textureRect.width / textureRect.height;
				var guiAspect = rect.width / rect.height;
				var textureDisplayRect = rect;

				// we either need to constrain our width or height based on the preview aspect and sprite aspect
				if( textureAspect < guiAspect )
				{
					var widthOffset = 1 / guiAspect;
					textureDisplayRect.width *= widthOffset;
					textureDisplayRect.x = ( rect.width - textureDisplayRect.width ) * 0.5f;
				}
				else
				{
					textureDisplayRect.height *= guiAspect;
				}

				GUI.DrawTextureWithTexCoords( textureDisplayRect, texture, textureCoords );
			}
		}


		public override void OnPreviewSettings()
		{
			EditorGUI.BeginChangeCheck();
			_selectedAnimationForPreview = EditorGUILayout.Popup( _selectedAnimationForPreview, _animationNames );
			if( EditorGUI.EndChangeCheck() )
			{
				Debug.Log( _animationNames[_selectedAnimationForPreview] + ", " + Time.realtimeSinceStartup );
			}
		}

	}
}