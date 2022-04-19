namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IComponent {
	string Name { get; }
	IEnumerable<IProp> Properties { get; }
}
