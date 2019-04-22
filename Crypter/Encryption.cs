/* EasyEncrypt
 * 
 * Copyright (c) 2019 henkje
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 *          -https://github.com/GHenkje/EasyEncrypt/blob/master/LICENSE-
 */

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace EasyEncrypt
{
    public class Encryption
    {
        /// <summary>
        /// Algorithm for encryption and decryption data.
        /// </summary> 
        private SymmetricAlgorithm algorithm;

        /// <summary>
        /// Create class with an already set up algorithm.
        /// </summary>
        /// <param name="algorithm">Algorithm wich is alreay set up properly</param>
        public Encryption(SymmetricAlgorithm algorithm)
            => this.algorithm = algorithm ?? throw new ArgumentException("Invalid algorithm, algorithm is null.");
        /// <summary>
        /// Create class with an algorithm and overide the key for the selected algorithm.
        /// </summary>
        /// <param name="algorithm">New algorithm</param>
        /// <param name="key">Generated key</param>
        public Encryption(SymmetricAlgorithm algorithm, byte[] key)
        {
            this.algorithm = algorithm ?? throw new ArgumentException("Invalid algorithm, algorithm is null.");
            if (key == null) throw new ArgumentException("Invalid key, key is null.");
            if (!this.algorithm.ValidKeySize(key.Length * 8)) throw new ArgumentException("Invalid key, key has an invalid size.");
            this.algorithm.Key = key;
        }
        /// <summary>
        /// Create class and create a new key for the passed algorithm.
        /// </summary>
        /// <param name="algorithm">New algorithm</param>
        /// <param name="password">Password, used to generate key</param>
        /// <param name="salt">Salt, used to make generated key more random(min 8 characters)</param>
        /// <param name="iterations">Rounds PBKDF2 will make to genarete a key</param>
        public Encryption(SymmetricAlgorithm algorithm, string password, string salt, int iterations = 10000)
        {
            this.algorithm = algorithm ?? throw new ArgumentException("Invalid algorithm, algorithm is null.");
            this.algorithm.Key = CreateKey(this.algorithm, password, salt, iterations);
        }
        /// <summary>
        /// Create class and create a new key for the passed algorithm with a fixed keysize.
        /// </summary>
        /// <param name="algorithm">new algorithm</param>
        /// <param name="keySize">Keysize in bits(8 bits = 1 byte)</param>
        /// <param name="password">Password, used to generate key</param>
        /// <param name="salt">Salt, used to make generated key more random(min 8 characters)</param>
        /// <param name="iterations">Rounds PBKDF2 will make to genarete a key</param>
        public Encryption(SymmetricAlgorithm algorithm, int keySize, string password, string salt, int iterations = 10000)
        {
            this.algorithm = algorithm ?? throw new ArgumentException("Invalid algorithm, algorithm is null.");
            if (!this.algorithm.ValidKeySize(keySize)) throw new ArgumentException("Invalid key, key has an invalid size.");
            this.algorithm.Key = CreateKey(keySize, password, salt, iterations);
        }

        /// <summary>
        /// Encrypt a string and decode with UTF8.
        /// </summary>
        /// <param name="text">Text to encrypt</param>
        /// <returns>Encrypted text(base64 string)</returns>
        public string Encrypt(string text)
            => Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(text ?? throw new ArgumentException("Could not decrypt text: Text can't be null."))));
        /// <summary>
        /// Encrypt a string.
        /// </summary>
        /// <param name="text">Text to encrypt</param>
        /// <param name="encoder">Encoding used to convert string to byte[]</param>
        /// <returns>Encrypted text(base64 string)</returns>
        public string Encrypt(string text, Encoding encoder)
            => Convert.ToBase64String(Encrypt(encoder.GetBytes(text ?? throw new ArgumentException("Could not decrypt text: Text can't be null."))));
        /// <summary>
        /// Encrypt a byte[].
        /// </summary>
        /// <param name="data">Data to encrypt</param>
        /// <returns>Encrypted data(byte[])</returns>
        public byte[] Encrypt(byte[] data)
        {
            if (data == null) throw new Exception("Could not encrypt data: Data can't be null.");

            algorithm.GenerateIV();//Genarate new random IV.

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(algorithm.IV, 0, algorithm.IV.Length);//Write IV to ms(first 16 bytes)
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateEncryptor(algorithm.Key, algorithm.IV), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                }
                return ms.ToArray();
            }
        }

        /*static members*/
        /// <summary>
        /// Create a new encryption key with PBKDF2 and a selected keysize.
        /// </summary>
        /// <param name="keySize">Keysize in bits(8 bits = 1 byte)</param>
        /// <param name="password">Password, used to generate key</param>
        /// <param name="salt">Salt, used to make generated key more random(min 8 characters)</param>
        /// <param name="iterations">Rounds PBKDF2 will make to genarete a key</param>
        /// <returns>Encryption key</returns>
        public static byte[] CreateKey(int keySize, string password, string salt, int iterations = 10000)
        {
            if (keySize <= 0) throw new ArgumentException("Could not create key: Invalid KeySize.");
            else if (salt.Length < 8) throw new ArgumentException("Could not create key: Salt is to short.");
            else if (string.IsNullOrEmpty(password)) throw new ArgumentException("Could not create key: Password can't be empty.");
            else if (iterations <= 0) throw new ArgumentException("Could not create key: Invalid Iterations count.");

            return new Rfc2898DeriveBytes(password,
                Encoding.UTF8.GetBytes(salt),//Convert salt to byte[]
                iterations).GetBytes(keySize / 8);
        }
        /// <summary>
        /// Create a new encryption key with PBKDF2 for the selected Algorithm.
        /// </summary>
        /// <param name="algorithm">Algorithm is used to get the keysize</param>
        /// <param name="password">Password, used to generate key</param>
        /// <param name="salt">Salt, used to make generated key more random(min 8 characters)</param>
        /// <param name="iterations">Rounds PBKDF2 will make to genarete a key</param>
        /// <returns>Encryption key</returns>
        public static byte[] CreateKey(SymmetricAlgorithm algorithm, string password, string salt, int iterations = 10000)
            => CreateKey(algorithm.LegalKeySizes[0].MaxSize, password, salt, iterations);
    }
}
