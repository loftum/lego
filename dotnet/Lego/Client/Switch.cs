namespace Lego.Car
{
    public class Switch
    {
        public bool IsOn { get; set; }
        public bool WasOn { get; set; }
        public bool HasChanged() => IsOn != WasOn;
        public void UpdateWasOn()
        {
            WasOn = IsOn;
        }

        public void Toggle()
        {
            IsOn = !IsOn;
        }
    }
}

