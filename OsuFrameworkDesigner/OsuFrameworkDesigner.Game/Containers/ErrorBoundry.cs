namespace OsuFrameworkDesigner.Game.Containers;

public class ErrorBoundry : ErrorBoundry<Drawable> { }
public class ErrorBoundry<T> : Container<T> where T : Drawable {
	public double LastErrorTimestamp { get; private set; }

	public override bool UpdateSubTree () {
		if ( LastErrorTimestamp + 50 > Time.Current )
			return true;

		try {
			return base.UpdateSubTree();
		}
		catch ( Exception e ) {
			LastErrorTimestamp = Time.Current;
			Error?.Invoke( e );
			return true;
		}
	}

	public event Action<Exception>? Error;
}
