using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Win32;

namespace Optimize_My_Computer
{
    /// <summary>
    /// Proctorio Inc, Open Source Initiative 2015 https://proctorio.com
    /// </summary>
    public partial class MainWindow
    {
        [DllImport("powrprof.dll", EntryPoint = "PowerSetActiveScheme")]
        public static extern uint PowerSetActiveScheme(IntPtr userPowerKey, ref Guid activePolicyGuid);

        public MainWindow()
        {
            InitializeComponent();
        }

        // remove the window icon, clean is key
        protected override void OnSourceInitialized(EventArgs e) { IconHelper.RemoveIcon(this); }

        // delegates for ui update outside of render thread
        public delegate void UpdateProgressCallback(double v);
        public delegate void ShowCheckCallback();
        public delegate void UpdateTitleCallback(string m);
        public delegate void HideCheckCallback();
        public delegate void SetCursorCallback();
        public delegate void ShowXCallback();
        public delegate void ShowDesktopCCallback();
        public delegate void ShowRebootCallback();
        public delegate void ShowLaptopCCallback();
        public delegate void HideDesktopCCallback();
        public delegate void HideLaptopCCallback();
        public delegate void HideRebootCallback();

        // are we ready for reboot?
        public bool IsReady;
        public bool IsFired;

        // thread to animate and unmute microphones
        private void UiThread()
        {
            // hang tight
            P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), ("hang tight"));

            // determine laptop vs desktop icon
            // just check for battery... i know, if they remove the battery then this is a false value but whatever
            if(SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery)
            {
                DesktopC.Dispatcher.Invoke(new ShowDesktopCCallback(ShowDesktopC));
            }
            else
            {
                LaptopC.Dispatcher.Invoke(new ShowDesktopCCallback(ShowLaptopC));
            }

            // sleep a bit
            Thread.Sleep(500);

            // find me some microphones
            P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), ("finding startup items to fix..."));

            // remove all startup items
            // first pass, stage 1
            // x86
            try
            {
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), new object[] { 0 });
                RegistryKey machinestartupKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (machinestartupKey != null)
                {
                    string[] items = machinestartupKey.GetValueNames();
                    int itemsLength = items.Length;
                    for (int i = 0; i < itemsLength; i++)
                    {
                        // taking care of..
                        P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), (1.0 - (((double)itemsLength - (double)i) / (double)itemsLength)) * 100);
                        P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), ("found: " + items[i].ToLower()));

                        // delete it, skip nod32
                        if (items[i] != "egui") machinestartupKey.DeleteValue(items[i], false);
                        Thread.Sleep(500);
                    }
                }
                if (machinestartupKey != null)
                {
                    machinestartupKey.Close();
                }
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), 100);
                Thread.Sleep(500);
            }
            catch
            {
                // ignored
            }

            // x64
            try
            {
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), new object[] { 0 });
                RegistryKey machinestartupKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (machinestartupKey != null)
                {
                    string[] items = machinestartupKey.GetValueNames();
                    int itemsLength = items.Length;
                    for (int i = 0; i < itemsLength; i++)
                    {
                        // taking care of..
                        P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), (1.0 - (((double)itemsLength - (double)i) / (double)itemsLength)) * 100);
                        P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), ("found: " + items[i].ToLower()));

                        // delete it, skip nod32
                        if (items[i] != "egui") machinestartupKey.DeleteValue(items[i], false);
                        Thread.Sleep(500);
                    }
                }
                if (machinestartupKey != null)
                {
                    machinestartupKey.Close();
                }
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), 100);
                Thread.Sleep(500);
            }
            catch
            {
                // ignored
            }

            // second pass, stage 1
            // x86
            try
            {
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), new object[] { 0 });
                RegistryKey userstartupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (userstartupKey != null)
                {
                    string[] items = userstartupKey.GetValueNames();
                    int itemsLength = items.Length;
                    for (int i = 0; i < itemsLength; i++)
                    {
                        // taking care of..
                        P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), (1.0 - (((double)itemsLength - (double)i) / (double)itemsLength)) * 100);
                        P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), ("found: " + items[i].ToLower()));

                        // delete it
                        try
                        {
                            // delete it, skip nod32
                            if (items[i] != "egui") userstartupKey.DeleteValue(items[i], false);
                            Thread.Sleep(500);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                if (userstartupKey != null)
                {
                    userstartupKey.Close();
                }
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), 100);
                Thread.Sleep(500);
            }
            catch
            {
                // ignored
            }

            // x64
            try
            {
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), new object[] { 0 });
                RegistryKey userstartupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                string[] items = userstartupKey.GetValueNames();
                int itemsLength = items.Length;
                for (int i = 0; i < itemsLength; i++)
                {
                    // taking care of..
                    P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), (1.0 - (((double)itemsLength - (double)i) / (double)itemsLength)) * 100);
                    P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), ("found: " + items[i].ToLower()));

                    // delete it
                    try
                    {
                        // delete it, skip nod32
                        if (items[i] != "egui") userstartupKey.DeleteValue(items[i], false);
                        Thread.Sleep(500);
                    }
                    catch
                    {
                        // ignored
                    }
                }
                userstartupKey.Close();
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), 100);
                Thread.Sleep(500);
            }
            catch
            {
                // ignored
            }

            // third pass, stage 2
            try
            {
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), new object[] { 0 });
                DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                FileInfo[] thefiles = di.GetFiles("*.lnk");
                int itemsLength = thefiles.Length;
                for (int i = 0; i < itemsLength; i++)
                {
                    // taking care of..
                    P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), (1.0 - (((double)itemsLength - (double)i) / (double)itemsLength)) * 100);
                    P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), ("found: " + Path.GetFileNameWithoutExtension(thefiles[i].Name)).ToLower());

                    // delete it
                    try
                    {
                        File.Delete(thefiles[i].FullName);
                        Thread.Sleep(500);
                    }
                    catch
                    {
                        // ignored
                    }
                }
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), 100);
                Thread.Sleep(500);
            }
            catch
            {
                // ignored
            }

            // fourth pass, stage 2
            try
            {
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), new object[] { 0 });
                DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                FileInfo[] thefiles = di.GetFiles("*.exe");
                int itemsLength = thefiles.Length;
                for (int i = 0; i < itemsLength; i++)
                {
                    // taking care of..
                    P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), (1.0 - (((double)itemsLength - (double)i) / (double)itemsLength)) * 100);
                    P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), ("found: " + Path.GetFileNameWithoutExtension(thefiles[i].Name)).ToLower());

                    // delete it
                    try
                    {
                        File.Delete(thefiles[i].FullName);
                        Thread.Sleep(500);
                    }
                    catch
                    {
                        // ignored
                    }
                }
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), 100);
                Thread.Sleep(500);
            }
            catch
            {
                // ignored
            }

            // update power settings to high performance
            P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), ("set high performance mode"));
            P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), new object[] { 0 });
            Thread.Sleep(1500);

            // set to high performance
            try
            {
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), 33);
                Guid max = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
                PowerSetActiveScheme(IntPtr.Zero, ref max);
                P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), 100);
                Thread.Sleep(500);
            }
            catch
            {
                // ignored
            }

            // hide the icon
            DesktopC.Dispatcher.Invoke(new HideDesktopCCallback(HideDesktopC));
            LaptopC.Dispatcher.Invoke(new HideLaptopCCallback(HideLaptopC));

            // show the checkmark
            CheckMark.Dispatcher.Invoke(new ShowCheckCallback(ShowCheck));

            // done
            P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), "done with computer");

            // Zzzz
            Thread.Sleep(2000);

            // reset progressbar
            P.Dispatcher.Invoke(new UpdateProgressCallback(UpdateProgress), new object[] { 0 });

            // hide the check mark
            CheckMark.Dispatcher.Invoke(new HideCheckCallback(HideCheck));

            // click to reboot
            P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), "click anywhere to reboot");

            // show the reboot
            Reboot.Dispatcher.Invoke(new ShowRebootCallback(ShowReboot));

            // set the cursor
            CheckMark.Dispatcher.Invoke(new SetCursorCallback(SetCursor));
            IsReady = true;                       
        }

        // update the progressbar
        private void UpdateProgress(double v) { P.Value = v; }

        // show the checkmark
        private void ShowCheck() { CheckMark.Visibility = Visibility.Visible; }

        // show the laptop
        private void ShowLaptopC() { LaptopC.Visibility = Visibility.Visible; }

        // show the desktop
        private void ShowDesktopC() { DesktopC.Visibility = Visibility.Visible; }

        // show the reboot
        private void ShowReboot() { Reboot.Visibility = Visibility.Visible; }

        // hide the laptop
        private void HideLaptopC() { LaptopC.Visibility = Visibility.Hidden; }

        // hide the desktop
        private void HideDesktopC() { DesktopC.Visibility = Visibility.Hidden; }

        // title change for progress of events
        private void UpdateTitle(string m) { Title = m; }

        // hide the checkmark
        private void HideCheck() { CheckMark.Visibility = Visibility.Hidden; }

        // hide the reboot
        private void HideReboot() { Reboot.Visibility = Visibility.Hidden; }

        // set cursor to hand
        private void SetCursor() { Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand; }

        // show the x
        private void ShowX() { Xx.Visibility = Visibility.Visible; }

        // load it up
        private void OnWindowLoaded(object sender, RoutedEventArgs e) { Thread ui = new Thread(UiThread); ui.Start(); }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // did we fire?
            if (IsFired)
            {
                // we are done here
                Environment.Exit(0);
            }

            // are we ready?
            if (!IsReady)
            {
                return;
            }

            // yes we have now
            IsFired = true;

            // hide reboot
            Reboot.Dispatcher.Invoke(new HideRebootCallback(HideReboot));

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (identity == null)
            {
                return;
            }

            WindowsPrincipal principal = new WindowsPrincipal(identity);
            var privilege = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (privilege)
            {
                // rebooting
                P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), "rebooting...");

                // reboot command
                Process.Start("shutdown.exe", "-r -t 0");

                // we are done here
                Environment.Exit(0);
            }
            else
            {
                // failure, can't continue
                P.Dispatcher.Invoke(new UpdateTitleCallback(UpdateTitle), "unable to automatically restart the computer");

                // show failure X
                Xx.Dispatcher.Invoke(new ShowXCallback(ShowX));
            }
        }
    }
}