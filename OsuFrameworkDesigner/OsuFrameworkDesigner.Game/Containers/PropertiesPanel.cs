using osu.Framework.Caching;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.UserInterface;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Containers;

public class PropertiesPanel : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SidePanelDefault );
	FillFlowContainer items;

	public PropertiesPanel () {
		RelativeSizeAxes = Axes.Y;
		Width = 300;
		AddInternal( background = new Box().Fill() );
		AddInternal( items = new FillFlowContainer {
			Direction = FillDirection.Full,
			AutoSizeAxes = Axes.Y,
			RelativeSizeAxes = Axes.X,
			Padding = new( 10 )
		} );

		Components.BindCollectionChanged( ( _, _ ) => componentCache.Invalidate() );
	}

	public readonly BindableList<IComponent> Components = new();

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.SidePanel );
		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}

	Cached componentCache = new();
	protected override void Update () {
		base.Update();

		if ( !componentCache.IsValid ) {
			componentCache.Validate();

			items.Clear();
			foreach ( var category in Components.SelectMany( Extensions.GetNestedProperties ).GroupBy( x => x.Category ) ) {
				items.Add( new DesignerSpriteText { Text = category.Key, Font = DesignerFont.Bold( 18 ), Colour = Colour4.Black, Alpha = 0.5f, RelativeSizeAxes = Axes.X } );

				foreach ( var prop in category.GroupBy( x => (x.Name, x.Type) ) ) {
					var ungroupable = prop.Where( x => !x.Groupable );
					if ( ungroupable.Any() ) {
						items.Add( createEditField( prop.Key.Type, ungroupable ) );
					}

					foreach ( var i in prop.Where( x => x.Groupable ).GroupBy( x => x.Value ) ) {
						items.Add( createEditField( prop.Key.Type, i ) );
					}
				}
			}
		}
	}

	Drawable createEditField ( Type type, IEnumerable<IProp> props ) {
		if ( type == typeof( float ) ) {
			return new FloatEditField( props.First().Name, props );
		}

		throw new InvalidOperationException( $"No edit field for type {type.ReadableName()} exists." );
	}
}

public class FloatEditField : FillFlowContainer {
	DesignerSpriteText title;
	BasicTextBox textBox;
	IEnumerable<IProp> props;

	public FloatEditField ( string title, IEnumerable<IProp> props ) {
		RelativeSizeAxes = Axes.X;
		Height = 40;
		Width = 0.5f;
		Direction = FillDirection.Horizontal;
		Add( this.title = new DesignerSpriteText {
			Text = title[0..1],
			Colour = Colour4.Black,
			Alpha = 0.5f,
			Anchor = Anchor.CentreLeft,
			Origin = Anchor.CentreLeft,
			Font = DesignerFont.Monospace( 20 ),
			Width = 16
		} );
		Add( textBox = new BasicTextBox {
			Height = 30,
			Anchor = Anchor.CentreLeft,
			Origin = Anchor.CentreLeft
		} );

		this.props = props;
		updateValue();
		foreach ( Prop<float> i in props ) {
			i.ValueChanged += onValueChanged;
		}

		textBox.CommitOnFocusLost = true;
		textBox.OnCommit += ( _, _ ) => {
			if ( !float.TryParse( textBox.Current.Value, out var value ) ) {
				updateValue();
				return;
			}

			ignoreUpdates = true;
			foreach ( Prop<float> i in props ) {
				i.Value = value;
			}
			ignoreUpdates = false;
		};
	}

	bool ignoreUpdates;
	private void onValueChanged ( ValueChangedEvent<float> e ) {
		if ( !ignoreUpdates ) updateValue();
	}

	void updateValue () {
		var v = props.First().Value;
		if ( props.All( x => EqualityComparer<object>.Default.Equals( x.Value, v ) ) )
			textBox.Text = $"{v:0.##}";
		else
			textBox.Text = "Mixed";
	}

	protected override void Update () {
		base.Update();

		textBox.Width = ChildSize.X - title.DrawWidth - 10;
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );
		foreach ( IBindable<float> i in props ) {
			i.ValueChanged -= onValueChanged;
		}
	}
}