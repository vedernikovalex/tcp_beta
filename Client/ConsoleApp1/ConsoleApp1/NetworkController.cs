using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

//https://stackoverflow.com/questions/474803/how-to-simulate-network-failure-for-test-purposes-in-c

namespace Client
{

    static class NetworkController
    {

        public static void Disable()
        {
            Console.WriteLine("disable");
            SetIP("192.168.0.4", "255.255.255.0");
        }

        public static void Enable()
        {
            SetDHCP();
        }


        private static void SetIP(string ip_address, string subnet_mask)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    try
                    {
                        ManagementBaseObject setIP = default(ManagementBaseObject);
                        ManagementBaseObject newIP = objMO.GetMethodParameters("EnableStatic");

                        newIP["IPAddress"] = new string[] { ip_address };
                        newIP["SubnetMask"] = new string[] { subnet_mask };

                        setIP = objMO.InvokeMethod("EnableStatic", newIP, null);
                    }
                    catch (Exception generatedExceptionName)
                    {
                        throw;
                    }
                }


            }
        }

        private static void SetDHCP()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                // Make sure this is a IP enabled device. Not something like memory card or VM Ware
                if ((bool)mo["IPEnabled"])
                {
                    ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    newDNS["DNSServerSearchOrder"] = null;
                    ManagementBaseObject enableDHCP = mo.InvokeMethod("EnableDHCP", null, null);
                    ManagementBaseObject setDNS = mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                }
            }
        }
    }
}
