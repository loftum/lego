namespace SteeringWheel.Controllers
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
    }
}

