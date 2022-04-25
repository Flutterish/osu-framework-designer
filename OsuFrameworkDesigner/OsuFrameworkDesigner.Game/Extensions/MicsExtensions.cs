using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Extensions;

public static class MicsExtensions {
	// TODO remove this assumption
	public static Drawable AsDrawable ( this IComponent component )
		=> (Drawable)component;
}
