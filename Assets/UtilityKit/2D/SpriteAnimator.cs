using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Events;


namespace Prime31
{
	[RequireComponent( typeof( SpriteRenderer ) )]
	public class SpriteAnimator : MonoBehaviour
	{
		[System.Serializable]
		public class SpriteAnimationEvent : UnityEvent<int> {}


		[System.Serializable]
		public class AnimationTrigger
		{
			public int frame;
			public SpriteAnimationEvent onEnteredFrame;
		}


		[System.Serializable]
		public class Animation
		{
			public enum AnimationCompletionBehavior
			{
				RemainOnFinalFrame,
				RevertToFirstFrame,
				HideSprite
			}

			public string name;
			public float fps = 5;
			public bool loop;
			public bool pingPong;
			public float delay = 0f;
			public AnimationCompletionBehavior completionBehavior;
			public Sprite[] frames;
			public AnimationTrigger[] triggers;

			[System.NonSerialized, HideInInspector]
			public float secondsPerFrame;
			[System.NonSerialized, HideInInspector]
			public float iterationDuration;
			[System.NonSerialized, HideInInspector]
			public float totalDuration;

			bool _hasBeenPreparedForUse;
			HashSet<int> _framesWithTriggers;


			public void prepareForUse()
			{
				if( _hasBeenPreparedForUse )
					return;
				
				secondsPerFrame = 1f / fps;
				iterationDuration = secondsPerFrame * (float)frames.Length;

				if( loop )
					totalDuration = Mathf.Infinity;
				else if( pingPong )
					totalDuration = iterationDuration * 2f;
				else
					totalDuration = iterationDuration;

				// prep our trigger lookup helper
				_framesWithTriggers = new HashSet<int>();
				for( var i = 0; i < triggers.Length; i++ )
				{
					if( triggers[i].onEnteredFrame.GetPersistentEventCount() > 0 )
						_framesWithTriggers.Add( triggers[i].frame );
				}

				_hasBeenPreparedForUse = true;
			}


			public bool frameContainsTrigger( int frame )
			{
				return _framesWithTriggers.Contains( frame );
			}
		}


		public Animation[] animations;
		public System.Action<int> onAnimationCompletedEvent;
		public bool isPlaying { get; private set; }
		public int currentFrame { get; private set; }
		[SerializeField, HideInInspector]
		public string playAnimationOnStart;

		// cached goodies
		Transform _transform;
		Animation _currentAnimation;
		SpriteRenderer _spriteRenderer;

		// playback state
		float _totalElapsedTime;
		float _elapsedDelay;
		int _completedIterations;
		bool _delayComplete;
		bool _isReversed;
		bool _isLoopingBackOnPingPong;



		#region MonoBehavior

		void Awake()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_transform = gameObject.transform;

			if( playAnimationOnStart != string.Empty )
				play( playAnimationOnStart );
		}


		void OnDisable()
		{
			isPlaying = false;
			_currentAnimation = null;
		}


		void Update()
		{
			if( _currentAnimation == null || !isPlaying )
				return;

			// handle delay
			if( !_delayComplete && _elapsedDelay < _currentAnimation.delay )
			{
				_elapsedDelay += Time.deltaTime;
				if( _elapsedDelay >= _currentAnimation.delay )
					_delayComplete = true;
				
				return;
			}

			// count backwards if we are going in reverse
			if( _isReversed )
				_totalElapsedTime -= Time.deltaTime;
			else
				_totalElapsedTime += Time.deltaTime;


			_totalElapsedTime = Mathf.Clamp( _totalElapsedTime, 0f, _currentAnimation.totalDuration );
			_completedIterations = Mathf.FloorToInt( _totalElapsedTime / _currentAnimation.iterationDuration );
			_isLoopingBackOnPingPong = false;


			// handle ping pong loops. if loop is false but pingPongLoop is true we allow a single forward-then-backward iteration
			if( _currentAnimation.pingPong )
			{
				if( _currentAnimation.loop || _completedIterations < 2 )
					_isLoopingBackOnPingPong = _completedIterations % 2 != 0;
			}


			var elapsedTime = 0f;
			if( _totalElapsedTime < _currentAnimation.iterationDuration )
			{
				elapsedTime = _totalElapsedTime;
			}
			else
			{
				elapsedTime = _totalElapsedTime % _currentAnimation.iterationDuration;

				// if we arent looping and elapsedTimei is 0 we are done. Handle it appropriately
				if( !_currentAnimation.loop && elapsedTime == 0 )
				{
					// the animation id done so fire our event
					if( onAnimationCompletedEvent != null )
						onAnimationCompletedEvent( animationIndexForAnimationName( _currentAnimation.name ) );
					
					isPlaying = false;

					switch( _currentAnimation.completionBehavior )
					{
						case Animation.AnimationCompletionBehavior.RemainOnFinalFrame:
						{
							return;
						}
						case Animation.AnimationCompletionBehavior.RevertToFirstFrame:
						{
							break;
						}
						case Animation.AnimationCompletionBehavior.HideSprite:
						{
							_spriteRenderer.sprite = null;
							return;
						}
					}
				}
			}


			// if we reversed the animation and we reached 0 total elapsed time handle un-reversing things and loop continuation
			if( _isReversed && _totalElapsedTime <= 0 )
			{
				_isReversed = false;

				if( _currentAnimation.loop )
				{
					_totalElapsedTime = 0f;
				}
				else
				{
					// the animation id done so fire our event
					if( onAnimationCompletedEvent != null )
						onAnimationCompletedEvent( animationIndexForAnimationName( _currentAnimation.name ) );
					
					isPlaying = false;
					return;
				}
			}

			// time goes backwards when we are reversing a ping-pong loop
			if( _isLoopingBackOnPingPong )
				elapsedTime = _currentAnimation.iterationDuration - elapsedTime;


			// fetch our desired frame
			var desiredFrame = Mathf.FloorToInt( elapsedTime / _currentAnimation.secondsPerFrame );
			if( desiredFrame != currentFrame )
			{
				currentFrame = desiredFrame;
				_spriteRenderer.sprite = _currentAnimation.frames[currentFrame];
				handleFrameChanged();

				// ping-pong needs special care. we don't want to double the frame time when wrapping so we man-handle the totalElapsedTime
				if( _currentAnimation.pingPong && ( currentFrame == 0 || currentFrame == _currentAnimation.frames.Length - 1 ) )
				{
					if( _isReversed )
						_totalElapsedTime -= _currentAnimation.secondsPerFrame;
					else
						_totalElapsedTime += _currentAnimation.secondsPerFrame;
				}
			}
		}

		#endregion


		#region Playback control

		/// <summary>
		/// fetches the animationIndex for the given animationName. Use this to cache the indices to avoid strings!
		/// </summary>
		/// <returns>The index for animation name.</returns>
		/// <param name="animationName">Animation name.</param>
		public int animationIndexForAnimationName( string animationName )
		{
			for( var i = 0; i < animations.Length; i++ )
			{
				if( animations[i].name == animationName )
					return i;
			}

			Debug.LogError( "Animation [" + animationName + "] does not exist" );

			return -1;
		}


		/// <summary>
		/// plays the animation at the given index. You can cache the indices by calling animationIndexForAnimationName.
		/// </summary>
		/// <param name="animationIndex">Animation index.</param>
		/// <param name="startFrame">Start frame.</param>
		public void play( int animationIndex, int startFrame = 0 )
		{
			var animation = animations[animationIndex];

			animation.prepareForUse();

			_currentAnimation = animation;
			isPlaying = true;
			_isReversed = false;
			currentFrame = startFrame;
			_spriteRenderer.sprite = _currentAnimation.frames[currentFrame];

			_totalElapsedTime = (float)startFrame * _currentAnimation.secondsPerFrame;
		}


		public void play( string animationName, int startFrame = 0 )
		{
			var animationIndex = animationIndexForAnimationName( animationName );

			Assert.AreNotEqual<int>( -1, animationIndex, "You attempted to play an animation that doesnt exist!" );

			play( animationIndex, startFrame );
		}


		public bool isAnimationPlaying( string name )
		{
			return ( _currentAnimation != null && _currentAnimation.name == name );
		}


		public void pause()
		{
			isPlaying = false;
		}


		public void unPause()
		{
			isPlaying = true;
		}


		public void reverseAnimation()
		{
			_isReversed = !_isReversed;
		}


		public void stop()
		{
			isPlaying = false;
			_spriteRenderer.sprite = null;
			_currentAnimation = null;
		}

		#endregion


		Animation getAnimation( string name )
		{
			for( var i = 0; i < animations.Length; i++ )
			{
				if( animations[i].name == name )
					return animations[i];
			}

			Debug.LogError( "Animation [" + name + "] does not exist" );

			return null;
		}


		void handleFrameChanged()
		{
			if( _currentAnimation.frameContainsTrigger( currentFrame ) )
			{
				for( var i = 0; i < _currentAnimation.triggers.Length; i++ )
				{
					if( _currentAnimation.triggers[i].frame == currentFrame )
						_currentAnimation.triggers[i].onEnteredFrame.Invoke( currentFrame );
				}
			}
		}


		#region Sprite direction helpers

		public int getFacing()
		{
			return (int)Mathf.Sign( _transform.localScale.x );
		}


		public void flip()
		{
			var scale = _transform.localScale;
			scale.x *= -1f;
			_transform.localScale = scale;
		}


		public void faceLeft()
		{
			var scale = _transform.localScale;
			if( scale.x > 0f )
			{
				scale.x *= -1f;
				_transform.localScale = scale;
			}
		}


		public void faceRight()
		{
			var scale = _transform.localScale;
			if( scale.x < 0f )
			{
				scale.x *= -1f;
				_transform.localScale = scale;
			}
		}
	
		#endregion
	}
}