/*  
    Copyright (C) 2016 G. Michael Barnes

 
    The file Treasure.cs is an additional part of AGMGSKv7 a port and update of AGXNASKv6 from
    MonoGames 3.2 to MonoGames 3.4. Treasure.cs was created by David Kopp. User Treasure.cs
    at your own risk and please rememeber it is for David Kopp's educational purposes.  

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
#endregion

/// <summary>
/// This Class builds four treasures in Stage at predetermined locations. It also has methods to check
/// distances from a moveable object to a treasure, if the treasure has been tagged, or if the treasure 
/// game is over. - by David Kopp
/// </summary>

namespace AGMGSKv7
{
	public class Treasure : Model3D
	{
		private List<Vector3> treasureLocations;
		private int[] tagged; // Array for keeping track of tagged treasures (0 for not tagged | 1 for tagged)
		private bool gameOver; // Enables or Disables the treasure game

        /// <summary>
        /// Constructor to create the treasures and set the gameOver to false (Which starts the treasure game).
        /// </summary> 
		public Treasure (Stage theStage, string label, string meshFile) : base(theStage, label, meshFile)
		{
			gameOver = false;
			addTreasures ();
		}

        /// <summary>
        /// Enables and disables the treasure game to be over or not
        /// </summary>
		public bool GameOver {
			get { return gameOver; }
			set { gameOver = value; }
		}

        /// <summary>
        /// This method checks to see if the treasure Vector3 is tagged
        /// or not.
        /// </summary>
        /// <param name="treasure">The treasure Vector3 that the object is headed to.</param>
        /// <returns></returns>
		public bool isTagged(Vector3 treasure) {
			int i = 0;
			foreach (Vector3 loc in treasureLocations) {
				if (treasure.X == loc.X && tagged[i] == 1) { return true; }
				i++;
			}
			return false;
		}

        /// <summary>
        /// This method finds and tags the treasure Vector3 that corresponds to the tagged array.
        /// </summary>
        /// <param name="treasure">The treasure Vector3 to tag.</param>
		public void tagTreasure(Vector3 treasure) {
			int i = 0;
			foreach (Vector3 loc in treasureLocations) {
				if (treasure.X == loc.X) {
					if (tagged [i] == 0) {
						tagged [i] = 1;
						treasure.Y = 50;
						Model3D tag = new Model3D (stage, "tag", "greenTagBox");
						tag.IsCollidable = false;
						tag.addObject (treasure, Vector3.Up, 0.0f);
						stage.Components.Add (tag);
					} else {
						Console.WriteLine ("Already tagged this treasure!"); // Testing purposes
					}
				}
				i++;
			}
			if (getTreasure(treasure) == null) {
				GameOver = true;
			}
		}

        /// <summary>
        /// This method finds the closest untagged treasure and returns its NavNode.
        /// </summary>
        /// <param name="position">The Vector3 of the MoveableModel3D</param>
        /// <returns></returns>
		public NavNode getTreasure(Vector3 position) {
			for (int i = 0; i < tagged.Length; i++) {
				if (tagged [i] == 0 && isClosest (treasureLocations [i], position)) {
					return new NavNode (treasureLocations [i]); 	
				}	
			}	
			return null;
		}

        /// <summary>
        /// This method finds out if a treasure Vector3 and position is the closest 
        /// among the rest of the untagged treasures.
        /// </summary>
        /// <param name="treasure">The treasure Vector3.</param>
        /// <param name="position">The position Vector3 of an MovableModel3D</param>
        /// <returns></returns>
		public bool isClosest(Vector3 treasure, Vector3 position) {
			float distance = Vector3.Distance(treasure, position);
			for (int i = 0; i < tagged.Length; i++) {
				if (!isTagged(treasureLocations[i])) {
						float temp = Vector3.Distance(treasureLocations[i], position);
							if (distance > temp) {
							return false;
							}	
					}
				}
				return true;
		}

        /// <summary>
        /// This method checks to see if a position Vector3 is a
        /// legal treasure.
        /// </summary>
        /// <param name="treasure">A position Vector3.</param>
        /// <returns></returns>
		public bool isTreasure(Vector3 treasure) {
			foreach (Vector3 loc in treasureLocations) {
				if (treasure.X == loc.X) {
					return true;
				}
			}
			return false;
		}
		
		public float distanceToTreasure(Vector3 pos, Vector3 treasure) {
			return Vector3.Distance(pos, treasure);
		}		

        /// <summary>
        /// This method adds the four static treasures to the scene and initializes the tagged array. Updated the
		/// treasure locations to make sure they are not in a wall.
        /// </summary>
		private void addTreasures() {
			treasureLocations = new List<Vector3> ();
			tagged = new int[5] {0, 0, 0, 0, 0};  // 0 means it is not tagged, 1 means it has already been tagged
			IsCollidable = false; // Treasures are not collidable
			int spacing = stage.Spacing;
			Terrain terrain = stage.Terrain;
			treasureLocations.Add (new Vector3 (442 * spacing, terrain.surfaceHeight (442, 466), 466 * spacing)); 
			treasureLocations.Add (new Vector3 (418 * spacing, terrain.surfaceHeight (418, 442), 442 * spacing)); 
			treasureLocations.Add (new Vector3 (420 * spacing, terrain.surfaceHeight (420, 481), 481 * spacing)); 
			treasureLocations.Add (new Vector3 (474 * spacing, terrain.surfaceHeight (474, 474), 474 * spacing));  
			treasureLocations.Add (new Vector3 (463 * spacing, terrain.surfaceHeight (463, 417), 417 * spacing));
			
			foreach (Vector3 treasure in treasureLocations) {
				addObject (treasure, Vector3.Up, 0.0f);
			}
		}
	}
}