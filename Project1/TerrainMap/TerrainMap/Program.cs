/* Programmer: David Kopp
*  Email: david.kopp.330@my.csun.edu
*  Class: Comp565
*  Project#: 1
*  Description: Terrain builder by using Brownian Motion.
*/


using System;

namespace TerrainMap
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new TerrainMap())
                game.Run();
        }
    }
#endif
}
