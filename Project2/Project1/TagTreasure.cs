

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace AGMGSKv7
{
	public class TagTreasure : Model3D
	{
		public TagTreasure (Stage stage, String label, String meshFile) : base(stage, label, meshFile) { }

		public void addTag(Vector3 tagLocation) {
			addObject (tagLocation, Vector3.Up, 0.0f);
		}
	}
}