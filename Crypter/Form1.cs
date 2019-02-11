/* HenkCrypter
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
using AesEncryption;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Crypter
{
    public partial class Crypter : Form
    {
        const string KEY = "EncryptionKey";
        const string SALT = "SALT!@#1(D1423SFNI1234O)#234@)$321#90";

        public Crypter()=> InitializeComponent();

        private void build_Click(object sender, EventArgs e)
        {
            if (!File.Exists(path.Text)) { MessageBox.Show("No file selected"); return; }

            string EncryptionKey = Key.Text;//Get the right key, if textbox is empty. Get default key.
            if (EncryptionKey.Length <= 0) EncryptionKey = KEY;

            //Get the bytes of the rigth stub,
            //Stub1 = stub with password
            //Stub2 = stub without password
            byte[] StubBytes;
            if (Key.Text.Length <= 0) StubBytes =  File.ReadAllBytes(Path.Combine(Application.StartupPath, "Stub2.data"));
            else StubBytes = File.ReadAllBytes(Path.Combine(Application.StartupPath, "Stub1.data"));

            //Get the bytes of the Program.
            byte[] ProgramBytes = File.ReadAllBytes(path.Text);

            //Encrypt the bytes of the Program.
            ProgramBytes = new Encryption(Aes.Create(),EncryptionKey,SALT).Encrypt(ProgramBytes);

            //Get the length of the encrypted bytes and convert the length to a byte[].
            byte[] LengthBytes = BitConverter.GetBytes(ProgramBytes.Length);

            //Merge all the bytes,
            //result: [StubBytes,ProgramBytes,Length of ProgramBytes]
            byte[] ToWrite = new byte[StubBytes.Length + ProgramBytes.Length + LengthBytes.Length];
            Buffer.BlockCopy(StubBytes, 0, ToWrite, 0, StubBytes.Length);
            Buffer.BlockCopy(ProgramBytes, 0, ToWrite, StubBytes.Length, ProgramBytes.Length);
            Buffer.BlockCopy(LengthBytes, 0, ToWrite, StubBytes.Length + ProgramBytes.Length, LengthBytes.Length);

            //Write all the bytes.
            File.WriteAllBytes(Path.Combine(Application.StartupPath, "Build.exe"),ToWrite);
        }

        private void search_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
                if (openFileDialog1.ShowDialog() == DialogResult.OK) path.Text = openFileDialog1.FileName;
        }
    }
}
