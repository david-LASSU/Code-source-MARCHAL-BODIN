using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Alchemy;
using Alchemy.Classes;
using Phidgets;
using Phidgets.Events;
using Newtonsoft.Json;

namespace EpiService
{
    public partial class EpiService : ServiceBase
    {
        public string version = "1.0.0";
        public WebSocketServer aServer;

        //Thread-safe collection of Online Connections.
        protected static ConcurrentDictionary<string, Connection> OnlineConnections = new ConcurrentDictionary<string, Connection>();

        public EpiService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                this.aServer = new WebSocketServer(8100, System.Net.IPAddress.Any)
                {
                    OnReceive = this.OnReceive,
                    OnSend = this.OnSend,
                    OnConnected = this.OnConnect,
                    OnDisconnect = this.OnDisconnect,
                    TimeOut = new TimeSpan(0, 5, 0)
                };
                this.aServer.Start();
                EventLog.WriteEntry("Running Alchemy WebSocket Server " + this.version);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message.ToString(), EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            try
            {
                this.aServer.Stop();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message.ToString(), EventLogEntryType.Error);
            }
        }

        public void OnConnect(UserContext aContext)
        {
            try
            {
                EventLog.WriteEntry("Client Connected From : " + aContext.ClientAddress.ToString());

                // Create a new Connection Object to save client context information
                var conn = new Connection(aContext);
                //{ Context = aContext };

                // Add a connection Object to thread-safe collection
                OnlineConnections.TryAdd(aContext.ClientAddress.ToString(), conn);
                
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message.ToString(), EventLogEntryType.Error);
            }
        }

        public void OnReceive(UserContext aContext)
        {
            try
            {
                // Si la connexion existe dans onlineconnexion on demande si le rfid est attaché
                if (OnlineConnections.ContainsKey(aContext.ClientAddress.ToString()))                
                {
                    Connection conn;
                    conn = OnlineConnections.FirstOrDefault(x => x.Key == aContext.ClientAddress.ToString()).Value;
                    
                    // TODO Traiter la demande reçue

                }

                EventLog.WriteEntry("Data Received From [" + aContext.ClientAddress.ToString() + "] - " + aContext.DataFrame.ToString());
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message.ToString(), EventLogEntryType.Error);
            }
        }

        public void OnSend(UserContext aContext)
        {
            try
            {
                EventLog.WriteEntry("Data Sent To : " + aContext.ClientAddress.ToString());
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message.ToString(), EventLogEntryType.Error);
            }
        }

        public void OnDisconnect(UserContext aContext)
        {
            try
            {
                EventLog.WriteEntry("Client Disconnected : " + aContext.ClientAddress.ToString());

                // Remove the connection Object from the thread-safe collection
                Connection conn;
                OnlineConnections.TryRemove(aContext.ClientAddress.ToString(), out conn);

                //turn off the led
                conn.rfid.LED = false;

                //close the phidget and dispose of the object
                conn.rfid.close();
                conn.rfid = null;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message.ToString(), EventLogEntryType.Error);
            }
        }
    }

    public class Connection
    {
        // Instance of RFID reader
        public RFID rfid;
        public UserContext Context;
        //{ get; set; }
        
        public Connection(UserContext Context)
        {
            try
            {
                this.Context = Context;
                this.rfid = new RFID();
                //initialize our Phidgets RFID reader and hook the event handlers
                rfid.Attach += new AttachEventHandler(this.rfid_Attach);
                rfid.Detach += new DetachEventHandler(this.rfid_Detach);
                rfid.Error += new ErrorEventHandler(this.rfid_Error);

                rfid.Tag += new TagEventHandler(this.rfid_Tag);
                rfid.TagLost += new TagEventHandler(this.rfid_TagLost);
                rfid.open();

                //this.sendMessage("RFID_STATUS", "1");

                //Wait for a Phidget RFID to be attached before doing anything with 
                //the object
                Console.WriteLine("waiting for attachment...");
                rfid.waitForAttachment();

                //turn on the antenna and the led to show everything is working
                rfid.Antenna = true;
                rfid.LED = true;
            }
            catch (PhidgetException ex)
            {
                EventLog.WriteEntry("EpiService", ex.Description, EventLogEntryType.Error);
            }
        }

        //attach event handler...display the serial number of the attached RFID phidget
        public void rfid_Attach(object sender, AttachEventArgs e)
        {
            try
            {
                rfid.Antenna = true;
                rfid.LED = true;

                string m = "RFID reader " + e.Device.SerialNumber.ToString() + " attached.";
                this.sendMessage("RFID_ATTACH", m);

                EventLog.WriteEntry("EpiService", m);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("EpiService", ex.Message, EventLogEntryType.Error);
            }
        }

        //detach event handler...display the serial number of the detached RFID phidget
        private void rfid_Detach(object sender, DetachEventArgs e)
        {
            try {
                string m = "RFID reader " + e.Device.SerialNumber.ToString() + " detached.";
                this.sendMessage("RFID_DETACH", m);

                EventLog.WriteEntry("EpiService", m);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("EpiService", ex.Message, EventLogEntryType.Error);
            }
        }

        //Error event handler...display the error description string
        private void rfid_Error(object sender, ErrorEventArgs e)
        {
            try
            {
                // Sending Data to the Client
                this.sendMessage("RFID_ERROR", e.Description);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("EpiService", ex.Message, EventLogEntryType.Error);
            }
        }

        //Print the tag code of the scanned tag
        private void rfid_Tag(object sender, TagEventArgs e)
        {
            try
            {
                // Sending Data to the Client
                this.sendMessage("RFID_TAG", e.Tag.ToString());
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("EpiService", ex.Message, EventLogEntryType.Error);
            }
            //EventLog.WriteEntry("EpiService", "Tag " + e.Tag + " sent");
        }

        //print the tag code for the tag that was just lost
        private void rfid_TagLost(object sender, TagEventArgs e)
        {
            try
            {
                // Sending Data to the Client
                this.sendMessage("RFID_TAGLOST", e.Tag.ToString());
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("EpiService", ex.Message, EventLogEntryType.Error);
            }
            // EventLog.WriteEntry("EpiService", "Tag "+e.Tag+" lost");
        }

        // Send JSON formatted message
        private void sendMessage(string key, string message)
        {
            //Message m = new Message(key, message);
            string output = JsonConvert.SerializeObject(new Message(key, message));
            Context.Send(output);
        }
    }

    class Message
    {
        public string key;
        public string message;

        public Message(string key, string message)
        {
            this.key = key;
            this.message = message;
        }
    }
}
