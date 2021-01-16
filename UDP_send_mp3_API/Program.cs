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
            //Console.WriteLine("Hello World!");

            List<client_IPEndPoint> clientList = new List<client_IPEndPoint>()
            {
                 new client_IPEndPoint(){ ID_client = "20154023", On = true, NumSend = 1},
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
            udpSocket.launchUDPsocket(soundListServer, clientList);
            //create UDP socket listen from client
            udpSocket.UDPsocketListen();
            //create UDP socket for sending mp3 frame to client
            //udpSocket.UDPsocketSend();
            control(udpSocket);
        }

        static void control(UDPsocket udpSocket)
        {
            var statusNow = udpSocket.Status;
            int currentTime = udpSocket.TimePlaying_song_s; //second
            int duration = 0;
            int song_ID = 0; //order of song in soundList 
            Console.Title = "Project truyen thanh!!!";
            Console.OutputEncoding = Encoding.UTF8;
            int cursor = Console.CursorTop;
            Console.WriteLine("Status: {0}", statusNow); //line 2-7
            Console.WriteLine("Song: {0}", song_ID); //line 3-6
            Console.WriteLine("Current time play: "); //line 4-19
            Console.WriteLine("Duration: 0:0"); //line 5-10
            
            Console.WriteLine("Lệnh:");
            Console.WriteLine(" 1:Play/ Resume");
            Console.WriteLine(" 2:Pause");
            Console.WriteLine(" 3:Next");
            Console.WriteLine(" 4:Previous");
            Console.WriteLine(" 5:Stop");
            Console.Write("Nhập số tương ứng để tiến hành điều khiển: "); //line 10


            //thread update status every 1s
            Thread displayStatus = new Thread(() =>
            {
                while (true)
                {
                    if (statusNow != udpSocket.Status)
                    {
                        statusNow = udpSocket.Status;
                        Console.SetCursorPosition(8, cursor);
                        Console.Write(statusNow + "    ");
                    }
                    if (song_ID != udpSocket.SongID)
                    {
                        song_ID = udpSocket.SongID;
                        Console.SetCursorPosition(6, cursor + 1);
                        Console.Write(song_ID);
                    }

                    currentTime = udpSocket.TimePlaying_song_s;
                    Console.SetCursorPosition(19, cursor+2);
                    Console.Write("{0,2}:{1,2}", currentTime / 60, currentTime % 60);

                    if (duration != udpSocket.Duration_song_s)
                    {
                        duration = udpSocket.Duration_song_s;
                        Console.SetCursorPosition(10, cursor+3);
                        Console.Write("{0,2}:{1,2}", duration / 60, duration % 60);
                    }
                    //Console.SetCursorPosition(43, 9);
                    Thread.Sleep(1000);
                }
            });
            displayStatus.Priority = ThreadPriority.BelowNormal;
            displayStatus.Start();


            //thread control for test
            Thread readControl = new Thread(() =>
            {
                while (true)
                {
                    //Console.SetCursorPosition(43, 9);
                    var control = Console.ReadKey(true);
                    switch (control.KeyChar)
                    {
                        case '1': //play/resume
                            //
                            if(statusNow == UDPsocket.status_enum.STOP) //play
                            {
                                udpSocket.UDPsocketSend();
                            }
                            else if(statusNow == UDPsocket.status_enum.PAUSE) //resume
                            {
                                udpSocket.controlThreadSend(2);//resume
                            }
                            break;
                        case '2': //pause
                            //
                            if (statusNow == UDPsocket.status_enum.PLAY) //play
                            {
                                udpSocket.controlThreadSend(1);//pause
                            }
                            break;
                        case '3': //next
                            if (statusNow != UDPsocket.status_enum.STOP) 
                            {
                                udpSocket.controlThreadSend(3);
                            }
                            break;
                        case '4': //previous
                            if (statusNow != UDPsocket.status_enum.STOP) 
                            {
                                udpSocket.controlThreadSend(4);
                            }
                            break;
                        case '5': //stop
                            //
                            if (statusNow != UDPsocket.status_enum.STOP)
                            {
                                udpSocket.controlThreadSend(5);//stop
                            }
                            break;
                    }
                }
            });
            readControl.Priority = ThreadPriority.Lowest;
            readControl.Start();          
        }
    }
}
