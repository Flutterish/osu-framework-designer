using osu.Framework.Screens;
using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game;

public class DesignerScreen : Screen {
	DesignerTopBar topBar;
	Composer composer;
	AssetsPanel assetsPanel;
	PropertiesPanel propertiesPanel;

	public DesignerScreen () {
		AddInternal( new GridContainer {
			RowDimensions = new Dimension[] {
				new( GridSizeMode.AutoSize ),
				new()
			},
			Content = new Drawable[][] {
				new Drawable[] { topBar = new DesignerTopBar() },
				new Drawable[] { new GridContainer {
					ColumnDimensions = new Dimension[] {
						new( GridSizeMode.AutoSize ),
						new(),
						new( GridSizeMode.AutoSize )
					},
					Content = new Drawable[][] {
						new Drawable[] {
							assetsPanel = new AssetsPanel(),
							composer = new Composer(),
							propertiesPanel = new PropertiesPanel()
						}
					}
				}.Fill() }
			}
		}.Fill() );

		composer.Tool.BindTo( topBar.Tool );
	}
}