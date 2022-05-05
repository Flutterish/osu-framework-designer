using osu.Framework.Graphics.Cursor;
using osu.Framework.Screens;
using OsuFrameworkDesigner.Game.Containers;
using OsuFrameworkDesigner.Game.Containers.Assets;
using OsuFrameworkDesigner.Game.Containers.Properties;
using OsuFrameworkDesigner.Game.Containers.Timeline;
using OsuFrameworkDesigner.Game.Cursor;

namespace OsuFrameworkDesigner.Game;

public class DesignerScreen : Screen {
	DesignerTopBar topBar;
	Composer composer;
	AssetsPanel assetsTab;
	PropertiesTab propertiesTab;
	HistoryTab historyTab;
	DesignerCursorContainer cursorContainer;

	public DesignerScreen () {
		composer = new Composer();
		cursorContainer = new DesignerCursorContainer().Fill();

		Panel rightPanel;
		AddInternal( cursorContainer.WithChild( new TooltipContainer( cursorContainer.CursorContainer ) {
			new DesignerPopoverContainer {
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
									assetsTab = new AssetsPanel(),
									composer,
									rightPanel = new Panel()
								}
							}
						}.Fill() }
					}
				}.Fill()
			}.Fill()
		}.Fill() ) );

		rightPanel.Add( "Properties", propertiesTab = new PropertiesTab() );
		rightPanel.Add( "Timeline", historyTab = new HistoryTab( composer.History ) );

		composer.Tool.BindTo( topBar.Tool );
		propertiesTab.Components.BindTo( composer.SelectionTool.Selection );
		composer.ComponentAdded += assetsTab.AddComponent;
		composer.ComponentRemoved += assetsTab.RemoveComponent;
		assetsTab.Selection.BindTo( composer.SelectionTool.Selection );
		assetsTab.SelectionChanged += s => topBar.Tool.Value = composer.SelectionTool;
	}

	protected override void Update () {
		base.Update();
		composer.SaveProps = !topBar.Tool.Value.IsEditingProps;
	}
}