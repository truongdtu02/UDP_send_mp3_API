using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UDP_send_packet_frame;

namespace UDP_send_mp3_API
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            List<client_IPEndPoint> clientList = new List<client_IPEndPoint>()
            {
                 new client_IPEndPoint(){ ID_client = "20154023", On = true},
                 new client_IPEndPoint(){ ID_client = "20164023", On = false},
                 new client_IPEndPoint(){ ID_client = "00000001", On = true},
                 new client_IPEndPoint(){ ID_client = "00000002", On = true},
                 new client_IPEndPoint(){ ID_client = "00000003", On = true},
                 new client_IPEndPoint(){ ID_client = "00000004", On = true},
                 new client_IPEndPoint(){ ID_client = "00000005", On = true},
                 new client_IPEndPoint(){ ID_client = "00000006", On = true},
                 new client_IPEndPoint(){ ID_client = "00000007", On = true},
                 new client_IPEndPoint(){ ID_client = "00000008", On = true},
                 new client_IPEndPoint(){ ID_client = "00000009", On = true},
                 new client_IPEndPoint(){ ID_client = "000000010", On = true},
            };

            List<soundTrack> soundList = new List<soundTrack>()
            {
                new soundTrack(){ FilePath = @"E:\truyenthanhproject\read_mp3\duaComChoMeEmDiCay.mp3"},
                new soundTrack(){ FilePath = @"E:\truyenthanhproject\read_mp3\emYeuTruongEm.mp3"},
                new soundTrack(){ FilePath = @"E:\truyenthanhproject\read_mp3\xeDap.mp3"}
            };

            List<soundTrack> soundListServer = new List<soundTrack>()
            {
                new soundTrack(){ FilePath = "duaComChoMeEmDiCay.mp3"},
                new soundTrack(){ FilePath = "emYeuTruongEm.mp3"}
                //new soundTrack(){ FilePath = "LoveIsBlue.mp3"}
            };

            //var mp3_buff = File.ReadAllBytes(soundList[1].FilePath);
            //int mp3_buff_length = mp3_buff.Length;
            //var mp3_reader = new MP3_frame(mp3_buff, mp3_buff_length);
            //mp3_reader.IsValidMp3();
            ////count total Frame of mp3
            //int totalFrame = mp3_reader.countFrame();

            //int x = 0;
            UDPsocket udpSocket = new UDPsocket();
            var _status = udpSocket.Status; //get status (PLAY, PAUSE, STOP)

            //launch
            udpSocket.launchUDPsocket(soundList, clientList);
            //create UDP socket listen from client
            udpSocket.UDPsocketListen();
            //create UDP socket for sending mp3 frame to client
            udpSocket.UDPsocketSend();

            //thread control for test
            Thread readControl = new Thread(() =>
            {
                control(udpSocket);
            });
            readControl.Start();


        }

        static void control(UDPsocket udpSocket)
        {
            var statusNow = udpSocket.Status;
            int currentTime = (int)udpSocket.TimePlaying_song_s; //second
            var duration = udpSocket.Duration_song_s;
            Console.Title = "Project truyen thanh!!!";
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Staus: "); //line 2-7
            Console.WriteLine("Song: "); //line 3-6
            Console.WriteLine("Current time play: "); //line 4-19
            Console.WriteLine("Duration: ");
            //thread update status every 1s
            Thread displayStatus = new Thread(() =>
            {
                if(statusNow != udpSocket.Status)
                {
                    statusNow = udpSocket.Status;
                    Console.SetCursorPosition(7, 2);
                    Console.Write(statusNow);
                }



                currentTime = ((int)udpSocket.TimePlaying_song / 1000);
                Console.SetCursorPosition(7, 2);
                Console.Write("{0}:{1}", currentTime/60, currentTime%60);

            });
            displayStatus.Start();

            Console.WriteLine("Lệnh:");
            Console.WriteLine(" 1:Play/ Resume");
            Console.WriteLine(" 2:Pause");
            Console.WriteLine(" 3:Stop");
            Console.Write("Nhập số tương ứng để tiến hành điều khiển: ");
            while (true)
            {
                var control = Console.ReadKey(true);
                switch(control.KeyChar)
                {
                    case '1':
                        //
                        break;
                    case '2':
                        //
                        break;
                    case '3':
                        //
                        break;
                }
                    
            }
        }
    }
}
