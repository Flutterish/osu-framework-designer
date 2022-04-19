using osu.Framework.Graphics.Sprites;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Containers;

public class DesignerTopBar : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.TopbarDefault );

	FillFlowContainer toolSelection;
	Bindable<ToolButton> selected = new();
	public readonly Bindable<Tool> Tool = new();

	DesignerTextFlowContainer title;

	public DesignerTopBar () {
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
						new ToolButton( () => new SelectionTool() ) { Icon = FontAwesome.Solid.MousePointer },
						new ToolButton( () => new RectangleTool() ) { Icon = FontAwesome.Regular.Square }
					).WithEachChild<FillFlowContainer, ToolButton>( (child, children) => {
						child.Selected.BindValueChanged( v => {
							if ( v.NewValue ) selected.Value = child;
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

		selected.BindValueChanged( v => {
			Tool.Value = v.NewValue.Tool;
			foreach ( ToolButton i in toolSelection )
				i.Selected.Value = i == v.NewValue;
		}, true );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.Topbar );
		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}
}
