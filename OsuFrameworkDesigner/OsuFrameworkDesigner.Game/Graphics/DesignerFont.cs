using osu.Framework.Graphics.Sprites;

namespace OsuFrameworkDesigner.Game.Graphics;

public static class DesignerFont {
	public static FontUsage Default ( float size = 24 ) => new( size: size );

	public static FontUsage Bold ( float size = 24 ) => new( family: "Roboto-Bold", size );
	public static FontUsage Monospace ( float size = 24 ) => new( family: "RobotoCondensed-Regular", size );
}
