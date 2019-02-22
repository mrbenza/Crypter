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
using System.Linq;
using System.Text;
using EasyEncrypt;
using Microsoft.CSharp;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.CodeDom.Compiler;

namespace Crypter
{
    public partial class Crypter : Form
    {
        const string CODE =
@"
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace NaMeSpAcE
{
    class Program
    {
            static void Main()
            {
                byte[] ProgramBytes = GetBytes();//Get the bytes from the program
                ExecuteBytes(ProgramBytes);//Execute the bytes
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

            static byte[] GetBytes()
            {
                Aes Provider = Aes.Create();//Create algorithm
                Provider.Key = ___KEY___;//Set encryption key

                using (MemoryStream ms = new MemoryStream(___PROGRAM___))//Create memory stream with program bytes
                {
                    byte[] IV = new byte[Provider.IV.Length];
                    ms.Read(IV, 0, IV.Length);//Get IV from ms(first 16 bytes)
                    Provider.IV = IV;//Add IV to Algorithm

                    using (CryptoStream cs = new CryptoStream(ms, Provider.CreateDecryptor(Provider.Key, Provider.IV), CryptoStreamMode.Read))
                    {
                        byte[] Decrypted = new byte[ms.Length];
                        int byteCount = cs.Read(Decrypted, 0, (int)ms.Length);
                        return new MemoryStream(Decrypted, 0, byteCount).ToArray();
                    }
                }
            }
        }
    }

";

        public Crypter()=> InitializeComponent();

        private void build_Click(object sender, EventArgs e)
        {
            if (!File.Exists(path.Text)) { MessageBox.Show("No file selected"); return; }

            //Set up the compiler.
            CSharpCodeProvider csc = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, "build.exe") { GenerateExecutable = true };
            if (winexe.Checked) parameters.CompilerOptions = "/target:winexe";

            //Set up the code.
            string Code = CODE;

            byte[] EncryptionKey = Aes.Create().Key;
            Code = Code.Replace("___KEY___", ByteArrayToString(EncryptionKey));

            byte[] ProgramBytes = File.ReadAllBytes(path.Text);
            byte[] EncryptedProgramBytes = new Encryption(Aes.Create(), EncryptionKey).Encrypt(ProgramBytes);

            Code = Code.Replace("___PROGRAM___", ByteArrayToString(EncryptedProgramBytes));

            CompilerResults results = csc.CompileAssemblyFromSource(parameters, Code);
            results.Errors.Cast<CompilerError>().ToList().ForEach(error => MessageBox.Show(error.ErrorText));
        }

        private string ByteArrayToString(byte[] Array)
        {
            StringBuilder Builder = new StringBuilder();
            Builder.Append("new byte[] {");
            foreach (byte b in Array)
            {
                Builder.Append(b);
                Builder.Append(',');
            }
            Builder.Append("}");
            return Builder.ToString();           
        }

        private void search_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
                if (openFileDialog1.ShowDialog() == DialogResult.OK) path.Text = openFileDialog1.FileName;
        }
    }
}
