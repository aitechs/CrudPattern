namespace AiTech.LiteOrm.Encryption
{
    public class Password
    {
        static SimpleAES Generator = new SimpleAES();

        public static string Encrypt(string str)
        {
            return Generator.Encrypt(str);
        }

        public static string Decrypt(string str)
        {
            return Generator.Decrypt(str);
        }

    }

}
