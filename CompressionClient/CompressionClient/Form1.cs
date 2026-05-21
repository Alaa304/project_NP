using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace CompressionClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "All Files|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = ofd.FileName;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                Socket client = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

                client.Connect("127.0.0.1", 9050);

                string path = txtPath.Text;

                string fileName =
                    Path.GetFileName(path);

                byte[] fileBytes =
                    File.ReadAllBytes(path);


                byte[] nameBytes =
                    Encoding.UTF8.GetBytes(fileName);

                byte[] nameSize =
                    BitConverter.GetBytes(nameBytes.Length);

                client.Send(nameSize);

            

                client.Send(nameBytes);

              

                byte[] fileSize =
                    BitConverter.GetBytes(fileBytes.Length);

                client.Send(fileSize);

             

                client.Send(fileBytes);

                listBox1.Items.Add("File Sent");

             

                byte[] zipSizeBytes = new byte[4];

                client.Receive(zipSizeBytes);

                int zipSize =
                    BitConverter.ToInt32(zipSizeBytes, 0);

                listBox1.Items.Add(
                 "Original Size = "
                 + fileBytes.Length +
                      " bytes");
                ///////////////////////
                ///int zipSize =
                BitConverter.ToInt32(zipSizeBytes, 0);

                listBox1.Items.Add(
                    "Compressed Size = "
                    + zipSize +
                    " bytes" );

                byte[] zipBytes = new byte[zipSize];

                int totalReceived = 0;

                while (totalReceived < zipSize)
                {
                    totalReceived += client.Receive(
                        zipBytes,
                        totalReceived,
                        zipSize - totalReceived,
                        SocketFlags.None
                    );
                }

                SaveFileDialog sfd =
                    new SaveFileDialog();

                sfd.Filter = "ZIP FILE|*.zip";

                sfd.FileName = fileName + ".zip";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(
                        sfd.FileName,
                        zipBytes
                    );

                    listBox1.Items.Add(
                        "Compressed file received"
                    );
                }

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
