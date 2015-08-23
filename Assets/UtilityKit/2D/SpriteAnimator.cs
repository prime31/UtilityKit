using UnityEngine;
using System.Collections;


// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### WARNING: THIS CLASS AND ITS EDITOR ARE NOT COMPLETE AND ARE JUST A WORK IN PROGRESS!
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

namespace Prime31
{
	// based on the SpriteAnimator by Alec Holowka
	[RequireComponent( typeof( SpriteRenderer ) )]
	public class SpriteAnimator : MonoBehaviour
	{
		[System.Serializable]
		public class AnimationTrigger
		{
			public int frame;
			public string name;
		}


		[System.Serializable]
		public class Animation
		{
			public string name;
			public int fps = 5;
			public Sprite[] frames;
			public AnimationTrigger[] triggers;
		}


		public Animation[] animations;
		public bool isPlaying { get; private set; }
		public Animation currentAnimation { get; private set; }
		public int currentFrame { get; private set; }
		public bool loop { get; private set; }
		[HideInInspector]
		[SerializeField]
		public string playAnimationOnStart;

		SpriteRenderer _spriteRenderer;
		float _timer;
		float _frameDuration;


		#region MonoBehavior

		void Awake()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
		}


		void OnEnable()
		{
			if( playAnimationOnStart != string.Empty )
				Play( playAnimationOnStart );
		}


		void OnDisable()
		{
			isPlaying = false;
			currentAnimation = null;
		}


		void Update()
		{
			if( currentAnimation == null )
				return;
			
			//while( loop || currentFrame < currentAnimation.frames.Length - 1 )
			var desiredFrame = Mathf.FloorToInt( _timer / _frameDuration );
			//Debug.Log( "timer: " + _timer + ", desiredFrame: " + desiredFrame + ", currentFrame: " + currentFrame );
			_timer += Time.deltaTime;

			if( currentFrame != desiredFrame )
			{
				nextFrame( currentAnimation );
				_spriteRenderer.sprite = currentAnimation.frames[currentFrame];
			}

			if( !loop && currentFrame == currentAnimation.frames.Length - 1 )
				currentAnimation = null;
		}

		#endregion


		public void Play( string name, bool loop = true, int startFrame = 0 )
		{
			var animation = getAnimation( name );
			if( animation != null )
			{
				if( animation != currentAnimation )
				{
					ForcePlay( name, loop, startFrame );
				}
			}
			else
			{
				Debug.LogWarning( "could not find animation: " + name );
			}
		}


		public void ForcePlay( string name, bool loop = true, int startFrame = 0 )
		{
			var animation = getAnimation( name );
			if( animation != null )
			{
				this.loop = loop;
				currentAnimation = animation;
				isPlaying = true;
				currentFrame = startFrame;
				_spriteRenderer.sprite = animation.frames[currentFrame];

				_timer = 0f;
				_frameDuration = 1f / (float)currentAnimation.fps;
			}
		}


		public bool isAnimationPlaying( string name )
		{
			return ( currentAnimation != null && currentAnimation.name == name );
		}


		public Animation getAnimation( string name )
		{
			for( var i = 0; i < animations.Length; i++ )
			{
				if( animations[i].name == name )
					return animations[i];
			}

			return null;
		}


		void nextFrame( Animation animation )
		{
			currentFrame++;
			foreach( var animationTrigger in currentAnimation.triggers )
			{
				if( animationTrigger.frame == currentFrame )
				{
					gameObject.SendMessageUpwards( animationTrigger.name );
				}
			}

			if( currentFrame >= animation.frames.Length )
			{
				if( loop )
				{
					currentFrame = 0;
					_timer = 0f;
				}
				else
				{
					currentFrame = animation.frames.Length - 1;
				}
			}
		}


		public int GetFacing()
		{
			return (int)Mathf.Sign( _spriteRenderer.transform.localScale.x );
		}


		public void flipTo( float dir )
		{
			if( dir < 0f )
				_spriteRenderer.transform.localScale = new Vector3( -1f, 1f, 1f );
			else
				_spriteRenderer.transform.localScale = new Vector3( 1f, 1f, 1f );
		}


		public void flipTo( Vector3 position )
		{
			float diff = position.x - transform.position.x;
			if( diff < 0f )
				_spriteRenderer.transform.localScale = new Vector3( -1f, 1f, 1f );
			else
				_spriteRenderer.transform.localScale = new Vector3( 1f, 1f, 1f );
		}
	
	}
}