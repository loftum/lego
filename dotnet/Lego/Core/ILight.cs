namespace Lego.Core
{
    public interface ILight
    {
        bool On { get; set; }
        void Toggle();
    }
}