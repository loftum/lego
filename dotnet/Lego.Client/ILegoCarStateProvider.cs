using Lego.Core;

namespace Lego.Client
{
    public interface ILegoCarStateProvider
    {
        LegoCarState GetState();
    }
}