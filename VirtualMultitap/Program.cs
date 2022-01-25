using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Microsoft.DirectX.DirectInput;

using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

using static Nefarius.ViGEm.Client.Targets.DualShock4.DualShock4Axis;
using static Nefarius.ViGEm.Client.Targets.DualShock4.DualShock4Button;
using static Nefarius.ViGEm.Client.Targets.DualShock4.DualShock4DPadDirection;
using System.Linq;
using System.Runtime.InteropServices;

namespace VirtualMultitap
{
    public static class Extensions
    {
        public static int Axis(this JoystickState @this, int idx)
        {
            switch (idx)
            {
                case 00: return @this.ARx;
                case 01: return @this.ARy;
                case 02: return @this.ARz;
                case 06: return @this.FRx;
                case 07: return @this.FRy;
                case 08: return @this.FRz;
                case 15: return @this.VRx;
                case 16: return @this.VRy;
                case 17: return @this.VRz;
                case 03: return @this.AX;
                case 04: return @this.AY;
                case 05: return @this.AZ;
                case 09: return @this.FX;
                case 10: return @this.FY;
                case 11: return @this.FZ;
                case 12: return @this.Rx;
                case 13: return @this.Ry;
                case 14: return @this.Rz;
                case 18: return @this.VX;
                case 19: return @this.VY;
                case 20: return @this.VZ;
                case 21: return @this.X;
                case 22: return @this.Y;
                case 23: return @this.Z;
                default: return 32767;
            }
        }

        public static byte ToDSAxis(this int @this) => (byte)Math.Round(255 * (@this / 65535.0));

        public static DualShock4DPadDirection DPad(this int @this)
        {
            switch (@this)
            {
                case 0: return North;
                case 4500: return Northeast;
                case 9000: return East;
                case 13500: return Southeast;
                case 18000: return South;
                case 22500: return Southwest;
                case 27000: return West;
                case 31500: return Northwest;
            }
            return None;
        }

        public static int GetButton(this JoystickState joyStick)
        {
            byte[] btns = joyStick.GetButtons();
            for (int i = 0; i < btns.Length; i++)
                if (btns[i] > 0)
                    return i;
            return -1;
        }

        public static int GetAxis(this JoystickState @this, JoystickState refState)
        {
            int crntDiff, mostDiff = 16383, index = -1;
            if ((crntDiff = Math.Abs(refState.ARx - @this.ARx)) > mostDiff) { mostDiff = crntDiff; index = 00; }
            if ((crntDiff = Math.Abs(refState.ARy - @this.ARy)) > mostDiff) { mostDiff = crntDiff; index = 01; }
            if ((crntDiff = Math.Abs(refState.ARz - @this.ARz)) > mostDiff) { mostDiff = crntDiff; index = 02; }
            if ((crntDiff = Math.Abs(refState.FRx - @this.FRx)) > mostDiff) { mostDiff = crntDiff; index = 06; }
            if ((crntDiff = Math.Abs(refState.FRy - @this.FRy)) > mostDiff) { mostDiff = crntDiff; index = 07; }
            if ((crntDiff = Math.Abs(refState.FRz - @this.FRz)) > mostDiff) { mostDiff = crntDiff; index = 08; }
            if ((crntDiff = Math.Abs(refState.VRx - @this.VRx)) > mostDiff) { mostDiff = crntDiff; index = 15; }
            if ((crntDiff = Math.Abs(refState.VRy - @this.VRy)) > mostDiff) { mostDiff = crntDiff; index = 16; }
            if ((crntDiff = Math.Abs(refState.VRz - @this.VRz)) > mostDiff) { mostDiff = crntDiff; index = 17; }
            if ((crntDiff = Math.Abs(refState.AX - @this.AX)) > mostDiff) { mostDiff = crntDiff; index = 03; }
            if ((crntDiff = Math.Abs(refState.AY - @this.AY)) > mostDiff) { mostDiff = crntDiff; index = 04; }
            if ((crntDiff = Math.Abs(refState.AZ - @this.AZ)) > mostDiff) { mostDiff = crntDiff; index = 05; }
            if ((crntDiff = Math.Abs(refState.FX - @this.FX)) > mostDiff) { mostDiff = crntDiff; index = 09; }
            if ((crntDiff = Math.Abs(refState.FY - @this.FY)) > mostDiff) { mostDiff = crntDiff; index = 10; }
            if ((crntDiff = Math.Abs(refState.FZ - @this.FZ)) > mostDiff) { mostDiff = crntDiff; index = 11; }
            if ((crntDiff = Math.Abs(refState.Rx - @this.Rx)) > mostDiff) { mostDiff = crntDiff; index = 12; }
            if ((crntDiff = Math.Abs(refState.Ry - @this.Ry)) > mostDiff) { mostDiff = crntDiff; index = 13; }
            if ((crntDiff = Math.Abs(refState.Rz - @this.Rz)) > mostDiff) { mostDiff = crntDiff; index = 14; }
            if ((crntDiff = Math.Abs(refState.VX - @this.VX)) > mostDiff) { mostDiff = crntDiff; index = 18; }
            if ((crntDiff = Math.Abs(refState.VY - @this.VY)) > mostDiff) { mostDiff = crntDiff; index = 19; }
            if ((crntDiff = Math.Abs(refState.VZ - @this.VZ)) > mostDiff) { mostDiff = crntDiff; index = 20; }
            if ((crntDiff = Math.Abs(refState.X - @this.X)) > mostDiff) { mostDiff = crntDiff; index = 21; }
            if ((crntDiff = Math.Abs(refState.Y - @this.Y)) > mostDiff) { mostDiff = crntDiff; index = 22; }
            if (Math.Abs(refState.Z - @this.Z) > mostDiff) index = 23;
            return index;
        }
    }

    public class Program
    {
        public static bool[][] prevStates = new bool[12][];

        static void Main(string[] _)
        {
            // 1)
            Console.WriteLine("Enter number of virtual controllers (1 - 4):");

            int numCtrl;
            while (!int.TryParse(Console.ReadLine(), out numCtrl) || numCtrl < 1 || numCtrl > 4)
                Console.WriteLine("Invalid number ...");

            // 2)
            List<Guid> controllers = new List<Guid>();
            foreach (DeviceInstance deviceInstance in Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly))
                controllers.Add(deviceInstance.InstanceGuid);

            // 3)
            ViGEmClient client = new ViGEmClient();
            IDualShock4Controller[] virtualControllers = new IDualShock4Controller[numCtrl];
            IDualShock4Controller[] activeVirtualControllers = new IDualShock4Controller[numCtrl];
            for (int i = 0; i < virtualControllers.Length; i++)
            {
                virtualControllers[i] = client.CreateDualShock4Controller();
                activeVirtualControllers[i] = virtualControllers[i];
                virtualControllers[i].Connect();
            }

            Console.WriteLine("\nSelect master controller:\n 0: Detect");

            // 4)
            for (int i = 0; i < controllers.Count; i++)
                Console.WriteLine($" {i + 1}: {controllers[i]}");

            int selectedController;
            while (!int.TryParse(Console.ReadLine(), out selectedController) || selectedController < 0 || selectedController > controllers.Count)
                Console.WriteLine("Invalid number ...");

            Device masterController = null;

            if (selectedController == 0)
            {
                // 4.1)
                Console.WriteLine($"\nPress any key on the desired controller");
                Device[] devices = new Device[controllers.Count];
                for (int i = 0; i < controllers.Count; i++)
                {
                    Guid controller = controllers[i];
                    if (controller != null)
                        try
                        {
                            devices[i] = new Device(controller);
                            devices[i].SetDataFormat(DeviceDataFormat.Joystick);
                            devices[i].Acquire();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Could not aquire: {controllers[i]}\n{ex}");
                            devices[i] = null;
                        }
                }
                bool notFound = true;
                while (notFound)
                    foreach (Device d in devices)
                    {
                        if (d != null)
                        {
                            d.Poll();
                            JoystickState jss = d.CurrentJoystickState;
                            if (jss.GetPointOfView()[0].DPad() != None)
                            {
                                masterController = d;
                                notFound = false;
                                break;
                            }
                            foreach (byte b in jss.GetButtons())
                            {
                                if (b > 0)
                                {
                                    masterController = d;
                                    notFound = false;
                                    break;
                                }
                            }
                        }
                        if (!notFound) break;
                    }
                Console.WriteLine($"Master controller: {masterController.DeviceInformation.InstanceGuid}");
                foreach (Device d in devices)
                    if (d != masterController)
                        d.Unacquire();
            }
            else
            {
                // 4.2)
                try
                {
                    masterController = new Device(controllers[--selectedController]);
                    masterController.SetDataFormat(DeviceDataFormat.Joystick);
                    masterController.Acquire();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Could not aquire: {controllers[selectedController]}");
                    Console.ReadLine();
                    return;
                }
            }

            // 5)
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string virtualMultitap = Path.Combine(documentsFolder, "Virtual Multitap");

            if (!Directory.Exists(virtualMultitap))
            {
                Directory.CreateDirectory(virtualMultitap);
                Console.WriteLine("'Virtual Multitap' folder created in Documents\n");
            }

            string profilePath = Path.Combine(virtualMultitap, masterController.DeviceInformation.InstanceGuid.ToString());

            List<int> btnIndexes = new List<int>();
            List<int> axisIndexes = new List<int>();

            int btnCross = -1;
            int btnSquare = -1;
            int btnTriangle = -1;
            int btnCircle = -1;

            int btnOption = -1;
            int btnShare = -1;

            int btnSL = -1;
            int btnSR = -1;

            int btnTgrL = -1;
            int btnTgrR = -1;

            int btnTL = -1;
            int btnTR = -1;

            int axisLX = -1;
            int axisLY = -1;

            int axisRX = -1;
            int axisRY = -1;

            if (File.Exists(profilePath))
            {
                Console.WriteLine($"Using saved mappings for: {masterController.DeviceInformation.InstanceGuid}");
            }
            else
            {
                Console.WriteLine($"No mappings found, configuration needed ...\n Press Enter to start");
                Console.ReadLine();

                masterController.Poll();
                JoystickState reference = masterController.CurrentJoystickState;

                bool configuring = true;
                int stage = 0;

                while (configuring)
                {
                    switch (stage)
                    {
                        case 0:
                            {
                                Console.WriteLine($"Press a button for '{Cross}':");
                                while (btnCross == -1 || btnIndexes.Contains(btnCross))
                                {
                                    masterController.Poll();
                                    btnCross = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnCross);
                                break;
                            }
                        case 1:
                            {
                                Console.WriteLine($"Press a button for '{Square}':");
                                while (btnSquare == -1 || btnIndexes.Contains(btnSquare))
                                {
                                    masterController.Poll();
                                    btnSquare = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnSquare);
                                break;
                            }
                        case 2:
                            {
                                Console.WriteLine($"Press a button for '{Triangle}':");
                                while (btnTriangle == -1 || btnIndexes.Contains(btnTriangle))
                                {
                                    masterController.Poll();
                                    btnTriangle = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnTriangle);
                                break;
                            }
                        case 3:
                            {
                                Console.WriteLine($"Press a button for '{Circle}':");
                                while (btnCircle == -1 || btnIndexes.Contains(btnCircle))
                                {
                                    masterController.Poll();
                                    btnCircle = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnCircle);
                                break;
                            }
                        case 4:
                            {
                                Console.WriteLine($"Press a button for '{Options} (Start)':");
                                while (btnOption == -1 || btnIndexes.Contains(btnOption))
                                {
                                    masterController.Poll();
                                    btnOption = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnOption);
                                break;
                            }
                        case 5:
                            {
                                Console.WriteLine($"Press a button for '{Share} (Select)':");
                                while (btnShare == -1 || btnIndexes.Contains(btnShare))
                                {
                                    masterController.Poll();
                                    btnShare = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnShare);
                                break;
                            }
                        case 6:
                            {
                                Console.WriteLine($"Press a button for '{ShoulderLeft}':");
                                while (btnSL == -1 || btnIndexes.Contains(btnSL))
                                {
                                    masterController.Poll();
                                    btnSL = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnSL);
                                break;
                            }
                        case 7:
                            {
                                Console.WriteLine($"Press a button for '{ShoulderRight}':");
                                while (btnSR == -1 || btnIndexes.Contains(btnSR))
                                {
                                    masterController.Poll();
                                    btnSR = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnSR);
                                break;
                            }
                        case 8:
                            {
                                Console.WriteLine($"Press a button for '{TriggerLeft}':");
                                while (btnTgrL == -1 || btnIndexes.Contains(btnTgrL))
                                {
                                    masterController.Poll();
                                    btnTgrL = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnTgrL);
                                break;
                            }
                        case 9:
                            {
                                Console.WriteLine($"Press a button for '{TriggerRight}':");
                                while (btnTgrR == -1 || btnIndexes.Contains(btnTgrR))
                                {
                                    masterController.Poll();
                                    btnTgrR = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnTgrR);
                                break;
                            }
                        case 10:
                            {
                                Console.WriteLine($"Press a button for '{ThumbLeft}':");
                                while (btnTL == -1 || btnIndexes.Contains(btnTL))
                                {
                                    masterController.Poll();
                                    btnTL = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnTL);
                                break;
                            }
                        case 11:
                            {
                                Console.WriteLine($"Press a button for '{ThumbRight}':");
                                while (btnTR == -1 || btnIndexes.Contains(btnTR))
                                {
                                    masterController.Poll();
                                    btnTR = masterController.CurrentJoystickState.GetButton();
                                }
                                btnIndexes.Add(btnTR);
                                break;
                            }
                        case 12:
                            {
                                Console.WriteLine($"Move axis for '{LeftThumbX}':");
                                while (axisLX == -1 || axisIndexes.Contains(axisLX))
                                {
                                    masterController.Poll();
                                    axisLX = masterController.CurrentJoystickState.GetAxis(reference);
                                }
                                axisIndexes.Add(axisLX);
                                break;
                            }
                        case 13:
                            {
                                Console.WriteLine($"Move axis for '{LeftThumbY}':");
                                while (axisLY == -1 || axisIndexes.Contains(axisLY))
                                {
                                    masterController.Poll();
                                    axisLY = masterController.CurrentJoystickState.GetAxis(reference);
                                }
                                axisIndexes.Add(axisLY);
                                break;
                            }
                        case 14:
                            {
                                Console.WriteLine($"Move axis for '{RightThumbX}':");
                                while (axisRX == -1 || axisIndexes.Contains(axisRX))
                                {
                                    masterController.Poll();
                                    axisRX = masterController.CurrentJoystickState.GetAxis(reference);
                                }
                                axisIndexes.Add(axisRX);
                                break;
                            }
                        case 15:
                            {
                                Console.WriteLine($"Move axis for '{RightThumbY}':");
                                while (axisRY == -1 || axisIndexes.Contains(axisRY))
                                {
                                    masterController.Poll();
                                    axisRY = masterController.CurrentJoystickState.GetAxis(reference);
                                }
                                axisIndexes.Add(axisRY);
                                configuring = false;
                                break;
                            }
                    }
                    stage++;
                }
                Console.WriteLine($"Configuration for {masterController.DeviceInformation.InstanceGuid} complete\n");

                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"{btnCross}");
                sb.AppendLine($"{btnSquare}");
                sb.AppendLine($"{btnTriangle}");
                sb.AppendLine($"{btnCircle}");

                sb.AppendLine($"{btnOption}");
                sb.AppendLine($"{btnShare}");

                sb.AppendLine($"{btnSL}");
                sb.AppendLine($"{btnSR}");

                sb.AppendLine($"{btnTgrL}");
                sb.AppendLine($"{btnTgrR}");

                sb.AppendLine($"{btnTL}");
                sb.AppendLine($"{btnTR}");

                sb.AppendLine($"{axisLX}");
                sb.AppendLine($"{axisLY}");

                sb.AppendLine($"{axisRX}");
                sb.AppendLine($"{axisRY}");

                File.WriteAllText(profilePath, sb.ToString());
            }

            // 6)
            // Loads a mapping for the master controller
            string[] lines = File.ReadAllLines(profilePath);
            int[] profile = new int[lines.Length];
            for (int i = 0; i < lines.Length; i++)
                profile[i] = int.Parse(lines[i]);
            lines = null;

            int lastD = 0;
            for (int i = 0; i < prevStates.Length; i++)
                prevStates[i] = new bool[numCtrl];

            // 7)
            Console.WriteLine("Multitap started\n");

            SpinWait.SpinUntil(() =>
            {
                Keys.Poll();

                if (Keys.D1 && lastD != 1)
                {
                    lastD = 1;
                    Console.WriteLine("Switching to virtual controller 1 only");
                    for (int i = 0; i < numCtrl; i++)
                        if (i == 0)
                            activeVirtualControllers[i] = virtualControllers[i];
                        else
                            activeVirtualControllers[i] = null;
                }

                if (Keys.D2 && lastD != 2)
                {
                    lastD = 2;
                    Console.WriteLine("Switching to virtual controller 2 only");
                    for (int i = 0; i < numCtrl; i++)
                        if (i == 1)
                            activeVirtualControllers[i] = virtualControllers[i];
                        else
                            activeVirtualControllers[i] = null;
                }

                if (Keys.D3 && lastD != 3)
                {
                    lastD = 3;
                    Console.WriteLine("Switching to virtual controller 3 only");
                    for (int i = 0; i < numCtrl; i++)
                        if (i == 2)
                            activeVirtualControllers[i] = virtualControllers[i];
                        else
                            activeVirtualControllers[i] = null;
                }

                if (Keys.D4 && lastD != 4)
                {
                    lastD = 4;
                    Console.WriteLine("Switching to virtual controller 4 only");
                    for (int i = 0; i < numCtrl; i++)
                        if (i == 3)
                            activeVirtualControllers[i] = virtualControllers[i];
                        else
                            activeVirtualControllers[i] = null;
                }

                if (Keys.D5 && lastD != 5)
                {
                    lastD = 5;
                    Console.WriteLine("Enabled all virtual controllers");
                    for (int i = 0; i < numCtrl; i++)
                        activeVirtualControllers[i] = virtualControllers[i];
                }

                masterController.Poll();
                ApplyProfile(profile, masterController.CurrentJoystickState, activeVirtualControllers);

                return false; // Return false so SpinWait continues indefinitely (until program closes)
            });
        }

        // Applied the mapping profile and query for button presses etc
        public static void ApplyProfile(int[] profile, JoystickState joystickState, IDualShock4Controller[] controllers)
        {
            byte[] buttons = joystickState.GetButtons();

            // get/set cross state
            bool pressed = buttons[profile[0]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[0][i])
                    controller.SetButtonState(Cross, prevStates[0][i] = pressed);
            }

            // get/set square
            pressed = buttons[profile[1]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[1][i])
                    controller.SetButtonState(Square, prevStates[1][i] = pressed);
            }

            // get/set triangle
            pressed = buttons[profile[2]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[2][i])
                    controller.SetButtonState(Triangle, prevStates[2][i] = pressed);
            }

            // get/set circle
            pressed = buttons[profile[3]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[3][i])
                    controller.SetButtonState(Circle, prevStates[3][i] = pressed);
            }

            // get/set option
            pressed = buttons[profile[4]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[4][i])
                    controller.SetButtonState(Options, prevStates[4][i] = pressed);
            }

            // get/set share
            pressed = buttons[profile[5]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[5][i])
                    controller.SetButtonState(Share, prevStates[5][i] = pressed);
            }

            // get/set SL
            pressed = buttons[profile[6]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[6][i])
                    controller.SetButtonState(ShoulderLeft, prevStates[6][i] = pressed);
            }

            // get/set SR
            pressed = buttons[profile[7]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[7][i])
                    controller.SetButtonState(ShoulderRight, prevStates[7][i] = pressed);
            }

            /// The triggers seem to be sliders and sometimes buttons? iuno so i just did both XD
            // get/set TgrL
            pressed = buttons[profile[8]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[8][i])
                {
                    controller.SetButtonState(TriggerLeft, prevStates[8][i] = pressed);
                    controller.SetSliderValue(DualShock4Slider.LeftTrigger, (byte)(pressed ? 255 : 0));
                }
            }

            // get/set TgrR
            pressed = buttons[profile[9]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[9][i])
                {
                    controller.SetButtonState(TriggerRight, prevStates[9][i] = pressed);
                    controller.SetSliderValue(DualShock4Slider.RightTrigger, (byte)(pressed ? 255 : 0));
                }
            }

            // get/set TL
            pressed = buttons[profile[10]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[10][i])
                    controller.SetButtonState(ThumbLeft, prevStates[10][i] = pressed);
            }

            // get/set TR
            pressed = buttons[profile[11]] > 0;
            for (int i = 0; i < controllers.Length; i++)
            {
                IDualShock4Controller controller = controllers[i];
                if (controller != null && pressed != prevStates[11][i])
                    controller.SetButtonState(ThumbRight, prevStates[11][i] = pressed);
            }

            // get/set LX
            byte axis = joystickState.Axis(profile[12]).ToDSAxis();
            foreach (IDualShock4Controller controller in controllers)
                if (controller != null)
                    controller.SetAxisValue(LeftThumbX, axis);

            // get/set LY
            axis = joystickState.Axis(profile[13]).ToDSAxis();
            foreach (IDualShock4Controller controller in controllers)
                if (controller != null)
                    controller.SetAxisValue(LeftThumbY, axis);

            // get/set RX
            axis = joystickState.Axis(profile[14]).ToDSAxis();
            foreach (IDualShock4Controller controller in controllers)
                if (controller != null)
                    controller.SetAxisValue(RightThumbX, axis);

            // get/set RY
            axis = joystickState.Axis(profile[15]).ToDSAxis();
            foreach (IDualShock4Controller controller in controllers)
                if (controller != null)
                    controller.SetAxisValue(RightThumbY, axis);

            // get/set Direction
            DualShock4DPadDirection direction = joystickState.GetPointOfView()[0].DPad();
            foreach (IDualShock4Controller controller in controllers)
                if (controller != null)
                    controller.SetDPadDirection(direction);
        }
    }

    // Get keyboard inputs (but only 1 - 5)
    public static class Keys
    {
        [DllImport("user32")]
        private static extern sbyte GetKeyState(int nVirtKey);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        private static byte[] keyboardStates = new byte[256];
        private static readonly byte[] newState = new byte[256];
        internal static void Poll(bool raiseEvent = false)
        {
            GetKeyState(0);
            GetKeyboardState(newState);
            if (raiseEvent && !newState.SequenceEqual(keyboardStates))
                keyboardStates = newState;
            else
                keyboardStates = newState;
        }

        public static bool D1 => (keyboardStates[49] & 0x80) != 0;
        public static bool D2 => (keyboardStates[50] & 0x80) != 0;
        public static bool D3 => (keyboardStates[51] & 0x80) != 0;
        public static bool D4 => (keyboardStates[52] & 0x80) != 0;
        public static bool D5 => (keyboardStates[53] & 0x80) != 0;
    }
}