/* Crypter
 * Copyright (C) 2019  henkje (henkje@pm.me)
 * 
 * MIT license
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace AesEncryption
{
    public class Encryption
    {
        /// <summary>
        /// Algorithm used for encryption/decryption, contains encryption key.
        /// </summary>
        private SymmetricAlgorithm _Algorithm;

        /// <summary>
        /// Create class and create a new key for the passed algorithm.
        /// </summary>
        /// <param name="Algorithm">new algorithm</param>
        /// <param name="Password">Password, used to generate key</param>
        /// <param name="Salt">Salt, used to make generated key more random</param>
        /// <param name="Iterations">Rounds PBKDF2 will make to genarete a key</param>
        public Encryption(SymmetricAlgorithm Algorithm, string Password, string Salt, int Iterations = 10000)
        {
            _Algorithm = Algorithm ?? throw new Exception("Invalid algorithm, algorithm is null.");
            _Algorithm.Key = CreateKey(_Algorithm, Password, Salt, Iterations);
        }

        /// <summary>
        /// Encrypt a byte[].
        /// </summary>
        /// <param name="Data">Data to encrypt</param>
        /// <returns>Encrypted data(byte[])</returns>
        public byte[] Encrypt(byte[] Data)
        {
            if (Data == null) throw new Exception("Could not encrypt data: data can't be null.");

            _Algorithm.GenerateIV();//Genarate new random IV.

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(_Algorithm.IV, 0, _Algorithm.IV.Length);//Write IV to ms(first 16 bytes)
                using (CryptoStream cs = new CryptoStream(ms, _Algorithm.CreateEncryptor(_Algorithm.Key, _Algorithm.IV), CryptoStreamMode.Write))
                {
                    cs.Write(Data, 0, Data.Length);
                    cs.FlushFinalBlock();
                }
                return ms.ToArray();
            }
        }

        /*static members*/
        /// <summary>
        /// Create a new encryption key with PBKDF2 and a selected keysize.
        /// </summary>
        /// <param name="KeySize">Keysize in bits(8 bits = 1 byte)</param>
        /// <param name="Password">Password, used to generate key</param>
        /// <param name="Salt">Salt, used to make generated key more random</param>
        /// <param name="Iterations">Rounds PBKDF2 will make to genarete a key</param>
        /// <returns>Encryption key</returns>
        public static byte[] CreateKey(int KeySize, string Password, string Salt, int Iterations = 10000)
        {
            if (KeySize <= 0) throw new Exception("Could not create key: invalid KeySize.");
            else if (Salt.Length < 8) throw new Exception("Could not create key: salt is to short.");
            else if (string.IsNullOrEmpty(Password)) throw new Exception("Could not create key: password can't be empty.");
            else if (Iterations <= 0) throw new Exception("Could not create key: invalid Iterations count.");

            return new Rfc2898DeriveBytes(Password,
                Encoding.UTF8.GetBytes(Salt),//Convert salt to byte[]
                Iterations).GetBytes(KeySize / 8);
        }
        /// <summary>
        /// Create a new encryption key with PBKDF2.
        /// The length of the key will be the key length of the selected algorithm.
        /// </summary>
        /// <param name="Algorithm">Algorithm is used to get the keysize</param>
        /// <param name="Password">Password, used to generate key</param>
        /// <param name="Salt">Salt, used to make generated key more random</param>
        /// <param name="Iterations">Rounds PBKDF2 will make to genarete a key</param>
        /// <returns>Encryption key</returns>
        public static byte[] CreateKey(SymmetricAlgorithm Algorithm, string Password, string Salt, int Iterations = 10000)
            => CreateKey(Algorithm.KeySize, Password, Salt, Iterations);
    }
}