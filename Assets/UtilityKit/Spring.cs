using UnityEngine;
using System.Collections;


public class Spring
{
	public float springConstant = 0.015f;
	public float damping = 0.07f;
	public float velocity;
	public float acceleration;

	private float _springPosition;
	private float _nuetralPosition = 0f;


	public float simulate()
	{
		var force = springConstant * ( _springPosition - _nuetralPosition ) + velocity * damping;
		acceleration = -force;
		_springPosition += velocity;
		velocity += acceleration;

		return _springPosition;
	}


	public void applyForceStartingAtPosition( float force, float position )
	{
		acceleration = 0f;
		_springPosition = position;
		velocity = force;
	}


	public void applyAdditiveForce( float force )
	{
		velocity += force;
	}

}
