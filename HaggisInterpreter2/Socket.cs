using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace HaggisInterpreter2
{
    public class HSocket
    {
        public bool canBegin { get; private set; }

        IPAddress ip;
        IPEndPoint endPoint;
        bool isTCP;

        string forcedIPused;

        Socket listener;
        Socket client;

        Queue<string> messages;
        public HSocket(string forceIP = null, int forceEP = -1, bool useTCP = true)
        {
            this.ip = IPAddress.Parse( (forceIP is null) ? "127.0.0.1" : forceIP );
            this.forcedIPused = (forceIP is null) ? null : forceIP;
            /*           
                        H	a	g	g	i	s
                        72	97	130	130	105	115
                
                        = 595‬    
            
            You know, I am a programmer myself. I'm am very smart :3

            */

            int port = 595;

            if (forceEP > -1)
                port = forceEP;

            this.isTCP = useTCP;

            this.endPoint = new IPEndPoint(ip, port);

            this.canBegin = false;

            this.messages = new Queue<string>(1);

        }

        public void BootServer()
        {
            listener = new Socket(this.ip.AddressFamily, SocketType.Stream, (this.isTCP) ? ProtocolType.Tcp : ProtocolType.Udp);

            try
            {
                listener.Bind(this.endPoint);

                listener.Listen(1);


                Console.WriteLine("Waiting connection ... ");

                // Suspend while waiting for 
                // incoming connection Using  
                // Accept() method the server  
                // will accept connection of client 
                client = listener.Accept();
                this.canBegin = true;

                Console.WriteLine("RUNNING AS CLIENT IS HERE!");
                   
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void QueueMessage(string title, string data) { if(canBegin) { messages.Enqueue($"[{title}]{data}"); } }

        public string GetServerDetails()
        {
            string _ip = (forcedIPused is null) ? "127.0.0.1" : this.forcedIPused;
            string isTcp = (isTCP) ? "TCP" : "UDP or Other";
            return $"{_ip}|{endPoint.Port}|{isTcp}";
        }

        public void Shutdown()
        {
            if (canBegin)
            {
                if(messages.Count > 0)
                {
                    string message = "";
                    byte[] buffer;
                    while(messages.Count > 0)
                    {
                        message = messages.Dequeue();
                        buffer = Encoding.ASCII.GetBytes(message);
                        client.Send(buffer);
                        System.Threading.Thread.Sleep(100);
                    }
                }

                client.Shutdown(SocketShutdown.Both);
                client.Close();
                listener.Close();
            }
        }
    }
}
