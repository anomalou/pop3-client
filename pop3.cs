using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace pop3_client_csharp{
    class POP3{
        string hostname;
        int port;
        string username;

        Socket socket;

        Encoding UTF8;
        public POP3(string hostname, int port){
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.hostname = hostname;
            this.port = port;
            UTF8 = Encoding.UTF8;
        }

        public void Login(string username, string password){
            this.username = username;
            socket.Connect(hostname, port);
            if(!socket.Connected){
                Console.WriteLine("Error! Cannot connect to server!");
                return;
            }

            string[] answer;

            Console.WriteLine("\n---Connected!---\n");
            answer = GetAnswer().Split(' ', 2);
            Console.WriteLine($"{answer[0]} {answer[1]}");
            if(answer[0] != "+OK")
                return;
            socket.Send(GetByte("USER " + username + "\r\n"));
            answer = GetAnswer().Split(' ', 2);
            Console.WriteLine($"{answer[0]} {answer[1]}");
            if(answer[0] != "+OK")
                return;
            socket.Send(GetByte("PASS " + password + "\r\n"));
            answer = GetAnswer().Split(' ', 2);
            Console.WriteLine($"{answer[0]} {answer[1]}");
            if(answer[0] != "+OK")
                return;

            Console.WriteLine($"Hello {username}!\n\n");
            
            bool work = true;
            string input;

            string[] command;
            
            while(work){
                Console.Write(">");
                input = Console.ReadLine();
                command = input.Split(' ', 3);
                switch(command[0]){
                    case "getmsg":
                        if(command.Length > 1){
                            GetMessage(int.Parse(command[1]));
                        }else{
                            Console.WriteLine("Too few arguments!");
                        }
                    break;
                    case "del":
                        if(command.Length > 1){
                            Delete(int.Parse(command[1]));
                        }else{
                            Console.WriteLine("Too few arguments!");
                        }
                    break;
                    case "test":
                        Noop();
                    break;
                    case "quit":
                        Quit();
                        work = false;
                    break;
                    case "stat":
                        GetStat();
                    break;
                    case "list":
                        if(command.Length == 1){
                            ListMessage();
                        }else if(command.Length > 1){
                            ListMessage(int.Parse(command[1]));
                        }else{
                            Console.WriteLine("Check number of your arguments!");
                        }
                    break;
                    case "reset":
                        Reset();
                    break;
                    case "top":
                        if(command.Length > 2){
                            Top(int.Parse(command[1]), int.Parse(command[2]));
                        }else{
                            Console.WriteLine("Too few arguments!");
                        }
                    break;
                    case "help":
                        Console.WriteLine("Awilable commands:\ngetmsg <i>\ndel <i>\ntest\nquit\nstat\nlist\nlist <i>\nreset\ntop <i> <t>\nclear\n");
                    break;
                    case "clear":
                        Console.Clear();
                    break;
                    default:
                        if(input != "")
                            Console.WriteLine("I don't know that command! Try use \"help\" for list of all commands.");
                    break;
                }
            }
        }

        string GetAnswer(){
            return GetAnswer(256);
        }

        string GetAnswer(int size){
            Byte[] storage = new Byte[size];
            if(socket.Connected){
                socket.Receive(storage);
                return GetString(storage); 
            }
            return "";
        }

        void SendCommand(string command){
            if(socket.Connected){
                socket.Send(GetByte(command + "\r\n"));
            }
        }

        void GetMessage(int i){
            if(socket.Connected){
                string[] answer;
                SendCommand($"LIST {i}");
                answer = GetAnswer().Split(' ', 3);
                if(answer[0] == "+OK"){
                    SendCommand($"RETR {i}");
                    answer = GetAnswer(int.Parse(answer[2]) + 100).Split(' ', 2);
                    if(answer[0] == "+OK"){
                        Console.WriteLine(answer[1]);
                    }else
                        Console.WriteLine($"{answer[0]} {answer[1]}");
                }else
                    Console.WriteLine($"{answer[0]} {answer[1]} {answer[2]}");
            }
        }

        void ListMessage(){
            if(socket.Connected){
                SendCommand("LIST");
                Console.WriteLine(GetAnswer(10000));
            }
        }

        void ListMessage(int number){
            if(socket.Connected){
                SendCommand($"LIST {number}");
                Console.WriteLine(GetAnswer());
            }
        }

        void Top(int msg, int num){
            if(socket.Connected){
                string[] answer;
                SendCommand($"LIST {msg}");
                answer = GetAnswer().Split(' ', 3);
                if(answer[0] == "+OK"){
                    SendCommand($"TOP {msg} {num}");
                    Console.WriteLine(GetAnswer(int.Parse(answer[2]) + 100));
                }else
                    Console.WriteLine($"{answer[0]} {answer[1]} {answer[2]}");
            }

        }

        void GetStat(){
            if(socket.Connected){
                SendCommand("STAT");
                Console.WriteLine(GetAnswer());
            }
        }

        void Noop(){
            if(socket.Connected){
                SendCommand("NOOP");
                Console.WriteLine(GetAnswer());
            }
        }

        void Reset(){
            if(socket.Connected){
                SendCommand("RSET");
                Console.WriteLine(GetAnswer());
            }
        }

        void Quit(){
            if(socket.Connected){
                SendCommand("QUIT");
                Console.WriteLine(GetAnswer());
            }
        }

        void Delete(int msg){
            if(socket.Connected){
                SendCommand($"DELE {msg}");
                Console.WriteLine(GetAnswer());
            }
        }

        Byte[] GetByte(string msg){
            return UTF8.GetBytes(msg);
        }

        string GetString(Byte[] bytes){
            return UTF8.GetString(bytes);
        }
    }
}