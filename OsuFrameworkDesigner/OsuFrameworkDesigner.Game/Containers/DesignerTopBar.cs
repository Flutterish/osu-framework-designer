

using osu.Framework.Graphics.Sprites;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Containers;

public class DesignerTopBar : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.TopbarDefault );

	FillFlowContainer toolSelection;

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
						new ToolButton { Icon = FontAwesome.Regular.QuestionCircle },
						new ToolButton { Icon = FontAwesome.Solid.MousePointer },
						new ToolButton { Icon = FontAwesome.Solid.Crop },
						new ToolButton { Icon = FontAwesome.Regular.Square },
						new ToolButton { Icon = FontAwesome.Solid.PenNib },
						new ToolButton { Icon = FontAwesome.Solid.TextHeight },
						new ToolButton { Icon = FontAwesome.Solid.HandPointer },
						new ToolButton { Icon = FontAwesome.Regular.Comment }
					).WithEachChild<FillFlowContainer, ToolButton>( (child, children) => {
						child.Selected.BindValueChanged( v => {
							if ( v.NewValue ) foreach ( var i in children )
								i.Selected.Value = i == child;
						} );
					} ),
					new DesignerSpriteText {
						Text = "Drafts / Untitled",
						Font = new FontUsage(size: 24)
					}.Center(),
					null!
				}
			}
		}.Fill() );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.Topbar );

		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}
}
