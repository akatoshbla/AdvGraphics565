/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file NPAgent.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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

namespace AGMGSKv7 {

/// <summary>
/// A non-playing character that moves.  Override the inherited Update(GameTime)
/// to implement a movement (strategy?) algorithm.
/// Distribution NPAgent moves along an "exploration" path that is created by the
/// from int[,] pathNode array.  The exploration path is traversed in a reverse path loop.
/// Paths can also be specified in text files of Vector3 values, see alternate
/// Path class constructors.
/// 
/// 1/20/2016 last changed
/// </summary>
public class NPAgent : Agent {
   private int score; // This is the treasure score of the NPAgent aka Chaser - added by David Kopp
   protected bool treasureMode; // This bool enables treasure mode or not - added by David Kopp
   private NavNode nextGoalTemp; // This NavNode holds the value of the nextGoal the npc was going to before treasureMode was set to true - added by David Kopp
   private NavNode nextGoal;
   private Path path;
   private int snapDistance = 20;  // this should be a function of step and stepSize
	// If using makePath(int[,]) set WayPoint (x, z) vertex positions in the following array
	private int[,] pathNode = { {505, 490}, {500, 500}, {490, 505},  // bottom, right
										 {435, 505}, {425, 500}, {420, 490},  // bottom, middle
										 {420, 450}, {425, 440}, {435, 435},  // middle, middle
                               {490, 435}, {500, 430}, {505, 420},  // middle, right
										 {505, 105}, {500,  95}, {490,  90},  // top, right
                               {110,  90}, {100,  95}, { 95, 105},  // top, left
										 { 95, 480}, {100, 490}, {110, 495},  // bottom, left
										 {495, 480} };								  // loop return

   /// <summary>
   /// Create a NPC. 
   /// AGXNASK distribution has npAgent move following a Path.
   /// </summary>
   /// <param name="theStage"> the world</param>
   /// <param name="label"> name of </param>
   /// <param name="pos"> initial position </param>
   /// <param name="orientAxis"> initial rotation axis</param>
   /// <param name="radians"> initial rotation</param>
   /// <param name="meshFile"> Direct X *.x Model in Contents directory </param>
   public NPAgent(Stage theStage, string label, Vector3 pos, Vector3 orientAxis, 
			float radians, string meshFile, Treasure treasure) // Edited by David Kopp
      : base(theStage, label, pos, orientAxis, radians, meshFile)
      {  // change names for on-screen display of current camera
      first.Name =  "npFirst";
      follow.Name = "npFollow";
      above.Name =  "npAbove";
      // path is built to work on specific terrain, make from int[x,z] array pathNode
      path = new Path(stage, pathNode, Path.PathType.LOOP); // continuous search path
      stage.Components.Add(path);
      nextGoal = path.NextNode;  // get first path goal
      agentObject.turnToFace(nextGoal.Translation);  // orient towards the first path goal
		// set snapDistance to be a little larger than step * stepSize
		snapDistance = (int) (1.5 * (agentObject.Step * agentObject.StepSize));
		treasureMode = false; // Treasure mode initilized to false - added by David Kopp
		score = 0; // Score of NPAgent initialized to 0 - added by David Kopp
      }

        /// <summary>
        /// Chaser's / NPAgent's score. - by David Kopp
        /// </summary>
        public int Score {
			get { return score; }
		}

        /// <summary>
        /// Enables or disables treasure mode for Chaser / NPAgent. - by David Kopp
        /// </summary>
        public bool TreasureMode {
			get { return treasureMode; }
			set { treasureMode = value; }
		}

   /// <summary>
   /// Simple path following.  If within "snap distance" of a the nextGoal (a NavNode) 
   /// move to the NavNode, get a new nextGoal, turnToFace() that goal.  Otherwise 
   /// continue making steps towards the nextGoal. (Added by David Kopp) If treasure mode is enabled the nextGoal will be
   /// the closest treasure to the NPAgent. After the NPAgent tags the treasure nextGoal will be the last NavNode the 
   /// NPAgent was going to before entering treasure mode.
   /// </summary>
   public override void Update(GameTime gameTime) {
			// Check for if in treasureMode and nextGoal is not a treasure, and the game is not over - by David Kopp	
			if (treasureMode & !stage.Treasure.isTreasure (nextGoal.Translation) & !stage.Treasure.GameOver) {
				NavNode temp = stage.Treasure.getTreasure (agentObject.Translation); // Location of closest treasure - by David Kopp
				if (temp != null && !stage.Treasure.isTagged (temp.Translation)) { // If there is a treasure and the treasure is untagged - by David Kopp
					nextGoalTemp = nextGoal; // Hold the NavNode the Agent was going to before Treasure mode is enabled - by David Kopp
					nextGoal = temp;
				}
			} else if (treasureMode && stage.Treasure.isTreasure (nextGoal.Translation)) { // Check if treasure mode is enabled and NPAgent is going to a treasure - by David Kopp
				if (stage.Treasure.GameOver) { // If the game is over in route to a treasure, that was just tagged, then the NPAgent goes back to following the static path - by David Kopp
					nextGoal = nextGoalTemp;
				} else if (!stage.Treasure.GameOver && stage.Treasure.isTagged (nextGoal.Translation)) { // If the game is not over and in route the treasure gets tagged - by David Kopp
					NavNode switchTemp = stage.Treasure.getTreasure (agentObject.Translation); // Switch nextGoal with an untagged treasure - by David Kopp
					if (switchTemp != null) { // If the game is not over and in route to a treasure that just  got tagged, then NPAgent will goto the next closest untagged treasure - by David Kopp
						nextGoal = switchTemp;
					} else { // If the game is not over in route to a treasure and the treasure, which was the last untagged, gets tagged. NPAgent goes back to the static path and disable treasure mode - by David Kopp
						nextGoal = nextGoalTemp;
						treasureMode = !treasureMode;
					}
				}
			} else if (!treasureMode && stage.Treasure.isTreasure (nextGoal.Translation)) { // If treasure mode is disabled and the NPAgent was heading for a Treasure (Not implemented) - by David Kopp
				nextGoal = nextGoalTemp;
			}

		agentObject.turnToFace(nextGoal.Translation);  // adjust to face nextGoal every move
		// See if at or close to nextGoal, distance measured in 2D xz plane
		float distance = Vector3.Distance(
			new Vector3(nextGoal.Translation.X, 0, nextGoal.Translation.Z),
			new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
		stage.setInfo(15, stage.agentLocation(this));
      stage.setInfo(16,
			string.Format("          nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})", 
				nextGoal.Translation.X/stage.Spacing, nextGoal.Translation.Y, nextGoal.Translation.Z/stage.Spacing, distance) );
			if (distance <= snapDistance && !stage.Treasure.isTreasure (nextGoal.Translation)) { // Added if the nextGoal is not a treasure - by David Kopp 
				// snap to nextGoal and orient toward the new nextGoal 
				nextGoal = path.NextNode;
				// agentObject.turnToFace(nextGoal.Translation);
			} else if (distance <= snapDistance && stage.Treasure.isTreasure (nextGoal.Translation)) { // If the NPAgent approaches a treasure and tags it - Added by David Kopp
				stage.Treasure.tagTreasure (nextGoal.Translation); // Tag the treasure - by David Kopp
				nextGoal = nextGoalTemp; // NPAgent head back to last NavNode before treasure mode was enabled -by David Kopp
				treasureMode = false; // Disable treasure mode - by David Kopp
				score++; // Increase NPAgent's treasure score by 1 - by David Kopp
			}
      base.Update(gameTime);  // Agent's Update();
      }
   } 
}
