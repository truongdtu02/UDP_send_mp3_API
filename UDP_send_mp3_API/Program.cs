using System;
using System.Collections.Generic;
using System.IO;
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
                 new client_IPEndPoint(){ ID_client = "20164023", On = false}
            };

            List<soundTrack> soundList = new List<soundTrack>()
            {
                new soundTrack(){ FilePath = @"E:\truyenthanhproject\read_mp3\Binz.mp3"},
                new soundTrack(){ FilePath = @"E:\truyenthanhproject\read_mp3\LoveIsBlue.mp3"}
            };

            List<soundTrack> soundListServer = new List<soundTrack>()
            {
                new soundTrack(){ FilePath = "Binz.mp3"},
                new soundTrack(){ FilePath = "LoveIsBlue.mp3"}
            };

            //var mp3_buff = File.ReadAllBytes(soundList[1].FilePath);
            //int mp3_buff_length = mp3_buff.Length;
            //var mp3_reader = new MP3_frame(mp3_buff, mp3_buff_length);
            //mp3_reader.IsValidMp3();
            ////count total Frame of mp3
            //int totalFrame = mp3_reader.countFrame();

            //int x = 0;
            UDPsocket udpSocket = new UDPsocket();
            //launch
            udpSocket.launchUDPsocket(soundList, clientList);
            //create UDP socket listen from client
            udpSocket.UDPsocketListen();
            //create UDP socket for sending mp3 frame to client
            udpSocket.UDPsocketSend();
        }
    }
}
