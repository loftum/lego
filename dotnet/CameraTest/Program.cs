using System;
using Unosquare.RaspberryIO.Camera;

namespace CameraTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            using (var controller = CameraController.Instance)
            {
                Console.WriteLine("Nothing much here yet.");
            }
        }
    }
}
