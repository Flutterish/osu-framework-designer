using osu.Framework;
using OsuFrameworkDesigner.Game;

using ( var host = Host.GetSuitableHost( @"OsuFrameworkDesigner" ) )
using ( var game = new OsuFrameworkDesignerGame() )
	host.Run( game );