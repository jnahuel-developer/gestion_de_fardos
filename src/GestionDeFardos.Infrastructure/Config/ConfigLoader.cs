using GestionDeFardos.Core.Config;
using GestionDeFardos.Core.Interfaces;

namespace GestionDeFardos.Infrastructure.Config;

public sealed class ConfigLoader : IConfigLoader
{
    public AppSettings Load(string baseDirectory)
    {
        return new AppSettings();
    }
}
