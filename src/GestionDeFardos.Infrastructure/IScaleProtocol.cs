using GestionDeFardos.Core.Config;

namespace GestionDeFardos.Infrastructure;

internal interface IScaleProtocol
{
    string Id { get; }
    ScaleFrameReadResult TryReadFrame(List<byte> buffer, ScaleSettings settings);
}
