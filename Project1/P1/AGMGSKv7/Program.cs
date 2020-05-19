/* Programmer: David Kopp
*  Email: david.kopp.330@my.csun.edu
*  Class: Comp565
*  Project#: 1
*  Description: AGMGSKv7 is a kit made by Professor Mike Barnes. I added a better train following, Treasure Hunt Game, and NPAgent Treasure Finding and Tagging Mode.
*/

using System;

namespace AGMGSKv7
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
            using (var game = new Stage())
                game.Run();
        }
    }
#endif
}
