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
			foreach ( var category in Components.SelectMany( c => c.GetNestedProperties() ).GroupBy( x => x.Category ) ) {
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
			return new FloatEditField( props.First().Name, props.OfType<IProp<float>>() );
		}
		else if ( type == typeof( int ) ) {
			return new IntEditField( props.First().Name, props.OfType<IProp<int>>() );
		}

		throw new InvalidOperationException( $"No edit field for type {type.ReadableName()} exists." );
	}
}

public abstract class TextEditField<T> : FillFlowContainer {
	DesignerSpriteText title;
	BasicTextBox textBox;
	IEnumerable<IProp<T>> props;

	public TextEditField ( string title, IEnumerable<IProp<T>> props ) {
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
		foreach ( var i in props ) {
			i.ValueChanged += onValueChanged;
		}

		textBox.CommitOnFocusLost = true;
		textBox.OnCommit += ( _, _ ) => {
			if ( !TryParse( textBox.Current.Value, out var value ) ) {
				updateValue();
				return;
			}

			ignoreUpdates = true;
			foreach ( var i in props ) {
				i.Value = value;
			}
			ignoreUpdates = false;

			updateValue();
		};
	}

	bool ignoreUpdates;
	private void onValueChanged ( ValueChangedEvent<T> e ) {
		if ( !ignoreUpdates ) updateValue();
	}

	void updateValue () {
		var v = props.First().Value;
		if ( props.All( x => AreValuesEqual( x.Value, v ) ) )
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
		foreach ( IBindable<T> i in props ) {
			i.ValueChanged -= onValueChanged;
		}
	}

	protected abstract bool TryParse ( string s, out T value );
	protected virtual string Format ( T value )
		=> $"{value}";
	protected virtual bool AreValuesEqual ( T a, T b )
		=> EqualityComparer<object>.Default.Equals( a, b );
}

public class FloatEditField : TextEditField<float> {
	public FloatEditField ( string title, IEnumerable<IProp<float>> props ) : base( title, props ) { }

	protected override bool TryParse ( string s, out float value )
		=> float.TryParse( s, out value );

	protected override string Format ( float value )
		=> $"{value:0.##}";
}

public class IntEditField : TextEditField<int> {
	public IntEditField ( string title, IEnumerable<IProp<int>> props ) : base( title, props ) { }

	protected override bool TryParse ( string s, out int value )
		=> int.TryParse( s, out value );
}