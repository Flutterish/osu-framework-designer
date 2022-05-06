using osu.Framework.Graphics.UserInterface;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Containers.Properties;

public class Vector2EditField : EditField<Vector2> {
	DesignerSpriteText title1;
	BasicTextBox textBox1;
	DesignerSpriteText title2;
	BasicTextBox textBox2;

	public string Title {
		set { }
	}

	public Vector2EditField () {
		RelativeSizeAxes = Axes.X;
		Height = 40;

		AddInternal( new FillFlowContainer {
			Direction = FillDirection.Horizontal,
			RelativeSizeAxes = Axes.Both,
			Children = new Drawable[] {
				title1 = new DesignerSpriteText {
					Colour = Colour4.Black,
					Alpha = 0.5f,
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					Font = DesignerFont.Monospace( 20 ),
					Width = 16,
					Text = "X"
				},
				textBox1 = new BasicTextBox {
					Height = 30,
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					Margin = new() { Right = 10 }
				},
				title2 = new DesignerSpriteText {
					Colour = Colour4.Black,
					Alpha = 0.5f,
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					Font = DesignerFont.Monospace( 20 ),
					Width = 16,
					Text = "Y"
				},
				textBox2 = new BasicTextBox {
					Height = 30,
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft
				}
			}
		} );

		textBox1.CommitOnFocusLost = true;
		textBox1.OnCommit += ( _, _ ) => {
			InvalidateDisplay();
			if ( !float.TryParse( textBox1.Current.Value, out var value ) )
				return;

			foreach ( var i in Props ) {
				i.Value = i.Value with { X = value };
			}
		};
		textBox2.CommitOnFocusLost = true;
		textBox2.OnCommit += ( _, _ ) => {
			InvalidateDisplay();
			if ( !float.TryParse( textBox2.Current.Value, out var value ) )
				return;

			foreach ( var i in Props ) {
				i.Value = i.Value with { Y = value };
			}
		};
	}

	protected override void Update () {
		base.Update();

		textBox1.Width = ChildSize.X / 2 - title1.DrawWidth - 10;
		textBox2.Width = ChildSize.X / 2 - title2.DrawWidth - 10;
	}

	protected override void UpdateDisplay () {
		var v = Props[0].Value;
		if ( Props.All( x => x.Value.X == v.X ) )
			textBox1.Text = $"{v.X:0.##}";
		else
			textBox1.Text = "Mixed";

		if ( Props.All( x => x.Value.Y == v.Y ) )
			textBox2.Text = $"{v.Y:0.##}";
		else
			textBox2.Text = "Mixed";
	}
}
