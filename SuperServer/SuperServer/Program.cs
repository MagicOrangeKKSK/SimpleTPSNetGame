using SuperServer.Core;
using SuperServer.Logic;
using SuperServer.SceneLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Hello World");
            DataMgr dataMgr = new DataMgr();
            ServNet ser = new ServNet();
            ser.proto = new ProtocolBytes();
            ser.Start("127.0.0.1", 1234);
            Scene scene = new Scene();

            Player player = new Player("Lee", null);
            player.data = new PlayerData();
            player.data.score = 100;

            DataMgr.instance.SavePlayer(player);

            while (true)
            {
                string str = Console.ReadLine();
                switch (str)
                {
                    case "quit":
                        ser.Close();
                        return;
                    case "print":
                        ser.Print();
                        break;
                }
            }


            
        }
    }
}
