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
using System.Reflection;
using AesEncryption;
using System.Security.Cryptography;

namespace Stub1
{
    class Program
    {
        const string SALT = "SALT!@#1(D1423SFNI1234O)#234@)$321#90";

        static void Main(string[] args)
        {
            try
            {
                string EncryptionKey = Microsoft.VisualBasic.Interaction.InputBox("Key:", "Credentials");
                ExecuteBytes(GetBytes(EncryptionKey));//Execute the decrypted bytes of the program.
            }
            catch(Exception ex) { Microsoft.VisualBasic.Interaction.MsgBox(ex.Message); }
        }

        //Stub||Program||Length Of Program(4 bytes)    
        static byte[] GetBytes(string Key)
        {
            //Get the bytes of this assembly.
            byte[] AssemblyBytes = File.ReadAllBytes(Assembly.GetExecutingAssembly().Location);

            //Get the length of the program that is in the stub.
            //The length is saved in the last 4 bytes.
            byte[] ByteLength = new byte[4];
            Buffer.BlockCopy(AssemblyBytes, AssemblyBytes.Length - 4, ByteLength, 0, 4);//Get last 4 bytes.
            int length = BitConverter.ToInt32(ByteLength, 0);//Convert the bytes to a int.
            if (length <= 0) Environment.Exit(1); //if there is no file in the stub, exit.

            //Remove the first bytes. These bytes are the stub.
            //And remove the last byte's. These are the int with the length.
            byte[] ProgramBytes = new byte[length];
            Buffer.BlockCopy(AssemblyBytes, AssemblyBytes.Length - (length + 4), ProgramBytes, 0, ProgramBytes.Length);

            //Decrypt the bytes and return them.
            try { return new Encryption(Aes.Create(), Key, SALT).Decrypt(ProgramBytes); }
            catch
            {
                Microsoft.VisualBasic.Interaction.MsgBox("Invalid key", Microsoft.VisualBasic.MsgBoxStyle.Critical, "Error");
                Environment.Exit(1); return null;
            }
        }

        static void ExecuteBytes(byte[] Data)
        {
            //Create assembly of the Data.
            Assembly Assembly = Assembly.Load(Data);

            //Get the entry point of the program.
            MethodInfo method = Assembly.EntryPoint;

            //Run the application from starting point.
            method.Invoke(Assembly.CreateInstance(method.Name), null);
        }
    }
}
