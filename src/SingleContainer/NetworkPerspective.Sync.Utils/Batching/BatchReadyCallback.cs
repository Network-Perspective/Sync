using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Utils.Batching;

public delegate Task BatchReadyCallback<T>(BatchReadyEventArgs<T> args);