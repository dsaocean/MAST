using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MAST.Logic
{
    internal delegate void StatusChange();
    internal delegate void UDP_Update(UDP_Service.UDP_Message message, Simulation sim);
    internal delegate void SimulationComplete(Simulation sim);
    internal delegate void SimulationUpdate(Simulation sim);

    internal class Simulation
    {
        public int index = 0;
        public string IP = "224.121.3.7";
        public int listenPort = 23738;
        public int sendPort = 23737;

        // two?!
        internal bool consoleMode = false;
        private bool useCommandPrompt = false;
        internal System.IO.DirectoryInfo directory = null;

        internal bool overwrite = true;
        internal bool verbose = false;
        internal bool experimental = true;

        public DateTime haltRequestedAt = DateTime.Now;
        private UDP_Service.Globals.SimulationStatus m_status = UDP_Service.Globals.SimulationStatus.Editing;

        internal bool cancelled = false;

        internal event UDP_Update OnUDPUpdate = null;
        internal event SimulationComplete OnSimulationComplete = null;
        internal event StatusChange OnStatusChange = null;
        internal int exitcode = -999;

        internal UDP_Service.Globals.OutputType outputType = UDP_Service.Globals.OutputType.ASCII;

        internal string hostguid = Guid.NewGuid().ToString();
        internal string pdsguid = Guid.NewGuid().ToString();

        UDP_Service.UDP_Manager manager = new UDP_Service.UDP_Manager();

        System.Timers.Timer timer = new System.Timers.Timer();

        private double min_interval = 5000;
        private double max_interval = 10000;

        internal string errors = "";
        internal string warnings = "";
        internal string console = "";

        internal double endtime = 0;
        internal double currenttime = 0;
        internal double starttime = 0;
        internal double timeratio = 0;
        internal double lasttimestep = 0;
        internal string etr = "";
        internal string slowestDObject = "";

        internal DateTime simStartTime = DateTime.Now;
        internal double initSpamDuration = 10000;
        internal double initSpamInterval = 2500;
        internal bool overrideInitSpam = false;

        internal bool hasWarning = false;

        internal bool handshakeRequested = false;

        internal UDP_Service.Globals.SimulationStatus desiredStatus = UDP_Service.Globals.SimulationStatus.Running;
        private DateTime lastReceived = DateTime.Now;
        private bool winddown = false;

        bool workerReturned = false;
        bool insideTimer = false;
        object locker = new object();

        bool dealingWithReceive = false;

        internal bool report = true;

        internal bool runScript = true;
        internal bool scriptRan = false;
        internal int scriptExitCode = -999;

        //internal System.Windows.Forms.Timer executeSimTimer = new System.Windows.Forms.Timer();
        //internal System.Timers.Timer executeSimTimer = new System.Timers.Timer();

        internal UDP_Service.Globals.SimulationStatus Status
        {
            get
            {
                return this.m_status;
            }
            set
            {
                this.m_status = value;
                if (this.OnStatusChange != null)
                {
                    this.OnStatusChange();
                }
            }
        }

        internal int Progress
        {
            get
            {
                if (this.Status == UDP_Service.Globals.SimulationStatus.Complete)
                {
                    return 100;
                }
                else
                {
                    if (this.endtime > 0)
                    {
                        return (int)(((this.currenttime - this.starttime) / (this.endtime - this.starttime)) * 100);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        internal string InputPath
        {
            get
            {
                return this.directory.FullName.ToString();
            }
        }

        internal string SolverPath
        {
            get
            {
                return Program.SolverLocation;
            }
        }

        internal string ErrorFilePath
        {
            get
            {
                return System.IO.Path.Combine(this.directory.FullName.ToString(), "Results", "pds.error");
            }
        }

        internal string ResultsFilePath
        {
            get
            {
                return System.IO.Path.Combine(this.directory.FullName.ToString(), "Results", "Results.pdso");
            }
        }

        internal string OutputPath
        {
            get
            {
                return System.IO.Path.Combine(this.directory.FullName.ToString(), "Results");
            }
        }

        internal bool UseCommandPrompt
        {
            get
            {
                return this.useCommandPrompt;
            }
            set
            {
                this.useCommandPrompt = value;
            }
        }

        static int globalID = 0;
        internal Simulation()
        {
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

            lock (Simulation.logLocker)
            {
                this.id = Simulation.globalID++;
            }
        }

        private void ResetRunVariables()
        {
            this.haltRequestedAt = DateTime.Now;
            //this.haveSentLastShutdownRequest = false;
            this.exitcode = -999;
            this.winddown = false;
            this.workerReturned = false;
            this.insideTimer = false;
            this.dealingWithReceive = false;
            this.manager = new UDP_Service.UDP_Manager();
            this.consoleMode = false;
        }

        internal void RunDebugSim(string guid, bool console = false)
        {
            this.ResetRunVariables();
            string arguments = "";

            this.verbose = Program.useVerboseMode;

            arguments = string.Format("-i \"{0}\" -o \"{1}\" -l \"{2}\" -output {3} {4} {5} {6} -overwrite on", this.InputPath, this.OutputPath, Program.registryInfo.GetData("LicenseDirectory"), this.outputType.ToString().ToLower(), this.verbose ? " -verbose on" : "", this.experimental ? " -experimental on" : "", this.overwrite ? " -overwrite on" : "");

            if (!console)
            {
                arguments += string.Format(" -suppressconsole on -guid {0}", this.pdsguid);
            }

            this.workerReturned = false;

            this.pdsguid = guid;

            //string k = "ProteusDS.exe -i input -o output -output ascii -l licensefolder -verbose on/off -experimental on/off -guid A -overwrite on -suppressconsole on";

            //worker.RunWorkerAsync(ei);

            if (!console)
            {
                this.StartTimerRemote();
            }
        }

        internal System.Windows.Forms.Timer startTimer = new System.Windows.Forms.Timer();



        internal void RunSim()
        {
            bool console = false;
            this.ResetRunVariables();
            string arguments = "";

            this.verbose = Program.useVerboseMode;
            arguments = string.Format("-i \"{0}\" -o \"{1}\" -l \"{2}\" -output {3} {4} {5} {6} -overwrite on", this.InputPath, this.OutputPath, Program.registryInfo.GetData("LicenseDirectory"), this.outputType.ToString().ToLower(), this.verbose ? " -verbose on" : "", this.experimental ? " -experimental on" : "", this.overwrite ? " -overwrite on" : "");

            if (!console)
            {
                arguments += string.Format(" -suppressconsole on -guid {0}", this.pdsguid);
            }

            this.workerReturned = false;

            //string k = "ProteusDS.exe -i input -o output -output ascii -l licensefolder -verbose on/off -experimental on/off -guid A -overwrite on -suppressconsole on";

            ExecutionInput ei = new ExecutionInput();
            ei.arguments = arguments;
            ei.launchWithCommandPrompt = this.useCommandPrompt;
            ei.executablePath = this.SolverPath;
            ei.console = console;

            if (ei.console)
            {
                ei.launchWithCommandPrompt = true;
                this.consoleMode = true;
            }

            BackgroundWorker worker = new BackgroundWorker(); //Run proteusDS on another thread
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);

            //if (!ei.console)
            {
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            }

            this.simStartTime = DateTime.Now;
            worker.RunWorkerAsync(ei);

            if (!ei.console)
            {
                this.StartTimerRemote();
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (dealingWithReceive)
            {
                return;
            }
            if (insideTimer)
            {
                return;
            }

            if (this.workerReturned)
            {
                return;
            }

            string str = "";
            int maximumCommGap = 300;

            if ((DateTime.Now - this.lastReceived).TotalSeconds > maximumCommGap)
            {
                // okay, we've been trying to send for a very long time now and have not received a response.
                // first step is to update status to 'pending' (but desired to 'running')
                // if it reaches the next threshold, kill the process and set status to 'aborted'?
            }

            lock (locker)
            {
                if (insideTimer)
                {
                    return;
                }

                insideTimer = true;

                //if (this.desiredStatus == Globals.SimulationStatus.Cancel)
                //{
                if (this.handshakeRequested)
                {
                    str = this.GenerateSend(UDP_Service.Globals.Instruction.Handshake, true);
                    //Simulation.WritePacket(PacketWriteMode.Outbound, str, this.id, this.pdsguid, this.hostguid, false, this.directory.ToString());
                    UDP_Service.UDP_Sender.SendMessage(System.Net.IPAddress.Parse(this.IP), this.sendPort, str);
                    insideTimer = false;
                    return;
                }
                //}

                if (this.desiredStatus == UDP_Service.Globals.SimulationStatus.Complete)// || this.desiredStatus == Globals.SimulationStatus.Cancel)
                {
                    str = this.GenerateSend(UDP_Service.Globals.Instruction.SaveAndExit, false);
                    //Simulation.WritePacket(PacketWriteMode.Outbound, str, this.id, this.pdsguid, this.hostguid, false, this.directory.ToString());
                    UDP_Service.UDP_Sender.SendMessage(System.Net.IPAddress.Parse(this.IP), this.sendPort, str);
                    this.Status = UDP_Service.Globals.SimulationStatus.ProcessingShutdown;
                    //this.haveSentLastShutdownRequest = false;

                    insideTimer = false;
                    return;

                    //if ((DateTime.Now - this.haltRequestedAt).TotalSeconds > 5 && !this.haveSentLastShutdownRequest)
                    //{
                    //    this.haveSentLastShutdownRequest = true;
                    //    this.winddown = true;
                    //    this.exitcode = 0;
                    //    this.worker_RunWorkerCompleted(null, null);
                    //    insideTimer = false;
                    //    return;
                    //}
                }

                if (this.winddown)
                {
                    //if (!this.manager.NeedsInfo())
                    {
                        // try sending an exit message until the simulation stops
                        str = this.GenerateSend(UDP_Service.Globals.Instruction.SaveAndExit, false);
                        //Simulation.WritePacket(PacketWriteMode.Outbound, str, this.id, this.pdsguid, this.hostguid, false, this.directory.ToString());
                        UDP_Service.UDP_Sender.SendMessage(System.Net.IPAddress.Parse(this.IP), this.sendPort, str);
                        insideTimer = false;
                        return;
                    }
                }

                //if (this.Status == Globals.SimulationStatus.HandshakeRequested)
                //{
                //    str = this.GenerateSend(Globals.Instruction.Handshake, true);
                //    this.Status = Globals.SimulationStatus.Running;
                //}
                //else
                {
                    // send!
                    if (this.desiredStatus != this.Status)
                    {
                        switch (this.desiredStatus)
                        {
                            case UDP_Service.Globals.SimulationStatus.Cancel:
                            case UDP_Service.Globals.SimulationStatus.Complete:
                                str = this.GenerateSend(UDP_Service.Globals.Instruction.RequestHeartbeat | UDP_Service.Globals.Instruction.SaveAndExit, true);
                                break;
                            case UDP_Service.Globals.SimulationStatus.Paused:
                                str = this.GenerateSend(UDP_Service.Globals.Instruction.RequestHeartbeat | UDP_Service.Globals.Instruction.Pause, true);
                                break;
                            case UDP_Service.Globals.SimulationStatus.Running:
                                str = this.GenerateSend(UDP_Service.Globals.Instruction.RequestHeartbeat | UDP_Service.Globals.Instruction.Resume, true);
                                break;
                            //case Globals.SimulationStatus.HandshakeRequested:
                            //    str = this.GenerateSend(Globals.Instruction.RequestHeartbeat | Globals.Instruction.Handshake, true);
                            //    break;
                        }
                    }
                    else
                    {
                        str = this.GenerateSend(UDP_Service.Globals.Instruction.RequestHeartbeat, true);
                    }
                }

                UDP_Service.UDP_Message msg = UDP_Service.UDP_Message.ComposeMessage(str, this.hostguid, this.pdsguid);

                //this.UpdateSendMessage(msg);

                //Console.WriteLine(str);

                //Simulation.WritePacket(PacketWriteMode.Outbound, str, this.id, this.pdsguid, this.hostguid, false, this.directory.ToString());
                UDP_Service.UDP_Sender.SendMessage(System.Net.IPAddress.Parse(this.IP), this.sendPort, str);


                if ((DateTime.Now - this.simStartTime).TotalMilliseconds > this.initSpamDuration || this.overrideInitSpam)
                {
                    if (this.manager.NeedsInfo())
                    {
                        this.timer.Interval = this.min_interval;
                    }
                    else
                    {
                        this.timer.Interval = this.max_interval;
                    }
                }


                insideTimer = false;
            }
        }

        internal void UDPCallback(UDP_Service.UDP_Message message)
        {
            if (message.from.CompareTo(this.pdsguid) != 0)
            {
                //return;
            }

            try
            {
                //Simulation.WritePacket(PacketWriteMode.Inbound, message.message, this.id, this.pdsguid, this.hostguid, false, this.directory.ToString());
                PDSLibrary.Utilities.LocalizedParser.ForceUSLocale(true);
                this.overrideInitSpam = true;

                //Console.WriteLine("{0}: {1}", this.name_string, message.from);

                this.dealingWithReceive = true;
                this.lastReceived = DateTime.Now;

                //Console.WriteLine(message.message);

                this.manager.ProcessMessage(message);

                this.errors += this.manager.GetNewErrors();
                this.warnings += this.manager.GetNewWarnings();
                this.console += this.manager.GetNewConsole();

                if (this.warnings.Length > 0)
                {
                    this.hasWarning = true;
                }

                string receivedStatus = message.properties["state"][0];
                //bool readmsg = true;

                if (this.Status == UDP_Service.Globals.SimulationStatus.ProcessingShutdown)
                {
                    // we've sent a request to quit, so let's wait for a bit.
                    // we'll need to receive acknowledgment that the msg was received at some point, but for now, just ignore it
                }
                else
                {
                    switch (receivedStatus.ToLower().Trim())
                    {
                        case "waitingforhandshake":
                            //this.Status = Globals.SimulationStatus.HandshakeRequested;
                            this.handshakeRequested = true;
                            break;
                        case "handshakeacknowledged":
                            this.handshakeRequested = false;
                            //this.Status = Globals.SimulationStatus.HandshakeReceived;
                            break;
                        case "ending":
                            break;
                        case "cancelled":
                            this.Status = UDP_Service.Globals.SimulationStatus.Cancel;
                            break;
                        case "running":
                            this.Status = UDP_Service.Globals.SimulationStatus.Running;
                            break;
                        case "paused":
                            this.Status = UDP_Service.Globals.SimulationStatus.Paused;
                            break;
                        case "error":
                            //readmsg = false;
                            message.valid = false;

                            if (!this.manager.NeedsInfo())
                            {
                                this.winddown = true;
                            }
                            break;
                    }
                }

                if (message.valid)
                {
                    double value = 0;
                    if (message.properties.ContainsKey("endtime") && double.TryParse(message.properties["endtime"][0], out value))
                    {
                        this.endtime = value;
                    }
                    if (message.properties.ContainsKey("starttime") && double.TryParse(message.properties["starttime"][0], out value))
                    {
                        this.starttime = value;
                    }
                    if (message.properties.ContainsKey("currenttime") && double.TryParse(message.properties["currenttime"][0], out value))
                    {
                        this.currenttime = value;
                    }
                    if (message.properties.ContainsKey("timeratio") && double.TryParse(message.properties["timeratio"][0], out value))
                    {
                        this.timeratio = value;
                    }
                    if (message.properties.ContainsKey("lasttimestep") && double.TryParse(message.properties["lasttimestep"][0], out value))
                    {
                        this.lasttimestep = value;
                    }

                    if (message.properties.ContainsKey("slowestdobject") && message.properties.ContainsKey("slowestdobject"))
                    {
                        this.slowestDObject = message.properties["slowestdobject"][0];
                    }

                    //this.endtime = double.Parse(message.properties["endtime"][0]);
                    //this.starttime = double.Parse(message.properties["starttime"][0]);
                    //this.currenttime = double.Parse(message.properties["currenttime"][0]);
                    //this.timeratio = double.Parse(message.properties["timeratio"][0]);

                    if (message.properties.ContainsKey("etr"))
                    {
                        this.etr = message.properties["etr"][0];
                    }
                }

                if (this.report && this.OnUDPUpdate != null)
                {
                    this.OnUDPUpdate(message, this);
                }
                PDSLibrary.Utilities.LocalizedParser.RestoreNativeLocale(false);
                this.dealingWithReceive = false;
            }
            catch (Exception e)
            {
                PDSLibrary.Utilities.LocalizedParser.RestoreNativeLocale(false);
                throw e;
            }
        }

        private static object logLocker = new object();
        private static System.IO.TextWriter logWriter = null;
        private int id = 0;
        private bool hasWritten = false;
        private static void OpenPacket()
        {
            if (Simulation.logWriter == null)
            {
                logWriter = new System.IO.StreamWriter(@"C:\Temp\MAST_Log.txt");
            }
        }
        private enum PacketWriteMode
        {
            Outbound,
            Inbound,
            Status
        }
        private static void WritePacket(PacketWriteMode mode, string packet, int id, string PDSGUID, string MASTGUID, bool hasWritten, string folder)
        {
            lock (Simulation.logLocker)
            {
                if (Simulation.logWriter == null)
                {
                    OpenPacket();
                }

                string prependText = "";

                switch (mode)
                {
                    case PacketWriteMode.Outbound:
                        {
                            prependText = string.Format(" OUT {0:00000}: ", id);
                            break;
                        }
                    case PacketWriteMode.Inbound:
                        {
                            prependText = string.Format("  IN {0:00000}: ", id);
                            break;
                        }
                    case PacketWriteMode.Status:
                        {
                            prependText = string.Format("STAT {0:00000}: ", id);
                            break;
                        }
                }



                using (System.IO.StringReader sr = new System.IO.StringReader(packet))
                {
                    using (System.IO.StringWriter sw = new System.IO.StringWriter())
                    {
                        sw.WriteLine("{0}{1}", prependText, folder);
                        sw.WriteLine("{0}{1}", prependText, DateTime.Now);
                        sw.WriteLine("{0}{1}; {2}", prependText, PDSGUID, MASTGUID);


                        string line = "";
                        while ((line = sr.ReadLine()) != null)
                        {
                            sw.WriteLine("{0}{1}", prependText, line);
                        }

                        Simulation.logWriter.WriteLine(sw.ToString());
                        Simulation.logWriter.Flush();
                    }
                }
            }
        }

        private string GenerateHeader()
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();

            sw.WriteLine("$ProteusDSCommunicationProtocol 0");
            sw.WriteLine("$CommunicationVersion 1");
            sw.WriteLine("$From {0}", this.hostguid);
            sw.WriteLine("$To {0}", this.pdsguid);
            sw.WriteLine("$Timestamp {0}", DateTime.Now.Ticks);

            return sw.ToString();
        }

        private string GenerateInstructions(UDP_Service.Globals.Instruction instruction, bool sendMsgRequest)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();

            sw.WriteLine("<instructions>");

            Array arr = Enum.GetValues(instruction.GetType());
            List<UDP_Service.Globals.Instruction> instructions = new List<UDP_Service.Globals.Instruction>();
            foreach (UDP_Service.Globals.Instruction ins in arr)
            {
                if (instruction.HasFlag(ins))
                {
                    instructions.Add(ins);
                }
            }

            foreach (UDP_Service.Globals.Instruction ins in instructions)
            {
                switch (ins)
                {
                    case UDP_Service.Globals.Instruction.Handshake:
                        sw.WriteLine("$handshake 1");
                        break;
                    case UDP_Service.Globals.Instruction.SaveAndExit:
                        sw.WriteLine("$RequestExitWithSave 1");
                        break;
                    case UDP_Service.Globals.Instruction.NoSaveAndExit:
                        sw.WriteLine("$RequestExitWithoutSave 1");
                        break;
                    case UDP_Service.Globals.Instruction.RequestHeartbeat:
                        sw.WriteLine("$RequestHeartbeat 1");
                        break;
                    case UDP_Service.Globals.Instruction.Resume:
                        sw.WriteLine("$RequestResume 1");
                        break;
                    case UDP_Service.Globals.Instruction.Pause:
                        sw.WriteLine("$RequestPause 1");
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (sendMsgRequest)
            {
                sw.WriteLine(this.manager.PrepareUpdate());
            }
            sw.WriteLine("</instructions>");

            return sw.ToString();
        }

        private string GenerateSend(UDP_Service.Globals.Instruction instruction, bool sendMsgRequest)
        {
            string ret = "";
            using (System.IO.StringWriter sw = new System.IO.StringWriter())
            {
                sw.Write(this.GenerateHeader());

                sw.Write(this.GenerateInstructions(instruction, sendMsgRequest));

                ret = sw.ToString();
            }

            return ret;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            PDSLibrary.Utilities.LocalizedParser.ForceUSLocale(true);
            string executable = this.SolverPath;

            // always assume that arguments is ExecutionInput

            ExecutionInput ei = (ExecutionInput)e.Argument;
            if (ei.useCustomExecutablePath)
            {
                executable = ei.executablePath;
            }

            //if (System.Windows.Forms.MessageBox.Show(string.Format("proceed?\n{0}", executable), "", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
            //{
            //    this.exitcode = 0;
            //    return;
            //}

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            if (ei.launchWithCommandPrompt)
            {
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
            }
            else
            {
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            }
            psi.Arguments = ei.arguments;
            psi.FileName = executable;

            this.exitcode = -3;

            int runCount = 0;
            while (this.exitcode == -3 && this.desiredStatus == UDP_Service.Globals.SimulationStatus.Running)
            {
                this.ResetRunVariables();
                this.exitcode = -3;

                //System.Diagnostics.Process pro = System.Diagnostics.Process.Start(executable, ei.arguments);
                System.Diagnostics.Process pro = new System.Diagnostics.Process();
                pro.StartInfo = psi;

                //Simulation.WritePacket(PacketWriteMode.Status, string.Format("Starting sim, pass #: {0}", runCount), this.id, this.pdsguid, this.hostguid, false, this.directory.ToString());                
                pro.Start();
                pro.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
                //Simulation.WritePacket(PacketWriteMode.Status, string.Format("Sim finished, pass #: {0}", runCount), this.id, this.pdsguid, this.hostguid, false, this.directory.ToString());

                runCount++;

                if (!ei.console)
                {
                    pro.WaitForExit();
                    this.exitcode = pro.ExitCode;

                    //Simulation.WritePacket(PacketWriteMode.Status, string.Format("Exit code = {0}", this.exitcode), this.id, this.pdsguid, this.hostguid, false, this.directory.ToString());

                    // execute script here

                    if ((this.exitcode == 0 || this.exitcode == -2) && !this.cancelled)
                    {
                        this.ExecuteScript();
                    }
                }
            }

            //string cmd = string.Format("start /min \"{0}\" {1}", executable, ei.arguments);
            //System.Diagnostics.Process.Start(cmd);
            PDSLibrary.Utilities.LocalizedParser.RestoreNativeLocale(false);
        }

        void ExecuteScript()
        {
            // check whether the folder has a script file
            string scriptpath = System.IO.Path.Combine(this.directory.FullName, "..");
            string absoluteScriptPath = System.IO.Path.GetFullPath(scriptpath);
            string absoluteScriptFile = System.IO.Path.Combine(absoluteScriptPath, "script.bat");

            if (System.IO.File.Exists(absoluteScriptFile))
            {

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = absoluteScriptFile;
                psi.Arguments = string.Format("{0}", this.directory.FullName);// > Results\\ScriptLog.txt", this.directory.FullName);
                psi.WorkingDirectory = absoluteScriptPath;// this.directory.FullName;
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                System.Diagnostics.Process pro = new System.Diagnostics.Process();
                pro.StartInfo = psi;

                pro.Start();
                pro.WaitForExit();

                this.scriptExitCode = pro.ExitCode;

                this.scriptRan = true;
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.timer.Enabled)
            {
                this.timer.Stop();
            }

            this.workerReturned = true;

            if (this.exitcode == 0)
            {
                if (this.exitcode == 0)
                {
                    if (!this.cancelled)
                    {
                        this.Status = UDP_Service.Globals.SimulationStatus.Complete;
                    }
                    else
                    {
                        this.Status = UDP_Service.Globals.SimulationStatus.Cancel;
                    }
                }
            }
            else
            {
                if (this.errors.Length == 0)
                {
                    this.errors += "Unexpected error." + System.Environment.NewLine;
                }

                if (this.report && this.OnUDPUpdate != null)
                {
                    this.OnUDPUpdate(null, this);
                }
                this.Status = UDP_Service.Globals.SimulationStatus.Error;
            }

            // we need to unregister our UDP stuff here
            //.new.UDP_Service.UDP_Listener.Register(new UDP_Service.ConnectionName(QueueViewer.IP, QueueViewer.listenPort), this.hostguid, this.UDPCallback);

            UDP_Service.UDP_Listener.Deregister(new UDP_Service.ConnectionName(this.IP, this.listenPort), this.hostguid, this.UDPCallback);

            if (this.report && this.OnSimulationComplete != null)
            {
                this.OnSimulationComplete(this);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("Input folder: {0}", this.InputPath));
            sb.AppendLine(string.Format("\t  Status: {0}", this.Status));
            sb.AppendLine(string.Format("\tExitcode: {0}", this.exitcode));

            return sb.ToString();
        }

        internal System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        internal void Halt()
        {
            // keep sending halt messages to the solver until the sim stops
            //this.winddown = true; // this should do it!
            this.desiredStatus = UDP_Service.Globals.SimulationStatus.Cancel;
            this.cancelled = true;

            this.haltRequestedAt = DateTime.Now;
        }

        internal void PauseResume()
        {
            // keep sending pause/resume messages to the solver until the sim pauses or resumes
            if (this.desiredStatus == UDP_Service.Globals.SimulationStatus.Running)
            {
                this.desiredStatus = UDP_Service.Globals.SimulationStatus.Paused;
            }
            else
            {
                this.desiredStatus = UDP_Service.Globals.SimulationStatus.Running;
            }
        }

        internal void RemoveCallbacks()
        {
            this.OnSimulationComplete = null;
            //this.OnSimulationUpdate = null;
            this.OnUDPUpdate = null;
        }

        internal void Reset()
        {
            if (this.Status == UDP_Service.Globals.SimulationStatus.Complete || this.Status == UDP_Service.Globals.SimulationStatus.Error || this.Status == UDP_Service.Globals.SimulationStatus.Unknown)
            {
                this.endtime = 0;
                this.currenttime = 0;
                this.starttime = 0;
                this.timeratio = 0;
                this.lasttimestep = 0;
                this.etr = "";

                this.manager = new UDP_Service.UDP_Manager();

                this.errors = string.Empty;
                this.warnings = string.Empty;
                this.console = string.Empty;

                this.desiredStatus = UDP_Service.Globals.SimulationStatus.Running;

                this.Status = UDP_Service.Globals.SimulationStatus.Editing;
            }
        }

        internal void StartTimerRemote()
        {
            UDP_Service.UDP_Listener.Register(new UDP_Service.ConnectionName(this.IP, this.listenPort), this.hostguid, this.UDPCallback);

            //this.executeTimerCallback = true;
            this.simStartTime = DateTime.Now;
            this.timer.Interval = this.initSpamInterval;
            this.timer.Start();

            this.lastReceived = DateTime.Now;
        }

        internal void EraseResults()
        {
            if (System.IO.Directory.Exists(this.OutputPath))
            {
                try
                {
                    System.IO.Directory.Delete(this.OutputPath, true);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    internal class ExecutionInput
    {
        internal bool launchWithCommandPrompt = true;
        internal string arguments = "";
        internal string executablePath = "";
        internal bool useCustomExecutablePath = false;
        internal bool console = false;

        internal ExecutionInput()
        {
        }

        internal ExecutionInput(ExecutionInput ei)
        {
            this.launchWithCommandPrompt = ei.launchWithCommandPrompt;
            this.arguments = ei.arguments;
            this.executablePath = ei.executablePath;
            this.useCustomExecutablePath = ei.useCustomExecutablePath;
            this.console = ei.console;
        }
    }
}
