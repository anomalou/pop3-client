using System;

namespace pop3_client_csharp
{
    class Program
    {
        static POP3 pop3;
        static void Main(string[] args)
        {
            pop3 = new POP3("domain", 110);
            pop3.Login("", ""); //login password
        }
    }
}
