using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using NLog;
using System.Net.Sockets;

namespace ProtChecked
{
    class Program
    {
        public static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.WriteLine("Yazılan porta değerine göre portun boş olup olmadığını kontrol eder.");
            Console.WriteLine("Lütfen bir port numarası girip enter tuşuna basın.");
            string sPort = Console.ReadLine();
            logger.Trace($"Sorgulanması için girilen Port değeri {sPort} 'tur");
            int nPort = 0;
            logger.Trace("Port sorgulama girilen değerin sayıya çevrilmesi sonrasında yapılacaktır.");
            if (int.TryParse(sPort, out nPort))
            {
                logger.Trace("Girilen değer sayıya çevcrildi.");
                Console.WriteLine($"{sPort} portu kontrol ediliyor...");
                logger.Trace($"{sPort} portu için sorgulama yapılıyor...");
                if (CheckAvailableServerPort(nPort))
                {
                    Console.WriteLine("Port açık");
                    logger.Trace("Port açık");
                    Console.WriteLine("Port açma işlemleri başlatıldı");
                    if (ConnectPort(nPort))
                    {
                        Console.WriteLine("Port açma başarılı !!!");
                        logger.Trace("Port açma başarılı !!!");
                    }
                    else
                    {
                        Console.WriteLine("Port açma BAŞARISIZ hata log dosyasını okuyun !!!");
                        logger.Trace("Port açma BAŞARISIZ hata log dosyasını okuyun !!!");
                    }
                }
                else
                {
                    Console.WriteLine("Port KAPALI !!!");
                    logger.Trace("Port KAPALI !!!");
                }
            }
            else
            {
                Console.WriteLine("Girilen değer sayıya çevrilecek bir ifade değil !!! Bu yüzden port sorgulama İPTAL edildi.");
                logger.Trace("Girilen değer sayıya çevrilecek bir ifade değil !!! Bu yüzden port sorgulama İPTAL edildi.");
            }
            Console.WriteLine("İşlemi sonllandirmak için bir tuşa basın.");
            Console.ReadKey();
        }

        /// <summary>
        /// Mevcut Portun kullanım durumuna bakılıyor...
        /// </summary>
        /// <param name="port">Port Numarası</param>
        /// <returns>True/False</returns>
        public static bool CheckAvailableServerPort(int port)
        {
            bool isAvailable = true;
           
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();
            logger.Trace("Makine bilgileri alınıyor...");
            foreach (IPEndPoint endpoint in tcpConnInfoArray)
            {
                logger.Trace($"{endpoint.Address.ToString()} IP adresindeki {endpoint.Port.ToString()} portu bilgisayar tarafından kullanılıyor.");
                if (endpoint.Port == port)
                {
                    isAvailable = false;
                    logger.Trace("25600 portu kullanılan liste arasında !!!");
                    break;
                }
            }

            return isAvailable;
        }

        /// <summary>
        /// Port açılma işlemi yapılıyor...
        /// </summary>
        /// <param name="port">Açılacak olan Port numarası</param>
        /// <returns>True/False</returns>
        public static bool ConnectPort(int port)
        {
            bool tsOk = true;
            TcpListener tListener = default(TcpListener);
            //IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            IPAddress ipAddress = IPAddress.Parse(GetIPAddress(Dns.GetHostName()));
            logger.Trace($"{ipAddress.ToString()} için 25600 portu açılıyor");
            try
            {
                tListener = new TcpListener(ipAddress, port);
                tListener.Start();
                logger.Trace("Poprt açımı gerçekleşti.");
            }
            catch (SocketException ex)
            {
                logger.Error("Soket açma hatası !!! İlgili Hata ->", ex);
                tsOk = false;
            }
            finally
            {
                logger.Trace("Port kapatılıyor...");
                if (tListener != null)
                    tListener.Stop();
            }
            return tsOk;
        }

        /// <summary>
        /// HostName bilgisi verilen makinenin sahip olduğu IP Adresivi V4 standartına göre döner.
        /// </summary>
        /// <param name="hostname">Makine Adı</param>
        /// <returns>IP Adresi</returns>
        public static string GetIPAddress(string hostname)
        {
            logger.Trace($"{hostname} için IP adresi bulunuyor...");
            IPHostEntry host;
            host = Dns.GetHostEntry(hostname);

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    logger.Trace($"{hostname} için bulunan IP adresi {ip.ToString()} 'tir.");
                    return ip.ToString();
                }
            }
            return string.Empty;
        }
    }
}
