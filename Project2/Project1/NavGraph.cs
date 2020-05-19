/// <summary>
/// Programmer: David Kopp		
/// Project 2 : AGMGSKv7
/// Description: This class is a NavGraph that contains a dictionary of a keyvalue pair like a hashmap in java.
///					Due to my horribly busy life outside of school - I was not able to get the astar algorithm to work properly. (Note to self - summer project)
/// </summary>

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace AGMGSKv7
{
	public class NavGraph : DrawableGameComponent {
		// Variables and Data Structures
		private Dictionary<String, NavNode> graph;
		private Stage stage;
		private List<NavNode> open, closed, path;
		private bool aStarDone;		

		/// <summary>
		/// Constructor for NavGraph which holds the graph of all the stamped navnodes that can be traversed
		/// By David Kopp
		/// </summary>
		/// <param name="stage">Stage.</param>
		public NavGraph (Stage stage) : base(stage) {
			this.stage = stage;
			graph = new Dictionary<string, NavNode>();
			open = new List<NavNode>();
			closed = new List<NavNode>();
			path = new List<NavNode>();
			aStarDone = false;
		}

		/// <summary>
		/// Gets the a navnode from graph by keyvalue.
		/// Sets the navnode of a keyvalue.
		/// Added by David Kopp
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="z">The z coordinate.</param>
		public NavNode this [int x, int z] {
  			get { return graph[skey(x, z)]; }
		    set { graph[skey(x, z)] = value; }}	

		/// <summary>
		/// Inserts a NavNode into the graph with a keyvalue pairing. Also will not let you overwrite already existing keys.	
		/// Works great for when you want to stamp out collidable objects.
		/// </summary>
		/// <param name="node">Node.</param>
		public void insertNavNode(NavNode node) {
			if (!graph.ContainsKey(skey((int)node.X, (int)node.Z))) {
				graph.Add(skey((int)node.X, (int)node.Z), node);
			} else { /*Console.WriteLine("Copy Node Discarded!");*/ }
        }

		/// <summary>
		/// Format for the key for the dictionary graph.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="z">The z coordinate.</param>
		private String skey(int x, int z) {
   	 	   return String.Format("{0}::{1}", x, z); 
		}

		/// <summary>
		/// This removes all the collision nodes out of the graph. Run after navgraph has been stamped.
		/// </summary>
		public void removeCollisionNodes() {
			Dictionary<String, NavNode> temp = new Dictionary<string, NavNode>();

			foreach (KeyValuePair<String, NavNode> node in graph) {
				if (!(node.Value.Navigatable == NavNode.NavNodeEnum.COLLISIONPOINT)) {
					temp.Add(node.Key, node.Value);
				}
			}

			graph = temp;
		}

		/// <summary>
		/// Creates all the adjacencies by looking at the distance of each node against itself. If the node
		/// is within its offset then they are linked. 
		/// </summary>
		public void createAdjacentGraph() {
			float distance2Node = 0.0f;
			foreach (KeyValuePair<String, NavNode> node in graph) {
				foreach (KeyValuePair<String, NavNode> nextNode in graph) {
					distance2Node = Vector3.Distance(node.Value.Translation, nextNode.Value.Translation);
					if ((!node.Key.Equals(nextNode.Key)) && (distance2Node <= node.Value.DiagOffset)) {
						node.Value.insertAdjacentNode(nextNode.Value);
					} 
				}
			}
		}
		
		/// <summary>
		/// This displays all the adjagencies of all the nodes. For testing purposes.
		/// </summary>
		public void displayAllAdjagency() {
			foreach (KeyValuePair<String, NavNode> node in graph) {
				Console.WriteLine("KeyVale: {0}", node.Key);
				Console.WriteLine("Adjacencies: {0}", node.Value.Adjacency.Count());
				node.Value.adjList();
				//Console.WriteLine("DiagOfset: {0}", node.Value.DiagOffset);
			}
		}
		/// <summary>
		/// Find closest node to a position vector. Used in the astar algorithm. - Added by David Kopp
		/// </summary>
		/// <returns>The graph point.</returns>
		/// <param name="pos">Position.</param>
		public NavNode nearestGraphPoint(Vector3 pos) {
			float smallestDistance = 1000000.0f;
			float tempDistance = 0.0f;
			NavNode navNode = new NavNode(new Vector3(0,0,0));
			
			foreach (KeyValuePair<String, NavNode> node in graph) {
				tempDistance = Vector3.Distance(node.Value.Translation, pos);
				
				if (tempDistance < smallestDistance) {
					smallestDistance = tempDistance;
					navNode = node.Value;
				}
			}

			return navNode; 
		}
		
		/// <summary>
		/// Displays the node keys. For testing purposes
		/// </summary>
		public void displayNodeKeys()
        {
			if (graph.Count == 0) {
				Console.WriteLine("Dictionary is empty!!!!");
			} else {
            	foreach (KeyValuePair<String, NavNode> node in graph) {
                	Console.WriteLine("KeyValue: {0}", node.Key);
            	}
			}
        }

		/// <summary>
		/// This is the aStar Algorithm. It is still a work in progress. The open, closed, and path lists are not being 
		/// populated correctly and needs to be reworked. - By David Kopp (The algorithm was created by some psuedo code
		/// in lecture notes.
		/// </summary>
		/// <returns>The star algorithm.</returns>
		/// <param name="source">Source.</param>
		/// <param name="destination">Destination.</param>
		public List<NavNode> aStarAlgorithm(NavNode source, NavNode destination) {
			path = new List<NavNode>();
			open = new List<NavNode>();
			closed = new List<NavNode>();
			aStarDone = false;
			
			NavNode current = source;
			current.Cost = 0;	
			
			open.Add(current);		

			// Keep search till you run out of open nodes.
			while (open.Count != 0) {
				current = open.First<NavNode>();
				open.Remove(open.First<NavNode>());

				// If we are at the destination we have the path. Done.
				if (current.Translation == destination.Translation) {
					aStarDone = true;
					break;
				}

				closed.Add(current);
				current.Navigatable = NavNode.NavNodeEnum.CLOSED;

				// Evaluate the cost of travel and find the next node.
				foreach (NavNode adjacent in current.Adjacency) {
					if (!open.Contains(adjacent) && !closed.Contains(adjacent)) {
						adjacent.DistanceFromSource = current.DistanceFromSource + Vector3.Distance(current.Translation, 
							adjacent.Translation);
						adjacent.DistanceToGoal = Vector3.Distance(current.Translation, adjacent.Translation) +
							Vector3.Distance(adjacent.Translation, destination.Translation);
						adjacent.Cost = adjacent.DistanceFromSource + adjacent.DistanceToGoal;
						open.Add(adjacent);
						adjacent.Navigatable = NavNode.NavNodeEnum.OPEN;
						adjacent.PathPredecessor = current;
					}
				}
			}

			// Algorithm is done, just need to create the path by looking at PathPredescessors and reverse it to use.
			if (aStarDone) {
				while (Vector3.Distance(current.Translation, source.Translation) != 0.0) {
					current.Navigatable = NavNode.NavNodeEnum.PATH;
					path.Add(current);
					current = current.PathPredecessor;
				}
				path.Reverse();
				return path;
			} else { Console.WriteLine("No path exists!"); return path; } // If no path is found it will crash the program. (Need to fix)	
		}		

		/// <summary>
		/// This was copied from path.cs with only a little change.
		/// </summary>
		/// <param name="gameTime">Game time.</param>
		public override void Draw(GameTime gameTime) {
			// Need to draw the path nodes
           Matrix[] modelTransforms = new Matrix[stage.WayPoint3D.Bones.Count];
           List<NavNode> navNodes = new List<NavNode>();
			navNodes.AddRange(open);
			navNodes.AddRange(closed);
			navNodes.AddRange(path);

			if (aStarDone) { 
    	  		foreach(NavNode navNode in navNodes) {
       		  	// draw the Path markers - copyed from path.cs draw - By David Kopp
      	    		foreach (ModelMesh mesh in stage.WayPoint3D.Meshes) {
        	      		stage.WayPoint3D.CopyAbsoluteBoneTransformsTo(modelTransforms);
           	       		foreach (BasicEffect effect in mesh.Effects) {
           	            	effect.EnableDefaultLighting();
            	          	if (stage.Fog) {
					     		effect.FogColor = Color.LightSlateGray.ToVector3();   // Changed from CornFlowerBlue
                  	 	        effect.FogStart = stage.FogStart;
                  	 	        effect.FogEnd = stage.FogEnd;
                   		        effect.FogEnabled = true;
                            }
                           	else effect.FogEnabled = false;
                           		effect.DirectionalLight0.DiffuseColor = navNode.NodeColor;
                           		effect.AmbientLightColor = navNode.NodeColor;
                  		   		effect.DirectionalLight0.Direction = stage.LightDirection;
                  		   		effect.DirectionalLight0.Enabled = true;
                  	       		effect.View = stage.View;
                  		   		effect.Projection = stage.Projection;
                  		   		effect.World = Matrix.CreateTranslation(navNode.Translation) * modelTransforms[mesh.ParentBone.Index];
              			}
               			stage.setBlendingState(true);
              	    	mesh.Draw();
                   	 	stage.setBlendingState(false);
			  		}
				}
			}
		}
	}
}