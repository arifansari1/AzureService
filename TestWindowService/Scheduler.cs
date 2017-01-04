using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.IO;

namespace TestWindowService
{
    public partial class Scheduler : ServiceBase
    {
        private Timer timer1 = null;

        public Scheduler()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                timer1 = new Timer();
                this.timer1.Interval = 30000; //every 30 secs
                this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
                timer1.Enabled = true;
                Library.WriteErrorLog("Test window service started");
                // Retreiving data from App.config file
                string cxn = ConfigurationManager.ConnectionStrings["AzureStorageConn"].ConnectionString; // cxn is the connection string
                string localFolder = ConfigurationManager.AppSettings["sourceFolder"]; // localFolder is the connection string for the folder having files for upload

                // Creating the blob storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cxn);

                // Creating the blob storage client
                CloudBlobClient client = storageAccount.CreateCloudBlobClient();

                // Creating the container
                CloudBlobContainer container = client.GetContainerReference("documents");
                container.CreateIfNotExists();

                //After creating the container we can upload
                string[] entries = Directory.GetFiles(localFolder); // we want all files in the source folder

                foreach (string file in entries)
                {
                    string key = Path.GetFileName(file); // get file name with extension
                    CloudBlockBlob myblob = container.GetBlockBlobReference(key);

                    using (var f = System.IO.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None)) //open file stream with read capability
                    {
                        myblob.UploadFromStream(f);
                    }
                    Library.WriteErrorLog("File Uploaded successfully in to Azure");
                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(ex);
            }
        }


        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            //Write code here to do some job depends on your requirement
            Library.WriteErrorLog("Timer ticked and file upload has been done successfully");
        }

        public void download()
        {
        }
        protected override void OnStop()
        {
            timer1.Enabled = false;
            Library.WriteErrorLog("Test window service stopped");
        }
    }
}
