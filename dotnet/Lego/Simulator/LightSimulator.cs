using System;
using Lego.Core;

namespace Lego.Simulator
{
    public class LightSimulator : ILight
    {
        private readonly string _name;
        private bool _on;

        public LightSimulator(string name)
        {
            _name = name;
        }

        public bool On
        {
            get => _on;
            set
            {
                _on = value;
                Console.WriteLine($"{_name} {value.ToOnOff()}");
            }
        }

        public void Toggle()
        {
            On = !On;
        }
    }

    internal static class BoolExtensions
    {
        public static string ToOnOff(this bool value)
        {
            return value ? "on" : "off";
        }
    }
}