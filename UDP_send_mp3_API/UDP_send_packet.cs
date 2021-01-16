#define DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using System.IO;

namespace UDP_send_packet_frame
{
    class UDPsocket
    {
        static bool left_frame_not_packet = false;
        static int sizeOfPacket;

        bool loopBack = true;
        public bool LoopBack { get => loopBack; set => loopBack = value; }
        

        //2 thread
        Thread threadListen, threadSend, threadCheckRequest;

        //socket UDP
        static IPAddress localIp = IPAddress.Any;
        static int localPort = 1308;
        IPEndPoint localEndPoint = new IPEndPoint(localIp, localPort);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //for threadListen
        byte[] receive_buffer = new byte[8];
        // biến này về sau sẽ chứa địa chỉ của tiến trình client nào gửi gói tin tới
        EndPoint receive_IPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        //List save client request
        List<client_IPEndPoint> clientList = new List<client_IPEndPoint>();
        public List<client_IPEndPoint> ClientList { get => clientList; set => clientList = value; }

        //save duration and time playing of current song
        int duration_song_s = 0;
        int timePlaying_song_s = 0;
        int songID = 1;
        public int Duration_song_s { get => duration_song_s; }
        public int TimePlaying_song_s { get => timePlaying_song_s; }
        public int SongID { get => songID;}

        //for threadSend
        const int Max_send_buff_length = 1472;
        byte[] sendBuffer = new byte[Max_send_buff_length];
        //List soundTrack
        List<soundTrack> soundList = new List<soundTrack>();
        public List<soundTrack> SoundList { get => soundList; set => soundList = value; }
        

        //status to set up: play, pause, stop
        public enum status_enum { PLAY, PAUSE, STOP };
        status_enum status = status_enum.STOP;
        public status_enum Status { get => status; }
        

        public bool launchUDPsocket(List<soundTrack> _soundList, List<client_IPEndPoint> _clientList)
        {
            soundList = _soundList;
            clientList = _clientList;

            try
            {
                socket.Bind(localEndPoint);
                Console.WriteLine($"Local socket bind to {localEndPoint}. Waiting for request ...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        public void UDPsocketListen()
        {
            Stopwatch watch_client = new Stopwatch();
            watch_client.Start();

            threadListen = new Thread(() =>
            {
                threadListenFunc(watch_client);
            });

            threadCheckRequest = new Thread(() =>
            {
                threadCheckRequestFunc(watch_client);
            });

            threadCheckRequest.Priority = ThreadPriority.Lowest;
            threadListen.Priority = ThreadPriority.Normal;

            threadListen.Start();
            threadCheckRequest.Start();
        }

        public void UDPsocketSend()
        {
            
            threadSend = new Thread(() =>
            {
                threadSendFunc();
            });
            threadSend.Priority = ThreadPriority.Highest;
            threadSend.Start();
        }

        public bool controlThreadSend(int _control)
        {
            //_control: 1      2       3
            //          pause, resume, stop
            if( (_control == 1)  && (status == status_enum.PLAY))
            {

            }
            else if((_control == 2) && (status == status_enum.PAUSE))
            {

            }
            else if((_control == 3) && (status != status_enum.STOP))
            {

            }
            else
            {
                return false;
            }
            return true;
        }

        private void analyzeRequest(int length, Stopwatch _watchClient)
        {
            if (length == 8)
            {
                //test request
                IPEndPoint receive = (IPEndPoint)receive_IPEndPoint;
                Console.WriteLine("This message lentgh:" + length.ToString() + " was sent from " +
                                            receive.Address.ToString() +
                                            " on their port number " + receive.Port.ToString());


                //get id client
                var ID_client_received = Encoding.ASCII.GetString(receive_buffer, 0, 8);

                //check client in List
                for (int i = 0; i < ClientList.Count; i++)
                {
                    if (String.Equals(ID_client_received, ClientList[i].ID_client))
                    {
                        ClientList[i].TimeStamp_ms = _watchClient.ElapsedMilliseconds; //update time request
                        ClientList[i].TimeOut = false;
                        ClientList[i].IPEndPoint_client = receive_IPEndPoint; //update IP, port UDP of client
                    }
                }
            }
        }

        private void threadListenFunc(Stopwatch _watchClient)
        {
            while(true)
            {
                //client need to send ID, string 4 byte
                int length = 0;
                try
                {
                    length = socket.ReceiveFrom(receive_buffer, ref receive_IPEndPoint);
                }
                catch(Exception ex)
                {
                    //Console.WriteLine(ex);
                    continue;
                }

                analyzeRequest(length, _watchClient);
            }
        }

        private void threadCheckRequestFunc(Stopwatch _watchClient)
        {
            while(true)
            {
                for (int i = 0; i < ClientList.Count; i++)
                {
                    double offsetTime = _watchClient.ElapsedMilliseconds - ClientList[i].TimeStamp_ms;
                    if (offsetTime > 5000) // > 5s
                    {
                        ClientList[i].TimeOut = true;
                    }
                }
                Thread.Sleep(5000); //check every 5s
            }
        }

        private void threadSendFunc()
        {
            while(true)
            {
                for(int i = 0; i < soundList.Count; i++)
                {
                    byte[] mp3_buff;
                    try
                    {
                        mp3_buff = File.ReadAllBytes(soundList[i].FilePath);
                    }
                    
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        continue;
                    }

                    sendPacketMP3(mp3_buff, mp3_buff.Length);
                }
                if(!loopBack)
                {
                    break;
                }
            }
        }

        private bool sendPacketMP3(byte[] mp3_buff, int mp3_buff_length)
        {
            //launch timer
            var stopWatch = new Stopwatch();
            
            double timePoint, mark_time = 0;           

            var mp3_reader = new MP3_frame(mp3_buff, mp3_buff_length);
            if(!mp3_reader.IsValidMp3())
            {
                return false;
            }
            //const double framemp3_time = (double)1152.0 * 1000.0 / 44100.0; //ms
            double framemp3_time = mp3_reader.TimePerFrame_ms;
            //count total Frame of mp3
            duration_song_s = mp3_reader.countFrame() * (int)mp3_reader.TimePerFrame_ms / 1000;

            int numOfFrame;

            SocketFlags socketFlag = new SocketFlags();

            stopWatch.Start();

            while (true)
            {
                //change status, return value to threadSendFunc
                //value: 1-play-next, 2-play-previous, 3-stop

                numOfFrame = packet_udp_frameMP3(sendBuffer, Max_send_buff_length, mp3_reader);
                if (numOfFrame < 1)
                {
                    break;
                }

                for(int i = 0; i < clientList.Count; i++)
                {
                    if((!clientList[i].TimeOut) && (clientList[i].On))
                    {
                        for (int j = 0; j < clientList[i].NumSend; j++)
                        {
                            try
                            {
                                socket.SendTo(sendBuffer, sizeOfPacket, socketFlag, clientList[i].IPEndPoint_client);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }

                mark_time += framemp3_time * numOfFrame; //point to next time frame
                //get current time playing
                timePlaying_song_s = (int)mark_time / 1000; //second
                timePoint = mark_time - stopWatch.Elapsed.TotalMilliseconds;
                if (timePoint > 0)
                {
                    Thread.Sleep((int)timePoint);
                    
                }
                #if (DEBUG)
                    //Thread.Sleep(1000); //1s
                #endif
            }
            //done a song
            timePlaying_song_s = 0;
            duration_song_s = 0;

            return true;
        }
        static private int packet_udp_frameMP3(byte[] _send_buff, int _max_send_buff_length, MP3_frame _mp3_reader)
        {
            //packet: 4-byte
            int numOfFrame = 0, totalLength = 0;
            while (true)
            {
                if (left_frame_not_packet)
                {
                    left_frame_not_packet = false;
                }
                else if (_mp3_reader.ReadNextFrame())
                {

                }
                else
                {
                    break;
                }
                //check space for cmemcpy //(4+4) for numOfFrame, totalLength
                if (_mp3_reader.Frame_size <= (_max_send_buff_length - (4 + 4) - totalLength))
                {
                    Buffer.BlockCopy(_mp3_reader.Mp3_buff, _mp3_reader.Start_frame, _send_buff, (4 + 4 + totalLength), _mp3_reader.Frame_size);
                    totalLength += _mp3_reader.Frame_size;
                    numOfFrame++;
                }
                else
                {
                    left_frame_not_packet = true;
                    break;
                }
            }

            //copy num of frame 
            byte[] tmp_byte = BitConverter.GetBytes(numOfFrame); //little edian
            Buffer.BlockCopy(tmp_byte, 0, _send_buff, 0, 4);

            //copy total length frame (byte) 
            tmp_byte = BitConverter.GetBytes(totalLength); //little edian
            Buffer.BlockCopy(tmp_byte, 0, _send_buff, 4, 4);

            sizeOfPacket = 8 + totalLength;

            return numOfFrame;
        }
    }

    class client_IPEndPoint
    {
        // biến này về sau sẽ chứa địa chỉ của tiến trình client nào gửi gói tin tới
        EndPoint ipEndPoint_client = new IPEndPoint(IPAddress.Any, 0);

        string id_client;

        public EndPoint IPEndPoint_client { get => ipEndPoint_client; set => ipEndPoint_client = value; }
        public string ID_client { get => id_client; set => id_client = value; }       

        double timeStamp_ms = 0;

        bool timeOut = true; //timeOut = true, that mean don't receive request in last 5s, and don't send

        bool on; //change this on app

        int numSend = 2; //multi packet is sent to client to improve UDP loss
        public int NumSend { get => numSend; set => numSend = value; }

        //server just sends to client when timeOut == false and On == true

        public double TimeStamp_ms { get => timeStamp_ms; set => timeStamp_ms = value; }
        public bool TimeOut { get => timeOut; set => timeOut = value; }
        public bool On { get => on; set => on = value; }
        
    }

    class soundTrack
    {
        string filePath;
        int duration_ms = 0; //duration of a sound Track
        int playingTime_ms = 0; //current time playing of sound track

        public string FilePath { get => filePath; set => filePath = value; }
        public int Duration_ms { get => duration_ms; set => duration_ms = value; }
        public int PlayingTime_ms { get => playingTime_ms; set => playingTime_ms = value; }
    }

    class headerPacket
    {
        ////header of UDP packet
        // 1-byte: ID_song, 4-byte: ID_frame, 1-byte: numOfFrame, 1-byte: volume, 2-byte: totalLength
        //de cho an toan, nen tinh checksum cho header nay

        //total byte in header
        const int header_numOfByte = 7;
        byte volume = 0x00; // max:min 0x00:0xFE

        internal byte Volume
        {
            get { return volume; }
            set
            {
                if (value == 0xFF)
                    volume = 0xFE;
                else
                    volume = value;
            }
        }
    }
}
