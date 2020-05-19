/*  
	    Copyright (C) 2016 G. Michael Barnes
	 
	    The file Pack.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
    MonoGames 3.2 to MonoGames 3.4  

    AGMGSKv7 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//#if MONOGAMES //  true, build for MonoGames
//   using Microsoft.Xna.Framework.Storage;
//#endif
#endregion

namespace AGMGSKv7
{

/// <summary>
/// Pack represents a "flock" of MovableObject3D's Object3Ds.
/// Usually the "player" is the leader and is set in the Stage's LoadContent().
/// With no leader, determine a "virtual leader" from the flock's members.
/// Model3D's inherited List<Object3D> instance holds all members of the pack.
/// 
/// 2/1/2016 last changed
/// </summary>
public class Pack : MovableModel3D
{
	Object3D leader;

	/// <summary>
	/// Variable declarations added by David Kopp 
	/// </summary>
	double[] PackingProbability = new double[] { 0.0, 0.33, 0.66, 0.99 };
	int probabilityIndex = 0;
	int dogs;


	/// <summary>
	/// Construct a pack with an Object3D leader
	/// </summary>
	/// <param name="theStage"> the scene </param>
	/// <param name="label"> name of pack</param>
	/// <param name="meshFile"> model of a pack instance</param>
	/// <param name="xPos, zPos">  approximate position of the pack </param>
	/// <param name="aLeader"> alpha dog can be used for flock center and alignment </param>
	public Pack (Stage theStage, string label, string meshFile, int nDogs, int xPos, int zPos, Object3D theLeader)
		: base (theStage, label, meshFile)
	{
		isCollidable = true;
		random = new Random ();
		leader = theLeader;
		int spacing = stage.Spacing;
		// initial vertex offset of dogs around (xPos, zPos)
		int[,] position = {{ 0, 0 }, { 7, -4 }, { -5, -2 }, { -7, 4 }, { 5, 2 }, { 10, 7}, { -10, -7 }, { 12, 12 }};
		for (int i = 0; i < position.GetLength (0); i++) {
			int x = xPos + position [i, 0];
			int z = zPos + position [i, 1];
			float scale = (float)(0.5 + random.NextDouble ());
			addObject (new Vector3 (x * spacing, stage.surfaceHeight (x, z), z * spacing),
				new Vector3 (0, 1, 0), 0.0f,
				new Vector3 (scale, scale, scale));
		}
		dogs = nDogs;
	}

	/// <summary>
	/// Attribute that helps change the probability of the dogs wanting to pack.
	/// </summary>
	/// <value>The index of the probability.</value>
	public int ProbabilityIndex {
		get { return probabilityIndex; }
		set { probabilityIndex = value; }}

	/// <summary>
	/// Attributes to get the number of dogs. 
	/// </summary>
	/// <value>The dogs.</value>
	public int Dogs {
		get { return dogs; }}

	/// <summary>
	/// Attributes to get the packing probability before changing it or acting on it.
	/// </summary>
	/// <value>The packing value.</value>
	public double PackingValue {
		get { return PackingProbability [probabilityIndex]; }}

	/// <summary>
	/// Each pack member's orientation matrix will be updated.
	/// Distribution has pack of dogs moving randomly.  
	/// Supports leaderless and leader based "flocking" 
	/// </summary>      
	public override void Update (GameTime gameTime)
	{ //Need to update
		// if (leader == null) need to determine "virtual leader from members"
		if (random.NextDouble () < PackingProbability [probabilityIndex]) {
				foreach (Object3D obj in instance) {
					float angle = 0.02f; // This angle is a little more than 1 degree in radians.
					obj.Yaw = 0.0f;
					Vector3 alignment = calcAlignment(obj); // calculates alignment - By David Kopp
					Vector3 cohesion = calcCohesion(obj); // calculates cohesion - By David Kopp
					Vector3 separation = calcSeparation(obj); // calculates seperation - By David Kopp
					Vector3 forward = obj.Forward;	// forward vector			
					Vector3 flockingVector = alignment + cohesion + separation; // get packing vector from combined forces for each dog
					
					// Dog is packing apply packing forces and rotate dog to leaders forward vector.
					if (flockingVector != Vector3.Zero) { 
						flockingVector.Normalize();
						forward.Normalize();
						Vector3 faceForward = Vector3.Cross(forward, flockingVector);
						faceForward.Normalize();
						Vector3 rotation = Vector3.Cross(forward, flockingVector);
						rotation.Normalize();
						double cosRotation = Math.Acos(Vector3.Dot(flockingVector, forward)) / Vector3.Distance(
							forward, Vector3.Zero) * Vector3.Distance(flockingVector, Vector3.Zero);		
						
						if (rotation.X + rotation.Y + rotation.Z < 0) {
							angle = -angle;
						}
						
						obj.Yaw += angle;				 
					} else { }
		obj.updateMovableObject ();				
		stage.setSurfaceHeight (obj);
		}
		} else { // Dog is not flocking, but rather is in exploration mode - go dog go!
			foreach (Object3D obj in instance) {
			float angle = 0.3f;
			obj.Yaw = 0.0f;
			// change direction 4 time a second  0.07 = 4/60
			if (random.NextDouble () < 0.07) {
				if (random.NextDouble () < 0.5) {
					obj.Yaw -= angle; // turn left
				} else {
					obj.Yaw += angle; // turn right
				}
			}
			obj.updateMovableObject ();
			stage.setSurfaceHeight (obj);
		}
		base.Update (gameTime);  // MovableMesh's Update(); 
		}
}
	
	// Attributes for getting and assigning the leader. 
	public Object3D Leader {
		get { return leader; }
		set { leader = value; }
	}
	
	/// <summary>
	/// Calculates the Alignment force for a dog based on its distance from the leader.
	/// </summary>
	/// <returns>The alignment.</returns>
	/// <param name="obj">Object.</param>
	public Vector3 calcAlignment(Object3D obj) {
			float dist = Vector3.Distance(obj.Translation, leader.Translation);		
			
			Vector3 alignment = new Vector3(leader.Forward.X, 0, leader.Forward.Z);

			if (dist > 1000.0f && dist < 3000.0f) {
				return Vector3.Normalize(alignment);
			} else { return Vector3.Zero; }
	}

	/// <summary>
	/// Calculates the cohesion force by looking at the leader and dogs distance.
	/// </summary>
	/// <returns>The cohesion.</returns>
	/// <param name="obj">Object.</param>
	public Vector3 calcCohesion(Object3D obj) {
		float dist = Vector3.Distance(obj.Translation, leader.Translation);			
		Vector3 objLoc, leaderLoc, toLeader;

		if (dist > 3000.0f) {
			objLoc = new Vector3(obj.Translation.X, 0, obj.Translation.Z);
			leaderLoc = new Vector3(leader.Translation.X, 0, leader.Translation.Z);
			toLeader = leaderLoc - objLoc;
	
			return Vector3.Normalize(toLeader);
			} else { return Vector3.Zero; }
	}

	/// <summary>
	/// Calculates the separation force by using the distance from leader and all other dogs.
	/// </summary>
	/// <returns>The separation.</returns>
	/// <param name="obj">Object.</param>
	public Vector3 calcSeparation(Object3D obj) {
		float dist = Vector3.Distance(obj.Translation, leader.Translation);			
		Vector3 objLoc, toDogs, objectLoc, seperation;
		seperation = Vector3.Zero;
		
		objLoc = new Vector3(obj.Translation.X, 0, obj.Translation.Z);
		
		if (dist < 1000.0f) {
			foreach (Object3D object3D in instance) {
				if (object3D != obj) {
					objectLoc = new Vector3(object3D.Translation.X, 0, object3D.Translation.Z); 	 
					toDogs = objectLoc - objLoc;
					seperation = Vector3.Zero - toDogs; 
				} 
			}
		  	
			objectLoc = new Vector3(leader.Translation.X, 0, leader.Translation.Z);
			toDogs = objectLoc - objLoc;
			seperation = seperation - toDogs; 

			return Vector3.Normalize(seperation); 
			} else { return Vector3.Zero; }
	}
}
}