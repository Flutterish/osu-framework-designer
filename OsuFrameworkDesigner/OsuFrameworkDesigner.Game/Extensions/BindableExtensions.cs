namespace OsuFrameworkDesigner.Game.Extensions;

public static class BindableExtensions {
	public static T FadeColour<T> ( this T drawable, IBindable<Colour4> newColour, double duration = 100, Easing easing = Easing.Out ) where T : Drawable {
		newColour.BindValueChanged( v => {
			drawable.FadeColour( v.NewValue, duration, easing );
		}, true );
		return drawable;
	}

	/// <summary>
	/// Binds an event to both bindables
	/// </summary>
	/// <returns>An action which will unbind the events</returns>
	public static Action BindValueChanged<T1, T2> ( this (IBindable<T1>, IBindable<T2>) self, Action<T1, T2> action, bool runOnceImmediately = false ) {
		void T1Changed ( ValueChangedEvent<T1> _ ) {
			action( self.Item1.Value, self.Item2.Value );
		}
		void T2Changed ( ValueChangedEvent<T2> _ ) {
			action( self.Item1.Value, self.Item2.Value );
		}

		self.Item1.ValueChanged += T1Changed;
		self.Item2.ValueChanged += T2Changed;

		if ( runOnceImmediately )
			action( self.Item1.Value, self.Item2.Value );

		return () => {
			self.Item1.ValueChanged -= T1Changed;
			self.Item2.ValueChanged -= T2Changed;
		};
	}
}
