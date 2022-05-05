using osu.Framework.Graphics.UserInterface;

namespace OsuFrameworkDesigner.Game.Containers;

public class Panel : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( Theme.SidePanelDefault );
	protected TabControl<string> TabControl { get; private set; }
	protected Container Content { get; private set; }

	public Panel () {
		RelativeSizeAxes = Axes.Y;
		Width = 300;
		AddInternal( background = new Box().Fill() );
		AddInternal( new BasicScrollContainer {
			Child = new FillFlowContainer {
				Direction = FillDirection.Vertical,
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Padding = new( 10 ),
				Spacing = new( 5 )
			}.WithChildren(
				TabControl = new BasicTabControl<string> {
					RelativeSizeAxes = Axes.X,
					Height = 20,
					Colour = Color4.Gray
				},
				new Box { RelativeSizeAxes = Axes.X, Height = 2, Colour = Color4.Gray },
				Content = new Container { RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y }
			)
		}.Fill() );

		TabControl.Current.ValueChanged += e => {
			Content.Clear( disposeChildren: false );
			Content.Add( tabs[e.NewValue] );
		};
	}

	Dictionary<string, Drawable> tabs = new();
	public void Add ( string title, Drawable content ) {
		tabs.Add( title, content );
		TabControl.AddItem( title );
	}

	[BackgroundDependencyLoader]
	private void load ( Theme colours ) {
		backgroundColor.BindTo( colours.SidePanel );
		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}
}