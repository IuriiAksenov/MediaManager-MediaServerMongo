// Copyright (C) 2017 Dmitriy Belkin 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FileTransferServiceNS;
using FileTransferServiceNS.Contracts;
using Logger;
using Media;
using MediaMongo;
using MediaSQL;

namespace FileTransferServiceConsoleHost
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Log.InitLogger("FileTransferServiceLog");



            var hostName = Dns.GetHostName();
            Console.WriteLine($"Host: {hostName}");

            var hostUri = Dns.GetHostEntry(hostName).AddressList
                .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .Select(hostIp => new Uri($"net.tcp://{hostIp}/ImageService"))
                .First();

            Console.WriteLine($"Service endpoint: {hostUri}");

            // Dmitry Belkin TODO: In config
            string connectionString =
                @"Data Source=mkr0009;Initial Catalog=Media; User ID=db_user;Password=Qwerty_123; Integrated Security=False";

            string connectionMongoString =
                @"mongodb://mkr0007:27017/PhotoStore";

            Log.Instance.LogAsInfo($"{nameof(FileTransferServiceConsoleHost)}.{nameof(Program)}.{nameof(Main)}: Creating {nameof(FileTransferService)}");
            var service = new FileTransferService(
                new ImageDbFileSource(
                    new UnitOfWorkPhotoStorageMongo(connectionMongoString), 
                    new UnitOfWorkPhotoStorageSQL(connectionString)));
            Log.Instance.LogAsInfo($"{nameof(FileTransferServiceConsoleHost)}.{nameof(Program)}.{nameof(Main)}: Creating {nameof(FileTransferService)} is ended");

            Log.Instance.LogAsInfo($"{nameof(FileTransferServiceConsoleHost)}.{nameof(Program)}.{nameof(Main)}: Creating {nameof(FileTransferService)}");
            var serviceHost = new ServiceHost(service, hostUri);
            Log.Instance.LogAsInfo($"{nameof(FileTransferServiceConsoleHost)}.{nameof(Program)}.{nameof(Main)}: Creating {nameof(FileTransferService)} is ended");

            serviceHost.Description.Behaviors.Add(new ServiceMetadataBehavior());

            serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;


            serviceHost.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
            //serviceHost.CloseTimeout = TimeSpan.MaxValue;
            //serviceHost.OpenTimeout = TimeSpan.MaxValue;


            var httpb = new NetTcpBinding()
            {
                TransferMode = TransferMode.Streamed,
                MaxReceivedMessageSize = 2147483647,//4294967295,//2147483647,
                MaxBufferSize = 2147483647,
                SendTimeout =  new TimeSpan(4,01,0),
                CloseTimeout = new TimeSpan(4, 01, 0),
                OpenTimeout = new TimeSpan(4, 01, 0),
                ReceiveTimeout = new TimeSpan(4, 01, 0),
                ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                {
                    MaxDepth = 2147483647,
                    MaxStringContentLength = 2147483647,
                    MaxArrayLength = 2147483647,
                    MaxNameTableCharCount = 2147483647,
                    MaxBytesPerRead = 2147483647
                }
            };

            var tcpb2 = new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue
            };

            serviceHost.AddServiceEndpoint(typeof(IFileTransferService), httpb, "ImageService");
            serviceHost.AddServiceEndpoint(typeof(IFileTransferServiceV2), tcpb2, "ImageService2");
            Console.WriteLine("Initialization complete");

            Console.WriteLine("Open service");
            using (serviceHost)
            {
                Log.Instance.LogAsInfo($"{nameof(FileTransferServiceConsoleHost)}.{nameof(Program)}.{nameof(Main)}: Open ServiceHost...");
                serviceHost.Open();

                string cmd;
                do
                {
                    Console.Write('>');
                    cmd = Console.ReadLine();
                    if (cmd == "copy")
                    {
                        Clipboard.SetText(hostUri.ToString());
                    }
                    else if (cmd == "client")
                    {
                        System.Diagnostics.Process.Start("C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\IDE\\wcftestclient.exe");
                    }
                } while (cmd != "exit");

                serviceHost.Close();
                Console.WriteLine("Service closed");
                Log.Instance.LogAsInfo($"{nameof(FileTransferServiceConsoleHost)}.{nameof(Program)}.{nameof(Main)}: ServiceHost is closed.");
            }

            //  Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(FileTransferServiceConsoleHost)}.{nameof(Program)}.{nameof(Main)}: Open ServiceHost");
            // Console.WriteLine(e);

        }
    }
}