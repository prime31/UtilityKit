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

		// state for the preview GUI
		Texture _playButton;
		Texture _pauseButton;
		bool _isPlaying;
		SpriteAnimator.Animation _currentAnimation;
		float _startTime;
		float _lastUpdateTime;
		bool _didWireEditorUpdate;


		public override void OnInspectorGUI()
		{
			if( Application.isPlaying )
			{
				onInspectorGUIPlayMode();
				return;
			}

			DrawDefaultInspector();

			validateData();

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			_selectedAnimation = EditorGUILayout.Popup( "Play on Start", _selectedAnimation, _animationNamesForInspector );
			if( EditorGUI.EndChangeCheck() )
			{
				if( _selectedAnimation == 0 )
				{
					_spriteAnimator.playAnimationOnStart = string.Empty;
				}
				else
				{
					_spriteAnimator.playAnimationOnStart = _animationNames[_selectedAnimation - 1];

					var animation = _spriteAnimator.animations.Where( a => a.name == _spriteAnimator.playAnimationOnStart ).First();
					if( animation.frames.Length > 0 )
						_spriteRenderer.sprite = animation.frames[0];
				}
			}
		}


		void onInspectorGUIPlayMode()
		{
			_isPlaying = false;

			EditorGUILayout.Space();
			GUILayout.Label( "Playback Controls", EditorStyles.boldLabel );

			if( GUILayout.Button( "Reverse Animation" ) )
				_spriteAnimator.reverseAnimation();

			if( GUILayout.Button( "Pause" ) )
				_spriteAnimator.pause();

			if( GUILayout.Button( "Unpause" ) )
				_spriteAnimator.unPause();

			if( GUILayout.Button( "Stop" ) )
				_spriteAnimator.stop();

			EditorGUILayout.Space();
			GUILayout.Label( "Animations", EditorStyles.boldLabel );

			for( var i = 0; i < _spriteAnimator.animations.Length; i++ )
			{
				if( GUILayout.Button( "Play " + _spriteAnimator.animations[i].name ) )
					_spriteAnimator.play( _spriteAnimator.animations[i].name );
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

				// set the animation for the preview
				_currentAnimation = _spriteAnimator.animations.Where( a => a.name == _animationNames[_selectedAnimationForPreview] ).First();
			}
		}


		void OnEnable()
		{
			validateData();

			_playButton = EditorGUIUtility.Load( "icons/Animation.Play.png" ) as Texture;
			_pauseButton = EditorGUIUtility.Load( "icons/PauseButton Anim.png" ) as Texture;
			_isPlaying = false;
		}


		void OnDisable()
		{
			if( _didWireEditorUpdate )
			{
				_didWireEditorUpdate = false;
				EditorApplication.update -= editorUpdate;
			}
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

				if( _spriteRenderer.sprite == null )
				{
					EditorGUI.DropShadowLabel( rect, "Sprite is null" );
					return;
				}



				Sprite sprite = null;

				// handle the play/pause of the animation from the preview GUI
				if( _currentAnimation != null )
				{
					var desiredFrame = 0;
					if( _isPlaying )
					{
						_lastUpdateTime = Time.realtimeSinceStartup;
						var elapsedTime = Time.realtimeSinceStartup - _startTime;
						desiredFrame = Mathf.FloorToInt( elapsedTime / ( 1f / _currentAnimation.fps ) );

						if( desiredFrame >= _currentAnimation.frames.Length )
						{
							desiredFrame = 0;
							_startTime = Time.realtimeSinceStartup;
						}
					}
						
					sprite = _currentAnimation.frames[desiredFrame];
				}
				else
				{
					sprite = _spriteRenderer.sprite;
				}


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
				EditorGUI.DropShadowLabel( rect, "WARNING: This is still a WIP!" );
			}
		}


		public override void OnPreviewSettings()
		{
			EditorGUI.BeginChangeCheck();
			_selectedAnimationForPreview = EditorGUILayout.Popup( _selectedAnimationForPreview, _animationNames );
			if( EditorGUI.EndChangeCheck() )
			{
				_currentAnimation = _spriteAnimator.animations.Where( a => a.name == _animationNames[_selectedAnimationForPreview] ).First();
			}


			if( GUILayout.Button( _isPlaying ? _pauseButton : _playButton, EditorStyles.miniButton ) )
			{
				_startTime = Time.realtimeSinceStartup;
				_isPlaying = !_isPlaying;

				if( _isPlaying )
				{
					_didWireEditorUpdate = true;
					EditorApplication.update += editorUpdate;
				}
				else if( _didWireEditorUpdate )
				{
					_didWireEditorUpdate = false;
					EditorApplication.update -= editorUpdate;
				}
			}
		}


		/// <summary>
		/// this is used to force repaints when we need a new frame in the preview GUI
		/// </summary>
		void editorUpdate()
		{
			if( _currentAnimation != null && _isPlaying )
			{
				var secondsPerFrame = 1f / _currentAnimation.fps;
				if( _lastUpdateTime + secondsPerFrame < Time.realtimeSinceStartup )
				{
					Repaint();
				}
			}
		}

	}
}