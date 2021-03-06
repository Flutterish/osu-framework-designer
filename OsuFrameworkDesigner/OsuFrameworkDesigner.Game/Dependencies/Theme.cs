namespace OsuFrameworkDesigner.Game.Dependencies;

public class Theme {
	public static Colour4 TopbarDefault { get; } = Colour4.FromHex( "#2C2C2C" );
	public readonly Bindable<Colour4> Topbar = new( TopbarDefault );
	public static Colour4 TopbarButtonDefault { get; } = TopbarDefault;
	public readonly Bindable<Colour4> TopbarButton = new( TopbarButtonDefault );
	public static Colour4 TopbarButtonHoverDefault { get; } = Colour4.Black;
	public readonly Bindable<Colour4> TopbarButtonHover = new( TopbarButtonHoverDefault );
	public static Colour4 TopbarButtonActiveDefault { get; } = Colour4.FromHex( "#0D99FF" );
	public readonly Bindable<Colour4> TopbarButtonActive = new( TopbarButtonActiveDefault );
	public static Colour4 TopbarButtonIconDefault { get; } = Colour4.White;
	public readonly Bindable<Colour4> TopbarButtonIcon = new( TopbarButtonIconDefault );

	public static Colour4 ComposerBackgroundDefault { get; } = Colour4.FromHex( "#E5E5E5" );
	public readonly Bindable<Colour4> ComposerBackground = new( ComposerBackgroundDefault );
	public static Colour4 SidePanelDefault { get; } = Colour4.White;
	public readonly Bindable<Colour4> SidePanel = new( SidePanelDefault );

	public static Colour4 SelectionDefault { get; } = Colour4.FromHex( "#0D99FF" );
	public readonly Bindable<Colour4> Selection = new( SelectionDefault );
	public static Colour4 SelectionHandleDefault { get; } = Colour4.FromHex( "#FCFCFC" );
	public readonly Bindable<Colour4> SelectionHandle = new( SelectionHandleDefault );

	public static Colour4 SnapMarkerDefault { get; } = Colour4.FromHex( "#F34922" );
	public readonly Bindable<Colour4> SnapMarker = new( SnapMarkerDefault );
}
