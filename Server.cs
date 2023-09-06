using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using NetFwTypeLib;

namespace Server_GAME
{
    internal class Server
    {
        static void Main()
        {
            // Endereço IP e porta em que o servidor escutará conexões.
            string ipAddress = "127.0.0.1"; // Pode ser "0.0.0.0" para aceitar conexões de qualquer endereço IP.
            int port = 12345;

            // Chama o método para criar uma exceção no firewall.
            CreateFirewallException("NomeDaSuaAplicacao", port, ProtocolType.Tcp);


            // Cria um socket TCP/IP.
            TcpListener serverSocket = new TcpListener(IPAddress.Parse(ipAddress), port);
            serverSocket.Start();
            Console.WriteLine($"Servidor iniciado em {ipAddress}:{port}");

            while (true)
            {
                Console.WriteLine("Aguardando conexões...");
                TcpClient clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Cliente conectado.");

                // Lida com a conexão do cliente em uma nova thread.
                _ = new ClientHandler(clientSocket);
            }
        }

        private static void CreateFirewallException(string applicationName, int port, ProtocolType protocol)
        {
            try
            {
                Type type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(type);
                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));

                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.Enabled = true;
                firewallRule.ApplicationName = applicationName;
                firewallRule.Description = "Permite conexões para a aplicação";
                firewallRule.Protocol = (int)protocol;

                if (protocol == ProtocolType.Tcp)
                {
                    firewallRule.LocalPorts = port.ToString();
                }
                else if (protocol == ProtocolType.Udp)
                {
                    firewallRule.LocalPorts = port.ToString();
                }

                fwPolicy2.Rules.Add(firewallRule);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar exceção de firewall: {ex}");
            }
        }
    }

    class ClientHandler
    {
        private TcpClient clientSocket;
        private NetworkStream networkStream;

        public ClientHandler(TcpClient clientSocket)
        {
            this.clientSocket = clientSocket;
            networkStream = clientSocket.GetStream();
            Start();


            void Start()
            {
                byte[] bytesFrom = new byte[1024];
                string dataFromClient = null;

                while (true)
                {
                    try
                    {
                        int bytesRead = networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                        if (bytesRead <= 0)
                        {
                            Console.WriteLine("Cliente desconectado.");
                            break;
                        }

                        dataFromClient = Encoding.ASCII.GetString(bytesFrom, 0, bytesRead);
                        Console.WriteLine($"Mensagem do cliente: {dataFromClient}");

                        // Aqui, você pode processar os pacotes do jogo e enviar respostas ao cliente, de acordo com a lógica do seu jogo.
                        // Para simplicidade, estamos apenas ecoando a mensagem de volta para o cliente.

                        byte[] bytesToSend = Encoding.ASCII.GetBytes(dataFromClient);
                        networkStream.Write(bytesToSend, 0, bytesToSend.Length);
                        networkStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro: {ex.ToString()}");
                    }
                }

                networkStream.Close();
                clientSocket.Close();
            }
        }

    }
}
