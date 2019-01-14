using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace GreenSpoof
{
    class Program
    {
        static void Main()
        {

            Console.WriteLine(@"   ______                    _____                   ____");
            Console.WriteLine(@"  / ____/_______  ___  ____ / ___/____  ____  ____  / __/");
            Console.WriteLine(@" / / __/ ___/ _ \/ _ \/ __ \\__ \/ __ \/ __ \/ __ \/ /_  ");
            Console.WriteLine(@"/ /_/ / /  /  __/  __/ / / /__/ / /_/ / /_/ / /_/ / __/  ");
            Console.WriteLine(@"\____/_/   \___/\___/_/ /_/____/ .___/\____/\____/_/     ");
            Console.WriteLine(@"                              /_/                        ");
            Console.WriteLine();
            Console.WriteLine();

            if (IsAdministrator())
            {
                Console.WriteLine("NetworkAddress: " + NetworkAddress());
                Console.WriteLine("ComputerHardwareId: " + ComputerHardwareId());
                Console.WriteLine("BIOSVendor: " + BIOSVendor());
                Console.WriteLine("BIOSReleaseDate: " + BIOSReleaseDate());
                Console.WriteLine("SystemManufacturer: " + SystemManufacturer());
                Console.WriteLine("SystemProductName: " + SystemProductName());
                Console.WriteLine("DriverDesc: " + DriverDesc());
                Console.WriteLine("ProductId: " + ProductId());
                Console.WriteLine("SusClientId: " + SusClientId());

                SpoofInstallDate();
                SpoofInstallTime();
                //removeMachineGuid();
                //SpoofPCName();
                //ListHW();
            }
            else
            {
                Console.WriteLine("Please run the program as administrator.");
            }

            Console.ReadLine();
        }

        static string NetworkAddress() { return GetRegistryKey(@"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}\0012", "NetworkAddress"); }
        static string ComputerHardwareId() { return GetRegistryKey(@"SYSTEM\CurrentControlSet\Control\SystemInformation", "ComputerHardwareId"); }
        static string BIOSVendor() { return GetRegistryKey(@"HARDWARE\Description\System\BIOS", "BIOSVendor"); }
        static string BIOSReleaseDate() { return GetRegistryKey(@"HARDWARE\Description\System\BIOS", "BIOSReleaseDate"); }
        static string SystemManufacturer() { return GetRegistryKey(@"HARDWARE\Description\System\BIOS", "SystemManufacturer"); }
        static string SystemProductName() { return GetRegistryKey(@"HARDWARE\Description\System\BIOS", "SystemProductName"); }

        static string DriverDesc() { return GetRegistryKeyOther(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "DriverDesc"); }
        static string ProductId() { return GetRegistryKeyOther(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductId"); }
        static string SusClientId() { return GetRegistryKeyOther(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate", "SusClientId"); }

        static void ListHW()
        {
            Console.WriteLine("Hard Drives");
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\Scsi"))
            {
                if (key != null)
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        RegistryKey subKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\Scsi\" + subKeyName);

                        if (subKey != null)
                        {
                            foreach (string subSubKeyName in subKey.GetSubKeyNames())
                            {
                                RegistryKey subSubKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\Scsi\" + subKeyName + @"\" + subSubKeyName);

                                if (subSubKey != null)
                                {
                                    foreach (string subSubSubKeyName in subSubKey.GetSubKeyNames())
                                    {
                                        RegistryKey subSubSubKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\Scsi\" + subKeyName + @"\" + subSubKeyName + @"\" + subSubSubKeyName);

                                        if (subSubSubKey != null)
                                        {
                                            foreach (string subSubSubSubKeyName in subSubSubKey.GetSubKeyNames())
                                            {
                                                RegistryKey logicalUnitKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\Scsi\" + subKeyName + @"\" + subSubKeyName + @"\" + subSubSubKeyName + @"\Logical Unit Id 0");

                                                if (logicalUnitKey != null)
                                                {
                                                    string identifier = (string)logicalUnitKey.GetValue("Identifier");
                                                    string serialNumber = (string)logicalUnitKey.GetValue("SerialNumber");
                                                    logicalUnitKey.Close();

                                                    Console.WriteLine("-----------------");
                                                    Console.WriteLine(identifier);
                                                    Console.WriteLine(serialNumber);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        static void removeMachineGuid()
        {
            string path = @"C:\Windows\System32\restore\MachineGuid.txt";

            FileAttributes attributes = File.GetAttributes(path);
            attributes = attributes & ~FileAttributes.ReadOnly;
            File.SetAttributes(path, attributes);
            Console.WriteLine("The {0} file is no longer RO.", path);

            if (File.Exists(path))
            {
                File.Delete(path);
                Console.WriteLine("MachineGuid deleted");
            }
        }

        #region InstallDateSpoof
        static void SpoofInstallDate()
        {
            Console.WriteLine("Current Install Date: " + GetRegistryKeyOther(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallDate"));

            string newInstallDate = GenerateDate(8);

            RegistryKey OurKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            OurKey = OurKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", true);
            OurKey.SetValue("InstallDate", newInstallDate);
            OurKey.Close();

            Console.WriteLine("New Install Date: " + GetRegistryKeyOther(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallDate"));
        }
        #endregion

        #region InstallTimeSpoof
        static void SpoofInstallTime()
        {
            Console.WriteLine("Current Install Time: " + GetRegistryKeyOther(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallTime"));

            string newInstallTime = GenerateDate(15);

            RegistryKey OurKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            OurKey = OurKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", true);
            OurKey.SetValue("InstallTime", newInstallTime);
            OurKey.Close();
            Console.WriteLine("New Install Time: " + GetRegistryKeyOther(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallTime"));
        }
        #endregion

        #region SpoofPCName
        static void SpoofPCName()
        {
            Console.WriteLine("Current PC name: " + GetRegistryKeyOther(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", "ComputerName"));

            RegistryKey OurKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            OurKey = OurKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", true);
            OurKey.SetValue("ComputerName", "DESKTOP-" + GenerateString(15));
            OurKey.Close();

            Console.WriteLine("New PC name: " + GetRegistryKeyOther(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", "ComputerName"));
        }
        #endregion

        #region Extras
        static Random random = new Random();
        const string Alphabet1 = "abcdef0123456789";
        static string GenerateDate(int size)
        {
            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = Alphabet1[random.Next(Alphabet1.Length)];
            }
            return new string(chars);
        }
        static Random rand = new Random();
        const string Alphabet = "ABCDEF0123456789";
        static string GenerateString(int size)
        {
            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = Alphabet[rand.Next(Alphabet.Length)];
            }
            return new string(chars);
        }
        static string GetRegistryKey(string Key, string SubKey)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Key, true);

            if (key != null)
            {
                string value = (string)key.GetValue(SubKey);
                key.Close();
                return value;
            }

            return "Can't find it";
        }
        static string GetRegistryKeyOther(string key, string subKey)
        {
            using (RegistryKey localMachineX64View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey rk = localMachineX64View.OpenSubKey(key))
                {
                    if (rk == null)
                    {
                        Console.WriteLine(string.Format("Key Not Found: {0}", key));
                        return "";
                    }
                        
                    object rv = rk.GetValue(subKey);
                    if (rv == null)
                    {
                        Console.WriteLine(string.Format("Index Not Found: {0}", subKey));
                        return "";
                    }

                    return rv.ToString();
                }
            }
        }
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }
        #endregion
    }
}
