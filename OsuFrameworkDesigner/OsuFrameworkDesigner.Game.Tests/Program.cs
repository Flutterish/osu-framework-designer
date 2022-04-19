using osu.Framework;
using OsuFrameworkDesigner.Game.Tests;

using ( var host = Host.GetSuitableHost( "visual-tests" ) )
using ( var game = new OsuFrameworkDesignerTestBrowser() )
	host.Run( game );