using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Memory;

namespace OsuFrameworkDesigner.Game.Persistence;

public class PropsDelta {
	public readonly BindableList<IComponent> TrackedComponents = new();
	Dictionary<IProp, PropDelta> trackedProps = new();
	HashSet<IProp> changedProps = new();

	public PropsDelta () {
		TrackedComponents.BindCollectionChanged( ( _, e ) => {
			if ( e.OldItems != null ) {
				foreach ( IComponent i in e.OldItems ) {
					stopTracking( i );
				}
			}
			if ( e.NewItems != null ) {
				foreach ( IComponent i in e.NewItems ) {
					startTracking( i );
				}
			}
		} );
	}

	void startTracking ( IComponent component ) {
		startTracking( component.Name );
		foreach ( var i in component.Properties ) {
			startTracking( i );
		}
	}

	void stopTracking ( IComponent component ) {
		stopTracking( component.Name );
		foreach ( var i in component.Properties ) {
			stopTracking( i );
		}
	}

	void startTracking ( IProp prop ) {
		trackedProps.Add( prop, new() { InitialValue = prop.Value, FinalValue = prop.Value } );
		prop.IPropValueChanged += onTrackedPropValueChanged;
	}

	void stopTracking ( IProp prop ) {
		trackedProps.Remove( prop );
		changedProps.Remove( prop );
		prop.IPropValueChanged -= onTrackedPropValueChanged;
	}

	void onTrackedPropValueChanged ( IProp prop, ValueChangedEvent<object?> e ) {
		trackedProps[prop] = trackedProps[prop] with { FinalValue = e.NewValue };
		if ( EqualityComparer<object?>.Default.Equals( trackedProps[prop].InitialValue, trackedProps[prop].FinalValue ) ) {
			changedProps.Remove( prop );
		}
		else {
			changedProps.Add( prop );
		}
	}

	public bool AnyChanged
		=> changedProps.Any();

	public void Flush () {
		foreach ( var prop in changedProps ) {
			trackedProps[prop] = trackedProps[prop] with { InitialValue = trackedProps[prop].FinalValue };
		}

		changedProps.Clear();
	}

	public void Flush ( IComponent component ) {
		foreach ( var prop in component.Properties ) {
			if ( trackedProps.TryGetValue( prop, out var delta ) ) {
				trackedProps[prop] = delta with { InitialValue = delta.FinalValue };
				changedProps.Remove( prop );
			}
		}
	}

	public PropsChange CreateChange () {
		var array = MemoryPool<IPropChange>.Shared.Rent( changedProps.Count );
		int i = 0;
		foreach ( var prop in changedProps ) {
			var delta = trackedProps[prop];
			array[i++] = new PropChange { Target = prop, PreviousValue = delta.InitialValue, NextValue = delta.FinalValue };
		}

		return new() { Target = array };
	}

	readonly struct PropDelta {
		public object? InitialValue { get; init; }
		public object? FinalValue { get; init; }
	}
}