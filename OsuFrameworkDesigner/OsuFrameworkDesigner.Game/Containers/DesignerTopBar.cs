using osu.Framework.Graphics.Sprites;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Containers;

public class DesignerTopBar : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.TopbarDefault );

	FillFlowContainer toolSelection;
	public readonly Bindable<Tool> Tool = new();

	DesignerTextFlowContainer title;

	public DesignerTopBar ( Composer composer ) {
		RelativeSizeAxes = Axes.X;
		Height = 60;

		AddInternal( background = new Box().Fill() );
		AddInternal( new GridContainer {
			ColumnDimensions = new Dimension[] {
				new(),
				new( GridSizeMode.AutoSize ),
				new()
			},
			Content = new Drawable[][] {
				new Drawable[] {
					toolSelection = new FillFlowContainer().FilledHorizontal().WithChildren(
						new ToolButton( composer.SelectionTool ) { Icon = FontAwesome.Solid.MousePointer },
						new ToolButton( composer.RectangleTool ) { Icon = FontAwesome.Regular.Square },
						new ToolButton( new CircleTool() ) { Icon = FontAwesome.Regular.Circle },
						new ToolButton( new LineTool() ) { Icon = FontAwesome.Solid.Minus },
						new ToolButton( new ArrowTool() ) { Icon = FontAwesome.Solid.ArrowRight }
					).WithEachChild<FillFlowContainer, ToolButton>( (child, children) => {
						child.Selected.BindValueChanged( v => {
							if ( v.NewValue ) Tool.Value = child.Tool;
						} );
					} ).With( x => x.Children.OfType<ToolButton>().First().Selected.Value = true ),
					title = new DesignerTextFlowContainer {
						TextAnchor = Anchor.Centre
					}.FillY().Center(),
					null!
				}
			}
		}.Fill() );

		title.AddText( "Drafts / ", s => s.Alpha = 0.5f );
		title.AddText( "Untitled" );

		Tool.BindValueChanged( v => {
			foreach ( ToolButton i in toolSelection )
				i.Selected.Value = i.Tool == v.NewValue;
		}, true );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.Topbar );
		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}
}
