using osu.Framework.Graphics.UserInterface;
using OsuFrameworkDesigner.Game.Containers.Properties;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Containers;

public abstract class TextEditField<T> : EditField<T> {
	DesignerSpriteText title;
	BasicTextBox textBox;
	
	public string Title {
		set => title.Text = value.ToString()[0..1];
	}

	public TextEditField () {
		RelativeSizeAxes = Axes.X;
		Height = 40;
		Width = 0.5f;

		AddInternal( new FillFlowContainer().FilledHorizontal().WithChildren(
			title = new DesignerSpriteText {
				Colour = Colour4.Black,
				Alpha = 0.5f,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				Font = DesignerFont.Monospace( 20 ),
				Width = 16
			},
			textBox = new BasicTextBox {
				Height = 30,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft
			}
		) );

		textBox.CommitOnFocusLost = true;
		textBox.OnCommit += ( _, _ ) => {
			if ( !TryParse( textBox.Current.Value, out var value ) ) {
				InvalidateDisplay();
				return;
			}

			SetValue( value );
		};
	}

	protected override void UpdateDisplay () {
		var v = Props[0].Value;
		if ( Props.All( x => AreValuesEqual( x.Value, v ) ) )
			textBox.Text = $"{v:0.##}";
		else
			textBox.Text = "Mixed";
	}

	protected override void Update () {
		base.Update();

		textBox.Width = ChildSize.X - title.DrawWidth - 10;
	}

	protected abstract bool TryParse ( string s, out T value );
	protected virtual string Format ( T value )
		=> $"{value}";
	protected virtual bool AreValuesEqual ( T a, T b )
		=> EqualityComparer<object>.Default.Equals( a, b );
}

public class FloatEditField : TextEditField<float> {
	protected override bool TryParse ( string s, out float value )
		=> float.TryParse( s, out value );

	protected override string Format ( float value )
		=> $"{value:0.##}";
}

public class IntEditField : TextEditField<int> {
	protected override bool TryParse ( string s, out int value )
		=> int.TryParse( s, out value );
}