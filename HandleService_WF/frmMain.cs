using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Xml.Serialization;
using System.Xml;
using System.ServiceProcess;

namespace HandleService_WF
{
    public partial class frmMain : Form
    {
        private static List<ServiceAttributes> ListOfServices = new List<ServiceAttributes>();
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //Enable the timer
            timer1.Enabled = true;
        }

        #region Configuration
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            LoadXMLFile();

        }

        private void LoadXMLFile()
        {
            //
            FileInfo confFile = new FileInfo(ConfigurationManager.AppSettings["ServiceFile"]);

            if(confFile.Exists)
            {
                lblStatus.Text = "List of services is found";
                cmdOn.Enabled = false;
                cmdOff.Enabled = false;


                XmlSerializer ser = new XmlSerializer(typeof(Servers));
                Servers sList;
                using (XmlReader reader = XmlReader.Create(confFile.FullName))
                    sList = (Servers)ser.Deserialize(reader);

                foreach(var currentServer in sList.Items )
                {                    
                    foreach (var currentService in currentServer.Service)
                    {
                        ServiceAttributes t = new ServiceAttributes();
                        t.ServerName = currentServer.Name;
                        t.ServiceName = currentService.Value;
                        t.ServiceStatus = "";

                        ListOfServices.Add(t);
                    }
                }

                dgvServices.DataSource = ListOfServices;

                lblStatus.Text = "Updating Services";
                bwWorker.RunWorkerAsync('U');
            }
            else
            {
                lblStatus.Text = "No list of services found";
                cmdOn.Enabled = false;
                cmdOff.Enabled = false;
            }
        }
        #endregion

        #region Worker
        private void bwWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            switch(e.Argument)
            {
                case 'U':
                    break;
                case 'N':
                    break;
                case 'F':
                    break;
                default:
                    break;
            }

            int numRows = dgvServices.Rows.Count;

            for(int i = 0; i < dgvServices.Rows.Count; i++)
            {                   
                bwWorker.ReportProgress(i, dgvServices.Rows[i].Cells[0]);

                ServiceController mc = new ServiceController(ListOfServices[i].ServiceName, ListOfServices[i].ServerName);

                try
                {

                    ListOfServices[i].ServiceStatus = mc.Status.ToString();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message.Trim(), "Nope", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                mc.Close();

                System.Threading.Thread.Sleep(2000);
            }
        }

        private void bwWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.dgvServices.FirstDisplayedScrollingRowIndex = e.ProgressPercentage;
            lblStatus.Text = $"Updating Service: {e.UserState}";
        }

        private void bwWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            cmdOff.Enabled = true;
            cmdOn.Enabled = true;
            lblStatus.Text = "Complete";
        }
        #endregion

        #region Buttons
        private void cmdOn_Click(object sender, EventArgs e)
        {
            cmdOff.Enabled = false;
            cmdOn.Enabled = false;
            this.bwWorker.RunWorkerAsync('N');
        }

        private void cmdOff_Click(object sender, EventArgs e)
        {
            cmdOff.Enabled = false;
            cmdOn.Enabled = false;
            this.bwWorker.RunWorkerAsync('F');
        }

        private void cmdExit_Click(object sender, EventArgs e)
        {
            if (bwWorker.IsBusy)
                this.bwWorker.CancelAsync();

            Application.Exit();
        }
        #endregion
    }

    #region Classes
    class ServiceAttributes
    {
        public string ServerName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceStatus { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Servers
    {

        private ServersServer[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Server", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ServersServer[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ServersServer
    {

        private ServersServerService[] serviceField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Service", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public ServersServerService[] Service
        {
            get
            {
                return this.serviceField;
            }
            set
            {
                this.serviceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ServersServerService
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
    #endregion
}
