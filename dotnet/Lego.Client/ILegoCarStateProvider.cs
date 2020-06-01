using Lego.Core;
using Lego.Core.Description;

namespace Lego.Client
{
    public interface ILegoCarStateProvider
    {
        LegoCarState GetState();
        LegoCarDescriptor GetCarDescriptor();
    }
}