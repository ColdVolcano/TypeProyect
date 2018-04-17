using osu.Framework;
using osu.Framework.Platform;
using System;

namespace TypeProyect
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (Game game = new TypeProyect())
            using (GameHost host = Host.GetSuitableHost(@"sample-game"))
                host.Run(game);
        }
    }
}