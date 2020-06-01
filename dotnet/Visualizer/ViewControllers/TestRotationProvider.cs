using System;
using Lego.Client;
using Lego.Core;
using Lego.Core.Description;
using Maths;

namespace Visualizer.ViewControllers
{
    public class TestRotationProvider : IRotationProvider, ILegoCarStateProvider
    {
        private double _angle;
        private readonly LegoCarState _state = new LegoCarState();

        private double NextAngle()
        {
            _angle += (float) (2 * Math.PI / 1000);
            if (_angle >= 2 * Math.PI)
            {
                _angle = 0;
            }

            return _angle;
        }
        
        public Double3 GetEulerAngles()
        {
            var angle = NextAngle();
            _state.EulerAngles = new Double3(0, angle, 0);
            return _state.EulerAngles;
        }

        public Quatd GetQuaternion()
        {
            var angle = NextAngle();
            
            // Camera on top of car
            var wiggle = Math.PI / 12 * Math.Sin(angle * 12);
            _state.Quaternion = new Quatd(-Math.PI / 2 + wiggle, 0, Math.PI / 2 + angle);
            return _state.Quaternion;
        }

        public LegoCarState GetState() => _state;
        public LegoCarDescriptor GetCarDescriptor() => new LegoCarDescriptor();
    }
}