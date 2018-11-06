using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;
using System.Threading;

namespace ConsoleServer
{
    public class RRServer
    {
        private bool isStop = false;

        public void Start()
        {
            Console.WriteLine("Started..");
            Thread t = new Thread(new ThreadStart(delegate()
                {
                    using (var context = new Context(1))
                    {
                        using (Socket clients = context.Socket(SocketType.XREQ/*ROUTER*/), workers = context.Socket(SocketType.XREQ/*DEALER*/))
                        {
                            clients.Bind("tcp://*:5560");
                            workers.Bind("inproc://workers"); // FYI, inproc requires that bind is performed before connect

                            var workerThreads = new Thread[5];
                            for (int threadId = 0; threadId < workerThreads.Length; threadId++)
                            {
                                workerThreads[threadId] = new Thread(WorkerRoutine);
                                workerThreads[threadId].Start(context);
                            }

                            //  Connect work threads to client threads via a queue
                            //  Devices will be depricated from 3.x
                            Socket.Device.Queue(clients, workers);
                            //这是应答模式的代理，官方提供了三种标准代理： 
                            //应答模式：queue XREP/XREQ 
                            //订阅模式：forwarder SUB/PUB 
                            //分包模式：streamer PULL/PUSH 
                        }
                    }
                }
            ));
            t.Start();
        }

        private void WorkerRoutine(object context)
        {
            Socket receiver = ((Context)context).Socket(SocketType.REP);
            receiver.Connect("inproc://workers");

            while (!isStop)
            {
                string message = receiver.Recv(Encoding.UTF8);
                Console.WriteLine("Received request: " + message);
                Thread.Sleep(1000); //  Simulate 'work'
                receiver.Send(string.Format("Reply [{0}] from server at {1}", message, DateTime.Now), Encoding.UTF8);
            }
        }

        public void Stop()
        {
            isStop = true;
            Console.WriteLine("Stoped..");
        }
    }
}
