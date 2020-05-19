/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file Player.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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
/// Represents the user / player interacting with the stage. 
/// The Update(Gametime) handles both user keyboard and gamepad controller input.
/// If there is a gamepad attached the keyboard inputs are not processed.
/// 
/// removed game controller code from Update()
/// 
/// 2/8/2014 last changed
/// </summary>

public class Player : Agent {
   private KeyboardState oldKeyboardState;  
   private int rotate;
   private float angle;
   private Matrix initialOrientation;
   private int score; // This is the treasure score of the Player aka Evader - added by David Kopp
   private bool treasureMode; // This is to enable and disable treasureMode (Not Implemented, but the idea is when treasure mode 
                              // for the NPAgent is enbled the Player would also enter treasure mode and race to tag a untagged treasure) - added by David Kopp

   public Player(Stage theStage, string label, Vector3 pos, Vector3 orientAxis, 
   float radians, string meshFile)
   : base(theStage, label, pos, orientAxis, radians, meshFile)
      {  // change names for on-screen display of current camera
      first.Name =  "First";
      follow.Name = "Follow";
      above.Name =  "Above";
      IsCollidable = true;  // players test collision with Collidable set.
      stage.Collidable.Add(agentObject);  // player's agentObject can be collided with by others.
      rotate = 0;
      angle = 0.01f;
      initialOrientation = agentObject.Orientation;
	  score = 0; // Initializing Score of Player to 0 - added by David Kopp 
	  treasureMode = true; // Initializing treasureMode to false (Changed from false to true to always be in treasure mode) - added by David Kopp 
        }

    /// <summary>
    /// Player / Evader's score - by David Kopp
    /// </summary>
    public int Score {
	    get { return score; }}

    /// <summary>
    /// Enables or Disables treasure mode - by David Kopp
    /// </summary>
    public bool TreasureMode {
        get { return treasureMode; }
        set { treasureMode = value; }}

   /// <summary>
   /// Handle player input that affects the player.
   /// See Stage.Update(...) for handling user input that affects
   /// how the stage is rendered.
   /// First check if gamepad is connected, if true use gamepad
   /// otherwise assume and use keyboard.
   /// (Added by David Kopp) If the Player / Evader gets within 
   /// 300 pixels of a treasure that is not tagged then it will be 
   /// tagged and their score will increase by 1. If there is no
   /// untagged treasure then treasureMode = false.
   /// </summary>
   /// <param name="gameTime"></param>
   public override void Update(GameTime gameTime) {
      KeyboardState keyboardState = Keyboard.GetState();
      if (keyboardState.IsKeyDown(Keys.R) && !oldKeyboardState.IsKeyDown(Keys.R)) 
         agentObject.Orientation = initialOrientation; 
      // allow more than one keyboardState to be pressed
      if (keyboardState.IsKeyDown(Keys.Up)) agentObject.Step++;
      if (keyboardState.IsKeyDown(Keys.Down)) agentObject.Step--; 
      if (keyboardState.IsKeyDown(Keys.Left)) rotate++;
      if (keyboardState.IsKeyDown(Keys.Right)) rotate--;
      oldKeyboardState = keyboardState;    // Update saved state.
      agentObject.Yaw = rotate * angle;
		// Player / Evader tags closest Treasure if it is untagged - added by David Kopp
		NavNode temp = stage.Treasure.getTreasure(agentObject.Translation);
			if (temp != null && Vector3.Distance ((temp.Translation), agentObject.Translation) < 75 && treasureMode) {
				stage.Treasure.tagTreasure (temp.Translation); // Player tags the treasure - by David Kopp
//				treasureMode = false; // Uncomment to have the player play the treasure game - by David Kopp
				score++; // Increase Player / Evader's score by 1 - by David Kopp
			} else if (temp == null) { // All treasures are tagged - by David Kopp
				treasureMode = false;
			}
      base.Update(gameTime);
      rotate = agentObject.Step = 0;
      }
   }
}
