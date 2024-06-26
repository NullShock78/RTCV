//Most of this code is lifted from Bizhawk
//https://github.com/tasvideos/bizhawk
//Thanks guys

namespace RTCV.UI.Input
{
    using System;
    using System.Collections.Generic;
    using RTCV.Common;
    using SlimDX;
    using SlimDX.DirectInput;

    public class GamePad
    {
        // ********************************** Static interface **********************************

        private static readonly object _syncObj = new object();
        private static readonly List<GamePad> _devices = new List<GamePad>();
        private static DirectInput _dinput;

        public static void Initialize()
        {
            lock (_syncObj)
            {
                Cleanup();

                _dinput = new DirectInput();

                foreach (DeviceInstance device in _dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
                {
                    Console.WriteLine("joydevice: {0} `{1}`", device.InstanceGuid, device.ProductName);

                    if (device.ProductName.Contains("XBOX 360"))
                    {
                        continue; // Don't input XBOX 360 controllers into here; we'll process them via XInput (there are limitations in some trigger axes when xbox pads go over xinput)
                    }

                    var joystick = new Joystick(_dinput, device.InstanceGuid);
                    joystick.SetCooperativeLevel(S.GET<CoreForm>().Handle, CooperativeLevel.Background | CooperativeLevel.Nonexclusive);
                    foreach (DeviceObjectInstance deviceObject in joystick.GetObjects())
                    {
                        if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                        {
                            joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);
                        }
                    }
                    joystick.Acquire();

                    GamePad p = new GamePad(device.InstanceName, device.InstanceGuid, joystick, _devices.Count);
                    _devices.Add(p);
                }
            }
        }

        public static IEnumerable<GamePad> EnumerateDevices()
        {
            lock (_syncObj)
            {
                foreach (var device in _devices)
                {
                    yield return device;
                }
            }
        }

        public static void UpdateAll()
        {
            lock (_syncObj)
            {
                foreach (var device in _devices)
                {
                    device.Update();
                }
            }
        }

        public static void Cleanup()
        {
            lock (_syncObj)
            {
                foreach (var device in _devices)
                {
                    device.joystick.Dispose();
                }
                _devices.Clear();

                if (_dinput != null)
                {
                    _dinput.Dispose();
                    _dinput = null;
                }
            }
        }

        // ********************************** Instance Members **********************************

        readonly string name;
        readonly Guid guid;
        readonly Joystick joystick;
        JoystickState state = new JoystickState();

        GamePad(string name, Guid guid, Joystick joystick, int index)
        {
            this.name = name;
            this.guid = guid;
            this.joystick = joystick;
            PlayerNumber = index + 1;
            Update();
            InitializeCallbacks();
        }

        public void Update()
        {
            try
            {
                if (joystick.Acquire().IsFailure)
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            if (joystick.Poll().IsFailure)
            {
                return;
            }

            state = joystick.GetCurrentState();
            if (Result.Last.IsFailure)
            {
                // do something?
                return;
            }
        }

        public IEnumerable<Tuple<string, float>> GetFloats()
        {
            var pis = typeof(JoystickState).GetProperties();
            foreach (var pi in pis)
            {
                yield return new Tuple<string, float>(pi.Name, 10.0f * (float)(int)pi.GetValue(state, null));
            }
        }

        /// <summary>FOR DEBUGGING ONLY</summary>
        public JoystickState GetInternalState()
        {
            return state;
        }

        public string Name => name;
        public int PlayerNumber { get; private set; }

        public string ButtonName(int index)
        {
            return names[index];
        }

        public bool Pressed(int index)
        {
            return actions[index]();
        }

        public int NumButtons { get; private set; }

        private readonly List<string> names = new List<string>();
        private readonly List<Func<bool>> actions = new List<Func<bool>>();

        void AddItem(string _name, Func<bool> callback)
        {
            names.Add(_name);
            actions.Add(callback);
            NumButtons++;
        }

        void InitializeCallbacks()
        {
            const int dzp = 400;
            const int dzn = -400;

            names.Clear();
            actions.Clear();
            NumButtons = 0;

            AddItem("AccelerationX+", () => state.AccelerationX >= dzp);
            AddItem("AccelerationX-", () => state.AccelerationX <= dzn);
            AddItem("AccelerationY+", () => state.AccelerationY >= dzp);
            AddItem("AccelerationY-", () => state.AccelerationY <= dzn);
            AddItem("AccelerationZ+", () => state.AccelerationZ >= dzp);
            AddItem("AccelerationZ-", () => state.AccelerationZ <= dzn);
            AddItem("AngularAccelerationX+", () => state.AngularAccelerationX >= dzp);
            AddItem("AngularAccelerationX-", () => state.AngularAccelerationX <= dzn);
            AddItem("AngularAccelerationY+", () => state.AngularAccelerationY >= dzp);
            AddItem("AngularAccelerationY-", () => state.AngularAccelerationY <= dzn);
            AddItem("AngularAccelerationZ+", () => state.AngularAccelerationZ >= dzp);
            AddItem("AngularAccelerationZ-", () => state.AngularAccelerationZ <= dzn);
            AddItem("AngularVelocityX+", () => state.AngularVelocityX >= dzp);
            AddItem("AngularVelocityX-", () => state.AngularVelocityX <= dzn);
            AddItem("AngularVelocityY+", () => state.AngularVelocityY >= dzp);
            AddItem("AngularVelocityY-", () => state.AngularVelocityY <= dzn);
            AddItem("AngularVelocityZ+", () => state.AngularVelocityZ >= dzp);
            AddItem("AngularVelocityZ-", () => state.AngularVelocityZ <= dzn);
            AddItem("ForceX+", () => state.ForceX >= dzp);
            AddItem("ForceX-", () => state.ForceX <= dzn);
            AddItem("ForceY+", () => state.ForceY >= dzp);
            AddItem("ForceY-", () => state.ForceY <= dzn);
            AddItem("ForceZ+", () => state.ForceZ >= dzp);
            AddItem("ForceZ-", () => state.ForceZ <= dzn);
            AddItem("RotationX+", () => state.RotationX >= dzp);
            AddItem("RotationX-", () => state.RotationX <= dzn);
            AddItem("RotationY+", () => state.RotationY >= dzp);
            AddItem("RotationY-", () => state.RotationY <= dzn);
            AddItem("RotationZ+", () => state.RotationZ >= dzp);
            AddItem("RotationZ-", () => state.RotationZ <= dzn);
            AddItem("TorqueX+", () => state.TorqueX >= dzp);
            AddItem("TorqueX-", () => state.TorqueX <= dzn);
            AddItem("TorqueY+", () => state.TorqueY >= dzp);
            AddItem("TorqueY-", () => state.TorqueY <= dzn);
            AddItem("TorqueZ+", () => state.TorqueZ >= dzp);
            AddItem("TorqueZ-", () => state.TorqueZ <= dzn);
            AddItem("VelocityX+", () => state.VelocityX >= dzp);
            AddItem("VelocityX-", () => state.VelocityX <= dzn);
            AddItem("VelocityY+", () => state.VelocityY >= dzp);
            AddItem("VelocityY-", () => state.VelocityY <= dzn);
            AddItem("VelocityZ+", () => state.VelocityZ >= dzp);
            AddItem("VelocityZ-", () => state.VelocityZ <= dzn);
            AddItem("X+", () => state.X >= dzp);
            AddItem("X-", () => state.X <= dzn);
            AddItem("Y+", () => state.Y >= dzp);
            AddItem("Y-", () => state.Y <= dzn);
            AddItem("Z+", () => state.Z >= dzp);
            AddItem("Z-", () => state.Z <= dzn);

            // i don't know what the "Slider"s do, so they're omitted for the moment

            for (int i = 0; i < state.GetButtons().Length; i++)
            {
                int j = i;
                AddItem(string.Format("B{0}", i + 1), () => state.IsPressed(j));
            }

            for (int i = 0; i < state.GetPointOfViewControllers().Length; i++)
            {
                int j = i;
                AddItem(string.Format("POV{0}U", i + 1),
                    () => { int t = state.GetPointOfViewControllers()[j]; return (t >= 0 && t <= 4500) || (t >= 31500 && t < 36000); });
                AddItem(string.Format("POV{0}D", i + 1),
                    () => { int t = state.GetPointOfViewControllers()[j]; return t >= 13500 && t <= 22500; });
                AddItem(string.Format("POV{0}L", i + 1),
                    () => { int t = state.GetPointOfViewControllers()[j]; return t >= 22500 && t <= 31500; });
                AddItem(string.Format("POV{0}R", i + 1),
                    () => { int t = state.GetPointOfViewControllers()[j]; return t >= 4500 && t <= 13500; });
            }
        }
    }
}
