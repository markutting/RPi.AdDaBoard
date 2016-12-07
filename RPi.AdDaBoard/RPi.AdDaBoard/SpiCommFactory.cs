﻿using System;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

namespace Emmellsoft.IoT.Rpi.AdDaBoard
{
    internal class SpiCommFactory : IDisposable
    {
        private readonly SpiDevice _spiDevice;
        private readonly GpioController _gpioController;
        private readonly object _syncObj = new object();

        private class SpiComm : ISpiComm
        {
            private readonly SpiDevice _spiDevice;
            private readonly GpioPin _chipSelectGpioPin;
            private readonly object _syncObj;

            public SpiComm(SpiDevice spiDevice, GpioPin chipSelectGpioPin, object syncObj)
            {
                _spiDevice = spiDevice;
                _chipSelectGpioPin = chipSelectGpioPin;
                _syncObj = syncObj;

                _chipSelectGpioPin.Write(GpioPinValue.High);
            }

            public void Dispose()
            {
                _chipSelectGpioPin.Dispose();
            }

            public void Use(Action<SpiDevice> spiAction)
            {
                lock (_syncObj)
                {
                    _chipSelectGpioPin.Write(GpioPinValue.Low);
                    spiAction(_spiDevice);
                    _chipSelectGpioPin.Write(GpioPinValue.High);
                }
            }
        }

        public SpiCommFactory(SpiDevice spiDevice, GpioController gpioController)
        {
            _spiDevice = spiDevice;
            _gpioController = gpioController;
        }

        public void Dispose()
        {
            _spiDevice.Dispose();
        }

        public ISpiComm Create(int chipSelectPinNumber)
        {
            GpioPin chipSelectGpioPin = _gpioController.OpenPin(chipSelectPinNumber);
            chipSelectGpioPin.SetDriveMode(GpioPinDriveMode.Output);
            return new SpiComm(_spiDevice, chipSelectGpioPin, _syncObj);
        }
    }
}