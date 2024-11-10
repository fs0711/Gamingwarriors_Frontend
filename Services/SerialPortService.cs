﻿using System;
using System.IO.Ports;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using static POS.Pages.configModel;

namespace POS.Services
{
    public class SerialPortService : IDisposable
    {
        private readonly SerialPort _port;
        private readonly ILogger<SerialPortService> _logger;
        private readonly ConcurrentQueue<string> _dataQueue = new ConcurrentQueue<string>();

        public SerialPortService(ILogger<SerialPortService> logger)
        {
            _logger = logger;
            _port = new SerialPort("COM20", 9600, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _port.DataReceived += Port_DataReceived;
            _port.Open();
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _port.ReadExisting();
                _dataQueue.Enqueue(data);
                _logger.LogInformation($"Data Received: {data}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from serial port.");
            }
        }

        public string GetLatestData()
        {
            return _dataQueue.TryDequeue(out var data) ? data : null;
        }

        public void Dispose()
        {
            if (_port != null)
            {
                _port.DataReceived -= Port_DataReceived;
                if (_port.IsOpen) _port.Close();
                _port.Dispose();
            }
        }
    }
}
