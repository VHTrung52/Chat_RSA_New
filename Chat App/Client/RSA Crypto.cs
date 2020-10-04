using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Client
{
    public class RSA_Crypto
    {
        //Initializes a new instance of the RSACryptoServiceProvider class with a random key pair of the specified key size.
        public static RSACryptoServiceProvider csp = new RSACryptoServiceProvider(1024);
        //RSAParameters _privateKey;
        //RSAParameters _publicKey;

        public RSA_Crypto()
        {
            // _privateKey = csp.ExportParameters(true);
            // _publicKey = csp.ExportParameters(false);

        }
        // Hàm in ra public key
        /*public string PublicKeyString()
        {
            var sw = new StringWriter();
            var xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, _publicKey);
            return sw.ToString();
        }*/

        public byte[] Encrypt(string plaintext, string publicKey)
        {
            /*csp = new RSACryptoServiceProvider();
            csp.ImportParameters(_publicKey);

            var data = Encoding.Unicode.GetBytes(plaintext);
            var cypher = csp.Encrypt(data, false);
            return Convert.ToBase64String(cypher);*/
            var rsaClient = new RSACryptoServiceProvider(1024);
            rsaClient.FromXmlString(publicKey);
            //csp.FromXmlString(publicKey);
            var data = Encoding.UTF8.GetBytes(plaintext);
            var cypherText = rsaClient.Encrypt(data, false);
            return cypherText;
        }
        public string Decrypt(byte[] cypherText)
        {
            /*var dataBytes = Convert.FromBase64String(cypherText);
            csp.ImportParameters(_privateKey);
            var plaintext = csp.Decrypt(dataBytes, false);
            return Encoding.Unicode.GetString(plaintext);*/
            //var data = Encoding.UTF8.GetBytes(cypherText);
            var plainText = csp.Decrypt(cypherText, false);
            return Encoding.UTF8.GetString(plainText);
        }
        public string PublicKeyString()
        {
            //var rsaServer = new RSACryptoServiceProvider(1024);
            //var publicKeyXml = rsaServer.ToXmlString(false);
            //Console.WriteLine(publicKeyXml.ToString());
            var str = csp.ToXmlString(false);
            return str;
        }
    }
}
