using osu.Framework.Graphics.Cursor;
using osu.Framework.Screens;
using OsuFrameworkDesigner.Game.Containers;
using OsuFrameworkDesigner.Game.Cursor;

namespace OsuFrameworkDesigner.Game;

public class DesignerScreen : Screen {
	DesignerTopBar topBar;
	Composer composer;
	AssetsPanel assetsPanel;
	PropertiesPanel propertiesPanel;
	DesignerCursorContainer cursorContainer;

	public DesignerScreen () {
		composer = new Composer();
		cursorContainer = new DesignerCursorContainer().Fill();

		AddInternal( cursorContainer.WithChild( new TooltipContainer( cursorContainer.CursorContainer ) {
			new GridContainer {
				RowDimensions = new Dimension[] {
					new( GridSizeMode.AutoSize ),
					new()
				},
				Content = new Drawable[][] {
					new Drawable[] { topBar = new DesignerTopBar( composer ) },
					new Drawable[] { new GridContainer {
						ColumnDimensions = new Dimension[] {
							new( GridSizeMode.AutoSize ),
							new(),
							new( GridSizeMode.AutoSize )
						},
						Content = new Drawable[][] {
							new Drawable[] {
								assetsPanel = new AssetsPanel(),
								composer,
								propertiesPanel = new PropertiesPanel()
							}
						}
					}.Fill() }
				}
			}.Fill()
		}.Fill() ) );

		composer.Tool.BindTo( topBar.Tool );
		propertiesPanel.Components.BindTo( composer.SelectionTool.Selection );
	}
}