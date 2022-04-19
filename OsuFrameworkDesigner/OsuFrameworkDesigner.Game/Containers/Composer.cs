using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Containers;

[Cached]
public class Composer : CompositeDrawable {
	public readonly Bindable<Tool> Tool = new();
	Container<Tool> tools;

	public readonly BindableList<IComponent> Selection = new();

	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.ComposerBackgroundDefault );

	TransformContainer content;
	Container components;
	Container layerAbove;

	public Composer () {
		this.Fill();
		AddInternal( background = new Box().Fill() );
		AddInternal( new DrawSizePreservingFillContainer {
			TargetDrawSize = new Vector2( 500 ),
			Child = content = new TransformContainer().Fit().Center().WithChildren(
				components = new Container().Center(),
				layerAbove = new Container().Center()
			)
		}.Fill() );
		AddInternal( tools = new Container<Tool>().Fill() );

		Selection.BindCollectionChanged( ( _, e ) => {
			if ( e.OldItems != null ) {
				foreach ( IComponent i in e.OldItems ) {
					visibleSelections.Remove( i, out var selection );
					selection!.Free();
					selectionPool.Push( selection );
					layerAbove.Remove( selection );
				}
			}
			if ( e.NewItems != null ) {
				foreach ( IComponent i in e.NewItems ) {
					if ( !selectionPool.TryPop( out var selection ) ) {
						selection = new();
					}
					visibleSelections.Add( i, selection );
					layerAbove.Add( selection );
					selection.Apply( i.AsDrawable() );
				}
			}
		} );
	}

	public IEnumerable<IComponent> Components => components.OfType<IComponent>();
	public IEnumerable<IComponent> ComponentsReverse => components.Children.Reverse().OfType<IComponent>();
	public void Add<T> ( T component ) where T : Drawable, IComponent {
		components.Add( component );
	}
	public void Remove<T> ( T component ) where T : Drawable, IComponent {
		components.Remove( component );
	}

	Stack<DrawableSelection> selectionPool = new();
	Dictionary<IComponent, DrawableSelection> visibleSelections = new();

	new public Vector2 ToLocalSpace ( Vector2 screenSpace )
		=> content.ToLocalSpace( screenSpace ) - content.DrawSize / 2 + content.Position;

	protected override void LoadComplete () {
		base.LoadComplete();
		Tool.BindValueChanged( v => {
			var oldTool = v.OldValue;
			if ( oldTool != null ) {
				oldTool.FadeOut( 200 );
			}

			var tool = v.NewValue;
			if ( tool != null ) {
				if ( tool.Parent == tools ) {
					tool.FadeIn( 200 );
				}
				else {
					tools.Add( tool );
					tool.FadeInFromZero( 200 );
				}
				tools.ChangeChildDepth( tool, (float)-Time.Current );
			}
		}, true );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.ComposerBackground );
		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}
}
