using UnityEngine;
using UnityEditor;
using System.Linq;
using Prime31;
using System.Collections.Generic;
using UnityEditorInternal;


namespace Prime31
{
	[CustomEditor( typeof( SpriteAnimator ) )]
	public class SpriteAnimatorEditor : Editor
	{
		SpriteAnimator _spriteAnimator;
		SpriteRenderer _spriteRenderer;

		// inspector GUI
		const int kOddPaddingInReorderableList = 30;
		UnityEditor.AnimatedValues.AnimBool _framesListAnimBool;
		UnityEditor.AnimatedValues.AnimBool _triggerListAnimBool;

		private GUIStyle _boxStyle;
		private GUIStyle boxStyle
		{
			get
			{
				if( _boxStyle == null )
				{
					_boxStyle = new GUIStyle( GUI.skin.box );

					var tex = new Texture2D( 1, 1 );
					tex.hideFlags = HideFlags.HideAndDontSave;
					tex.SetPixel( 0, 0, Color.white );
					tex.Apply();

					_boxStyle.normal.background = tex;

					tex = new Texture2D( 1, 1 );
					tex.hideFlags = HideFlags.HideAndDontSave;
					tex.SetPixel( 0, 0, Color.yellow );
					tex.Apply();

					_boxStyle.hover.background = tex;

					var p = _boxStyle.padding;
					p.left = p.right = 20;
					p.top = 20;
					_boxStyle.padding = p;

					_boxStyle.fontSize = 15;
				}

				return _boxStyle;
			}
		}

		private GUIStyle _triggerStyleOdd;
		private GUIStyle triggerStyleOdd
		{
			get
			{
				if( _triggerStyleOdd == null )
				{
					_triggerStyleOdd = new GUIStyle( GUI.skin.box );

					var tex = new Texture2D( 1, 1 );
					tex.hideFlags = HideFlags.HideAndDontSave;
					tex.SetPixel( 0, 0, new Color( 0.3f, 0.3f, 0.3f, 1f ) );
					tex.Apply();

					_triggerStyleOdd.normal.background = tex;
				}

				return _triggerStyleOdd;
			}
		}
			
		private GUIStyle _triggerStyleEven;
		private GUIStyle triggerStyleEven
		{
			get
			{
				if( _triggerStyleEven == null )
				{
					_triggerStyleEven = new GUIStyle( GUI.skin.box );

					var tex = new Texture2D( 1, 1 );
					tex.hideFlags = HideFlags.HideAndDontSave;
					tex.SetPixel( 0, 0, new Color( 0.2f, 0.2f, 0.2f, 1f ) );
					tex.Apply();

					_triggerStyleEven.normal.background = tex;
				}

				return _triggerStyleEven;
			}
		}

		ReorderableList _reorderableAnimationList;
		ReorderableList _reorderableFrameList;
		int _selectedIndex = -1;

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

			validateData();

			EditorGUILayout.Space();
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


			EditorGUILayout.Space();

			// reorderable list for our animation list
			serializedObject.Update();
			_reorderableAnimationList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();

			// if we have a selection, draw the animation
			if( _selectedIndex >= 0 )
				drawSelectedAnimation();
		}


		void onInspectorGUIPlayMode()
		{
			_isPlaying = false;

			EditorGUILayout.Space();
			GUILayout.Label( "Playback Controls", EditorStyles.boldLabel );

			if( GUILayout.Button( "Flip Sprite" ) )
				_spriteAnimator.flip();
			
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
				if( GUILayout.Button( _spriteAnimator.animations[i].name ) )
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
							_selectedAnimation = i;
							break;
						}
					}
				}

				// set the animation for the preview
				_currentAnimation = _spriteAnimator.animations.Where( a => a.name == _animationNames[_selectedAnimation] ).First();
			}
		}


		void drawAnimationListElement( Rect rect, int index, bool isActive, bool isFocused )
		{
			rect.y += 1;
			var kButtonWidth = 60;

			var prop = _reorderableAnimationList.serializedProperty.GetArrayElementAtIndex( index );
			EditorGUI.PropertyField
			(  
				new Rect( rect.x, rect.y, rect.width - kButtonWidth - 40, EditorGUIUtility.singleLineHeight ),
				prop.FindPropertyRelative( "name" ), GUIContent.none
			);
				
			if( GUI.Button( new Rect( rect.width - kButtonWidth + kOddPaddingInReorderableList, rect.y, kButtonWidth, EditorGUIUtility.singleLineHeight ), "Edit" ) )
			{
				selectAnimationAtIndex( index );
			}
		}


		void OnEnable()
		{
			validateData();

			// setup the reorderable list
			deselectAnimation();
			_reorderableAnimationList = new ReorderableList( serializedObject, serializedObject.FindProperty( "animations" ), true, true, true, true );
			_reorderableAnimationList.drawElementCallback = drawAnimationListElement;
			_reorderableAnimationList.drawHeaderCallback = ( Rect rect ) =>
			{
				EditorGUI.LabelField( rect, "Animations" );
			};
			_reorderableAnimationList.onAddCallback = ( ReorderableList list ) =>
			{
				list.serializedProperty.InsertArrayElementAtIndex( list.count );

				var prop = list.serializedProperty.GetArrayElementAtIndex( list.count - 1 );
				prop.FindPropertyRelative( "name" ).stringValue = "new-animation";
				prop.FindPropertyRelative( "frames" ).ClearArray();
				prop.FindPropertyRelative( "triggers" ).ClearArray();

				selectAnimationAtIndex( list.count - 1 );
			};
			_reorderableAnimationList.onMouseUpCallback = ( ReorderableList list ) =>
			{
				selectAnimationAtIndex( _selectedIndex );
			};
			_reorderableAnimationList.onRemoveCallback = ( ReorderableList list ) =>
			{
				list.serializedProperty.DeleteArrayElementAtIndex( list.index );
				deselectAnimation();
			};

			// hide/show helper
			_triggerListAnimBool = new UnityEditor.AnimatedValues.AnimBool( false );
			_triggerListAnimBool.valueChanged.AddListener( Repaint );

			_framesListAnimBool = new UnityEditor.AnimatedValues.AnimBool( true );
			_framesListAnimBool.valueChanged.AddListener( Repaint );

			// setup our PreviewGUI goodies
			_playButton = EditorGUIUtility.Load( "icons/Animation.Play.png" ) as Texture;
			_pauseButton = EditorGUIUtility.Load( "icons/PauseButton Anim.png" ) as Texture;
			_isPlaying = false;
		}


		void OnDisable()
		{
			deselectAnimation();

			if( _didWireEditorUpdate )
			{
				_didWireEditorUpdate = false;
				EditorApplication.update -= editorUpdate;
			}
		}


		/// <summary>
		/// handles selection in the reorderable list and pressing the edit button. Responsible for setting up the reorderable list
		/// for frame and trigger management.
		/// </summary>
		/// <param name="selectedIndex">Selected index.</param>
		void selectAnimationAtIndex( int selectedIndex )
		{
			_selectedIndex = selectedIndex;
			_reorderableAnimationList.index = selectedIndex;

			// update the preview GUI
			_currentAnimation = _spriteAnimator.animations[selectedIndex];
			_selectedAnimationForPreview = selectedIndex;

			// prep the reorderable list
			var selectedAnimation = _reorderableAnimationList.serializedProperty.GetArrayElementAtIndex( selectedIndex );
			_reorderableFrameList = new ReorderableList( serializedObject, selectedAnimation.FindPropertyRelative( "frames" ) );
			_reorderableFrameList.elementHeight = 55;
			_reorderableFrameList.drawHeaderCallback = ( Rect rect ) =>
			{
				EditorGUI.LabelField( rect, "Animation Frames" );
			};
			_reorderableFrameList.onAddCallback = ( ReorderableList list ) =>
			{
				list.serializedProperty.InsertArrayElementAtIndex( list.count );

				var prop = list.serializedProperty.GetArrayElementAtIndex( list.count - 1 );
				prop.objectReferenceValue = null;
			};
			_reorderableFrameList.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
			{
				rect.y += 1;
				var kSpriteSize = 50;

				var prop = _reorderableFrameList.serializedProperty.GetArrayElementAtIndex( index );
				EditorGUI.PropertyField
				(  
					new Rect( rect.x, rect.y, rect.width - kSpriteSize - kOddPaddingInReorderableList, EditorGUIUtility.singleLineHeight ),
					prop, GUIContent.none
				);

				var sprite = prop.objectReferenceValue as Sprite;
				var spriteRect = new Rect( rect.width - kSpriteSize + kOddPaddingInReorderableList, rect.y, kSpriteSize, kSpriteSize );
				drawSpriteInRect( sprite, spriteRect, new Color( 0.8f, 0.8f, 0.8f, 0.5f ) );
			};
		}


		void deselectAnimation()
		{
			_selectedIndex = -1;
			if( _reorderableAnimationList != null )
				_reorderableAnimationList.index = -1;
			
			_reorderableFrameList = null;
		}


		void drawSelectedAnimation()
		{
			var selectedProp = _reorderableAnimationList.serializedProperty.GetArrayElementAtIndex( _selectedIndex );
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();

			if( GUILayout.Button( selectedProp.FindPropertyRelative( "name" ).stringValue, EditorStyles.toolbarButton ) )
				_framesListAnimBool.target = !_framesListAnimBool.target;

			if( EditorGUILayout.BeginFadeGroup( _framesListAnimBool.faded ) )
			{

				GUILayout.Space( 10 );
				GUI.backgroundColor = Color.white;

				EditorGUILayout.PropertyField( selectedProp.FindPropertyRelative( "fps" ), new GUIContent( "FPS" ) );
				EditorGUILayout.PropertyField( selectedProp.FindPropertyRelative( "completionBehavior" ), new GUIContent( "Completion Behavior" ) );
				EditorGUILayout.PropertyField( selectedProp.FindPropertyRelative( "loop" ), new GUIContent( "Loop" ) );
				EditorGUILayout.PropertyField( selectedProp.FindPropertyRelative( "pingPong" ), new GUIContent( "Ping-Pong" ) );
				EditorGUILayout.PropertyField( selectedProp.FindPropertyRelative( "delay" ), new GUIContent( "Delay" ) );

				EditorGUILayout.Space();

				_reorderableFrameList.DoLayoutList();
				dropAreaGUI();

				GUILayout.Space( 20 );
			}

			EditorGUILayout.Space();

			// triggers
			if( GUILayout.Button( "Animation Triggers", EditorStyles.toolbarButton ) )
				_triggerListAnimBool.target = !_triggerListAnimBool.target;

			if( EditorGUILayout.BeginFadeGroup( _triggerListAnimBool.faded ) )
			{
				var totalAnimationFrames = selectedProp.FindPropertyRelative( "frames" ).arraySize - 1;
				var triggersProp = selectedProp.FindPropertyRelative( "triggers" );
				for( var i = triggersProp.arraySize - 1; i >= 0; i-- )
				{
					EditorGUILayout.BeginVertical( i % 2 == 0 ? triggerStyleOdd : triggerStyleEven );

					var triggerEle = triggersProp.GetArrayElementAtIndex( i );
					EditorGUILayout.IntSlider( triggerEle.FindPropertyRelative( "frame" ), 0, totalAnimationFrames );
					EditorGUILayout.PropertyField( triggerEle.FindPropertyRelative( "onEnteredFrame" ) );

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if( GUILayout.Button( "Remove Trigger" ) )
						triggersProp.DeleteArrayElementAtIndex( i );
					GUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();
				}

				EditorGUILayout.Space();

				if( GUILayout.Button( "Add New Animation Trigger" ) )
					triggersProp.InsertArrayElementAtIndex( triggersProp.arraySize );

				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();


			serializedObject.ApplyModifiedProperties();

			GUILayout.Space( 10 );
			if( GUILayout.Button( "Done Editing" ) )
				_selectedIndex = -1;

			EditorGUILayout.EndVertical();
		}


		void dropAreaGUI()
		{
			var evt = Event.current;
			var dropArea = GUILayoutUtility.GetRect( 0f, 60f, GUILayout.ExpandWidth( true ) );
			GUI.Box( dropArea, "Drop Sprites to add to animation", boxStyle );

			switch( evt.type )
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					{
						if( !dropArea.Contains( evt.mousePosition ) )
							break;

						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

						if( evt.type == EventType.DragPerform )
						{
							DragAndDrop.AcceptDrag();
							foreach( var draggedObject in DragAndDrop.objectReferences )
							{
								var sprite = draggedObject as Sprite;
								if( !sprite )
									continue;

								// add the sprite to our list
								_reorderableFrameList.serializedProperty.InsertArrayElementAtIndex( _reorderableFrameList.count );
								var prop = _reorderableFrameList.serializedProperty.GetArrayElementAtIndex( _reorderableFrameList.count - 1 );
								prop.objectReferenceValue = sprite;

								serializedObject.ApplyModifiedProperties();
							}
						}

						Event.current.Use();
						break;
					} // end DragPerform
			} // end switch
		}


		void drawSpriteInRect( Sprite sprite, Rect rect, Color backgroundColor = default( Color ) )
		{
			if( sprite == null )
			{
				EditorGUI.DrawRect( rect, backgroundColor );
				return;
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

			EditorGUI.DrawRect( textureDisplayRect, backgroundColor );
			GUI.DrawTextureWithTexCoords( textureDisplayRect, texture, textureCoords );
		}


		#region Preview GUI

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


				drawSpriteInRect( sprite, rect );
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

		#endregion
	
	}
}