using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using System.Diagnostics;
using System.Threading;

namespace jdAffi
{
    public class jdAffiController
    {
        private I2cDevice devArduino;
        private Timer periodicTimer;
        private string retValue = "00000";

        public jdAffiController()
        {
            Init();
        }

        private async void Init()
        {
            try
            {
                var settings = new I2cConnectionSettings(0x40);
                settings.BusSpeed = I2cBusSpeed.StandardMode;
                string aqs = I2cDevice.GetDeviceSelector("I2C1");
                var dis = await DeviceInformation.FindAllAsync(aqs).AsTask();
                devArduino = await I2cDevice.FromIdAsync(dis[0].Id, settings);
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
            }
            // InitializeSensors();
            periodicTimer = new Timer(TimerCallback, null, 0, 1000); 
        }

        public string Values
        {
            get
            {
                return this.retValue;
            }
            set
            {
                this.retValue = value;
            }
        }

        private void TimerCallback(object state)
        {
           // byte[] RegAddrBuf = new byte[] { 0x40 };
            byte[] ReadBuf = new byte[5];

            try
            {
                devArduino.Read(ReadBuf);
                char[] cArray = System.Text.Encoding.UTF8.GetString(ReadBuf, 0, 5).ToCharArray();
                String c = new String(cArray);
                Debug.WriteLine(c);
                retValue = c;
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
                retValue = "00123";
            }
        }

        public  void StartWatering()
        {
            byte[] WriteBuf = new byte[1];
            WriteBuf[0] = 1;
            try
            {
                devArduino.Write (WriteBuf);
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
            }
        }

        public  void StopWatering()
        {
            byte[] WriteBuf = new byte[1];
            WriteBuf[0] = 2;
            try
            {
                devArduino.Write(WriteBuf);
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
            }
        }

        public void FillCan()
        {
            byte[] WriteBuf = new byte[1];
            WriteBuf[0] = 3;
            try
            {
                devArduino.Write(WriteBuf);
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
            }
        }

        public void StopFillCan()
        {
            byte[] WriteBuf = new byte[1];
            WriteBuf[0] = 4;
            try
            {
                devArduino.Write(WriteBuf);
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
            }
        }
    }
}
