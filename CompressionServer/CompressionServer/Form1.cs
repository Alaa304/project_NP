using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace CompressionServer
{
    public partial class Form1 : Form
    {
        Socket server;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(StartServer);

            t.IsBackground = true;

            t.Start();

            listBox1.Items.Add("Server Started...");
        }
        private void StartServer()
        {
            try
            {
                IPEndPoint ipep =
                    new IPEndPoint(IPAddress.Any, 9050);

                server = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

                server.Bind(ipep);

                server.Listen(10);

                while (true)
                {
                    Invoke(new Action(() =>
                    {
                        listBox1.Items.Add("Waiting for client...");
                    }));

                    Socket client = server.Accept();

                    Invoke(new Action(() =>
                    {
                        listBox1.Items.Add("Client Connected");
                    }));

                    Thread clientThread =
                        new Thread(() => HandleClient(client));

                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void HandleClient(Socket client)
        {
            try
            {
              
                byte[] nameSizeBytes = new byte[4];

                client.Receive(nameSizeBytes);

                int nameSize =
                    BitConverter.ToInt32(nameSizeBytes, 0);


                byte[] nameBytes = new byte[nameSize];

                client.Receive(nameBytes);

                string fileName =
                    System.Text.Encoding.UTF8.GetString(nameBytes);


                byte[] sizeBytes = new byte[4];

                client.Receive(sizeBytes);

                int fileSize =
                    BitConverter.ToInt32(sizeBytes, 0);

              

                byte[] fileData = new byte[fileSize];

                int totalReceived = 0;

                while (totalReceived < fileSize)
                {
                    totalReceived += client.Receive(
                        fileData,
                        totalReceived,
                        fileSize - totalReceived,
                        SocketFlags.None
                    );
                }

                // =========================
                // SAVE FILE
                // =========================

                string originalPath =
                    Path.Combine(
                        Application.StartupPath,
                        fileName
                    );

                File.WriteAllBytes(originalPath, fileData);

                Invoke(new Action(() =>
                {
                    listBox1.Items.Add(fileName + " received");
                }));

               

                string zipPath = originalPath + ".zip";

                using (FileStream zipFs =
                    new FileStream(zipPath, FileMode.Create))
                {
                    using (ZipArchive archive =
                        new ZipArchive(zipFs, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(
                            originalPath,
                            fileName
                        );
                    }
                }



                byte[] zipBytes =
                    File.ReadAllBytes(zipPath);

               

                byte[] zipSize =
                    BitConverter.GetBytes(zipBytes.Length);

                client.Send(zipSize);

              

                client.Send(zipBytes);

                Invoke(new Action(() =>
                {
                    listBox1.Items.Add("Compressed file sent");
                }));

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
