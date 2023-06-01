using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace MAST.Logic
{
    internal class SimulationCollection
    {
        internal List<Simulation> sims = new List<Simulation>();
        internal List<Simulation> runningSims = new List<Simulation>();

        internal int maxNumSimultaneousSims = 1;
        internal int lastStarted = 0;

        internal System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        internal bool keepRunning = true;
        internal bool runSimulations = false;
        public event SimulationComplete OnSimulationComplete = null;
        public event SimulationUpdate OnSimulationUpdate = null;
        public event UDP_Update OnUDPUpdate = null;
        public object lockable = new object();

        Thread thread = null;

        internal SimulationCollection()
        {
            this.thread = new Thread(new ThreadStart(this.Run));
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public void Start()
        {
            lock (this.lockable)
            {
                this.runSimulations = true;
            }
        }

        public void Stop()
        {
            lock (this.lockable)
            {
                this.runSimulations = false;
            }
        }

        public void HaltThread()
        {
            this.Stop();

            // must notify all currently running sims that they should not return status/complete signals
            foreach (Simulation sim in this.runningSims)
            {
                sim.report = false;
            }
        }

        public void PrepareRunSimulation(Simulation sim)
        {
            // spawn the process
            sim.Status = UDP_Service.Globals.SimulationStatus.Running;

            sim.RemoveCallbacks();
            sim.OnSimulationComplete += new SimulationComplete(SimulateComplete);
            //sim.OnSimulationUpdate += new SimulationUpdate(SimulationUpdate);
            sim.OnUDPUpdate += new UDP_Update(sim_OnUDPUpdate);
        }

        void sim_OnUDPUpdate(UDP_Service.UDP_Message message, Simulation sim)
        {
            this.OnUDPUpdate(message, sim);
        }

        private void SimulateComplete(Simulation sim)
        {
            // remove the sim from the list of running simulations
            Console.WriteLine(string.Format("Simulate COMPLETE: {0}", sim));

            lock (this.runningSims)
            {
                if (this.runningSims.Contains(sim))
                {
                    this.runningSims.Remove(sim);
                }
            }

            if (this.OnSimulationComplete != null)
            {
                this.OnSimulationComplete(sim);
            }
        }

        private void SimulationUpdate(Simulation sim)
        {
            Console.WriteLine(string.Format("Simulate UPDATE: {0}", sim));
            if (this.OnSimulationUpdate != null)
            {
                this.OnSimulationUpdate(sim);
            }
        }

        internal void UnPauseAll()
        {
            this.runSimulations = true;

            foreach (Simulation sim in this.runningSims)
            {
                sim.desiredStatus = UDP_Service.Globals.SimulationStatus.Running;
            }
        }

        internal void PauseAll()
        {
            this.runSimulations = false;

            foreach (Simulation sim in this.runningSims)
            {
                sim.desiredStatus = UDP_Service.Globals.SimulationStatus.Paused;
            }
        }

        internal void HaltAll()
        {
            this.runSimulations = false;

            foreach (Simulation sim in this.runningSims)
            {
                sim.desiredStatus = UDP_Service.Globals.SimulationStatus.Cancel;
                sim.cancelled = true;
            }
        }

        private List<System.IO.DirectoryInfo> GetFoldersToAdd(System.IO.DirectoryInfo startingFolder)
        {
            List<System.IO.DirectoryInfo> ret = new List<System.IO.DirectoryInfo>();

            System.IO.FileInfo[] files = startingFolder.GetFiles("env.ini", System.IO.SearchOption.TopDirectoryOnly);

            if (files.Length > 0)
            {
                ret.Add(startingFolder);
            }

            System.IO.DirectoryInfo[] dirs = startingFolder.GetDirectories();
            foreach (System.IO.DirectoryInfo di in dirs)
            {
                files = di.GetFiles("Results.PDSo", System.IO.SearchOption.TopDirectoryOnly);

                if (files.Length > 0)
                {
                    break;
                }
                else
                {
                    ret.AddRange(this.GetFoldersToAdd(di));
                }
            }

            return ret;
        }

        internal bool RetrieveSimulations(System.IO.DirectoryInfo folder, bool overwrite)
        {
            if (overwrite)
            {
                this.sims.Clear();
            }

            //int count = 0;
            //foreach (System.IO.DirectoryInfo di in folder.GetDirectories())
            //{
            //    if (di.GetFiles("*.pdsi", System.IO.SearchOption.TopDirectoryOnly).Length > 0)
            //    {
            //        Simulation sim = new Simulation();
            //        sim.directory = di;
            //        sim.index = count++;
            //        this.sims.Add(sim);
            //    }
            //}


            List<int> rowsAdded = new List<int>();

            List<System.IO.DirectoryInfo> list = new List<System.IO.DirectoryInfo>();

            list.AddRange(this.GetFoldersToAdd(folder));

            int count = 0;
            foreach (System.IO.DirectoryInfo di in list)
            {
                if (di.GetFiles("*.pdsi", System.IO.SearchOption.TopDirectoryOnly).Length > 0)
                {
                    Simulation sim = new Simulation();
                    sim.directory = di;
                    sim.index = count++;
                    this.sims.Add(sim);
                }
            }

            return true;
        }

        public void RunSimulation(Simulation sim)
        {
            this.PrepareRunSimulation(sim);
            sim.RunSim();
        }



        public void Run()
        {
            Random rand = new Random();

            lastStarted = 0;
            while (keepRunning)
            {
                if (this.runSimulations)
                {
                    if (this.runningSims.Count < this.maxNumSimultaneousSims && this.runningSims.Count < this.sims.Count)
                    {
                        Simulation newSimToRun = null;

                        for (int i = lastStarted; i < this.sims.Count; i++)
                        {
                            if (this.sims[i].Status != UDP_Service.Globals.SimulationStatus.Running)
                            {
                                newSimToRun = this.sims[i];
                                lastStarted = i + 1;
                                break;
                            }
                        }


                        //newSimToRun.executeSimTimer.Interval = 1500 * rand.Next(2, 23);
                        //Console.WriteLine("Sim id: {0}. Interval: {1}. Expected callback time: {2}", lastStarted - 1, newSimToRun.executeSimTimer.Interval, (DateTime.Now.AddMilliseconds(newSimToRun.executeSimTimer.Interval)).ToLocalTime());
                        //newSimToRun.executeSimTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => ExecuteSimCallback(sender, e, newSimToRun);//; += new EventHandler(executeSimTimer_Tick);// (sender, e) => ExecuteSimCallback(sender, e, newSimToRun);
                        //newSimToRun.executeSimTimer.Start();

                        if (newSimToRun != null)
                        {
                            this.RunSimulation(newSimToRun);

                            lock (this.runningSims)
                            {
                                this.runningSims.Add(newSimToRun);
                            }
                        }

                        Thread.Sleep(1000);
                        Thread.Yield();
                    }
                    else
                    {
                        Thread.Sleep(500);
                        Thread.Yield();
                    }
                }
                else
                {
                    Thread.Sleep(100);
                    Thread.Yield();
                }
            }
        }

        //private void ExecuteSimCallback(object sender, System.Timers.ElapsedEventArgs e, Simulation newSimToRun)
        //{

        //    newSimToRun.executeSimTimer.Stop();

        //    this.RunSimulation(newSimToRun);
        //}

        internal void Shutdown()
        {
            this.Stop();

            this.keepRunning = false;
            this.timer.Stop();

            this.thread.Abort();

            // must notify all currently running sims that they should not return status/complete signals
            foreach (Simulation sim in this.runningSims)
            {
                sim.report = false;
            }
        }
    }
}
