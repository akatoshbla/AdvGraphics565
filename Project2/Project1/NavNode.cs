/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file NavNode.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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
/// A WayPoint or Marker to be used in path following or path finding.
/// Four types of WAYPOINT:
/// <list type="number"> WAYPOINT, a navigatable terrain vertex </list>
/// <list type="number"> PATH, a node in a path (could be the result of A*) </list>
/// <list type="number"> OPEN, a possible node to follow in an A*path</list>
/// <list type="number"> CLOSED, a node that has been evaluated by A* </list>
 
/// 
/// 2/14/2012  last update
/// </summary>
public class NavNode : IComparable<NavNode> {
   public enum NavNodeEnum { WAYPOINT, PATH, OPEN, CLOSED, COLLISIONPOINT };
   private double distance;  // can be used with A* path finding.
   private Vector3 translation;
   private NavNodeEnum navigatable;
   private Vector3 nodeColor;
	
   // Added by David Kopp (new stuff)
	private float distanceFromSource, distanceToGoal, cost;
	private List<NavNode> adjacent;
	private NavNode pathPredecessor;
	private float x, z, diagOffset;
		
// constructors

   /// <summary>
   /// Make a VERTEX NavNode
   /// </summary>
   /// <param name="pos"> location of WAYPOINT</param>
   public NavNode(Vector3 pos) {
      translation = pos;
	  x = pos.X;
	  z = pos.Z;
      Navigatable = NavNodeEnum.WAYPOINT;
	  distanceFromSource = 0.0f;
	  distanceToGoal = 0.0f;
	  diagOffset = 0.0f;
	  adjacent = new List<NavNode>();
	  cost = 0;
    }

   /// <summary>
   /// Make a WAYPOINT and set its Navigational type
   /// </summary>
   /// <param name="pos"> location of WAYPOINT</param>
   /// <param name="nType"> Navigational type {VERTEX, WAYPOINT, A_STAR, PATH} </param>
   public NavNode(Vector3 pos, NavNodeEnum nType) {
      translation = pos;
	  x = pos.X;
	  z = pos.Z;
      Navigatable = nType;
      distanceFromSource = 0.0f;
	  distanceToGoal = 0.0f;
	  diagOffset = 0.0f;
	  adjacent = new List<NavNode>();
	  cost = 0;
      }

   public NavNode(Vector3 pos, NavNodeEnum nType, float offset) {
      translation = pos;
	  x = pos.X;
	  z = pos.Z;
      Navigatable = nType;
      distanceFromSource = 0.0f;
	  distanceToGoal = 0.0f;
	  diagOffset = offset;
	  adjacent = new List<NavNode>();
	  cost = 0;
      }
// properties

   public Vector3 NodeColor {
      get { return nodeColor; }}

   public Double Distance {
      get { return distance; }
      set { distance = value; }
      }

   /// <summary>
   /// When changing the Navigatable type the WAYPOINT's nodeColor is 
   /// also updated.
   /// </summary>
   public NavNodeEnum Navigatable {
      get { return navigatable; }
      set { navigatable = value; 
            switch (navigatable) {
               case NavNodeEnum.WAYPOINT : nodeColor = Color.Yellow.ToVector3(); break;  // yellow
               case NavNodeEnum.PATH     : nodeColor = Color.Blue.ToVector3();   break;  // blue
               case NavNodeEnum.OPEN     : nodeColor = Color.White.ToVector3();  break;  // white
               case NavNodeEnum.CLOSED   : nodeColor = Color.Red.ToVector3();    break;  // red
			   case NavNodeEnum.COLLISIONPOINT : nodeColor = Color.Green.ToVector3(); break; // green - Added by David Kopp
               }
            }} 

	
	// Testing purposes - Added by David Kopp
	public void ToString(NavNode node) {
			Console.WriteLine("NavNode: " + node.X + ", " + node.Z + " - " + node.navigatable);	
	}

	// Testing purposes - Added by David Kopp
	public void adjList() {
		foreach (NavNode node in adjacent) {
			Console.Write("(" + node.Translation.X + ", " + node.Translation.Z + ") ");	
		}
	}

	// Insert node to the adjacency List - Added by David Kopp
	public void insertAdjacentNode(NavNode adjacentNode) {
		if (adjacentNode != null) {
			adjacent.Add(adjacentNode);
		}
    }

	// Public access to the adjaceny list for astar and graph.
	public List<NavNode> Adjacency {
		get { return adjacent; }}	

   public Vector3 Translation {
      get { return translation; }
      }

	public float X {
		get { return x; }
		set { x = value; }}

	public float Z {
		get { return z; }
		set { z = value; }}

	/// <summary>
	/// Attributes for offset. I named it Diag Offset because the distance is by the diagional distance
	/// from the center and a corner of the stamping. This was not needed as it is a static number because stamping only
	/// has two cases. Normal or Fine stamping.
	/// </summary>
	/// <value>The diag offset.</value>
	public float DiagOffset {
		get { return diagOffset; }}

	/// <summary>
	/// Cost attributes used by astar.
	/// </summary>
	/// <value>The cost.</value>
	public float Cost {
		get { return cost; }
		set { cost = value; }} 

	/// <summary>
	/// Distance from source attributes used by astar.
	/// </summary>
	/// <value>The distance from source.</value>
	public float DistanceFromSource {
		get { return distanceFromSource; }
		set { distanceFromSource = value; }} 

	/// <summary>
	/// Distance to goal attributes used by astar.
	/// </summary>
	/// <value>The distance to goal.</value>
	public float DistanceToGoal {
		get { return distanceToGoal; }
		set { distanceToGoal = value; }} 

	/// <summary>
	/// Predecessor path used by astar for final path.
	/// </summary>
	/// <value>The path predecessor.</value>
	public NavNode PathPredecessor {
		get { return pathPredecessor; }
			set { pathPredecessor = value; }}	 

// methods

   /// <summary>
   /// Useful in A* path finding 
   /// when inserting into an min priority queue open set ordered on distance
   /// </summary>
   /// <param name="n"> goal node </param>
   /// <returns> usual comparison values:  -1, 0, 1 </returns>
   public int CompareTo(NavNode n) {
      if (distance < n.Distance)       return -1;
      else if (distance > n.Distance)  return  1;
      else                             return  0;
      }
      
   }
}
