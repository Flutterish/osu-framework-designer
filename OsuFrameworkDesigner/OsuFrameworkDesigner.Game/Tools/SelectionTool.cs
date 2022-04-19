using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;

namespace OsuFrameworkDesigner.Game.Tools;

public class SelectionTool : Tool {
	SelectionBox? selection;
	protected override bool OnDragStart ( DragStartEvent e ) {
		AddInternal( selection = new() );
		selection.Position = e.MousePosition;

		return true;
	}

	protected override void OnDrag ( DragEvent e ) {
		selection!.Size = e.MousePosition - selection.Position;

		var topLeft = ToScreenSpace( selection.Position );
		var bottomRight = ToScreenSpace( selection.Position + selection.Size );
		var selectionQuad = new Quad( topLeft, new( bottomRight.X, topLeft.Y ), new( topLeft.X, bottomRight.Y ), bottomRight );

		Composer.Selection.Clear();
		Composer.Selection.AddRange( Composer.Components.Where( x => x.AsDrawable().ScreenSpaceDrawQuad.Intersects( selectionQuad ) ) );
	}

	protected override void OnDragEnd ( DragEndEvent e ) {
		selection.FadeOut( 500 ).Expire();
		selection = null;
	}

	protected override bool OnClick ( ClickEvent e ) {
		Composer.Selection.Clear();
		var item = Composer.ComponentsReverse.FirstOrDefault( x => x.AsDrawable().ScreenSpaceDrawQuad.Contains( e.ScreenSpaceMouseDownPosition ) );
		if ( item != null )
			Composer.Selection.Add( item );

		return true;
	}
}

public class SelectionBox : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionDefault );

	public SelectionBox () {
		AddInternal( background = new Box { Alpha = 0.2f }.Fill() );
		Masking = true;
		BorderThickness = 4;
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.Selection );
		background.FadeColour( backgroundColor );
		backgroundColor.BindValueChanged( v => BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}