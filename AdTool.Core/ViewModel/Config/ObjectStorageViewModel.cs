using CsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace AdTool.Core
{
    public class ObjectStorageViewModel : BaseViewModel
    {

        #region command
        public ICommand SaveCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }
        #endregion

        #region property
        public bool SaveIsRunning { get; set; }
        public string EndPoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Bucket { get; set; }
        public string AgentFolder { get; set; } = "agent";
        #endregion

        #region construtor
        public ObjectStorageViewModel()
        {
            SaveCommand = new RelayParameterizedCommand(async (parameter) => await SaveAsync(parameter));
            PageLoadedCommand = new RelayCommand(async () => await PageLoadedAsync());
        }
        #endregion

        #region event
        public async Task PageLoadedAsync()
        {
            try
            {
                logClientConfig = LogClient.Config.Instance;
                PsFileName = "AD.TXT";
                EndPoint = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint);
                AccessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
                SecretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);
                Bucket = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Bucket);
                AgentFolder = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.AgentFolder);
                userDataTemplate = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.userData);
                psContentsTemplate = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.PsContents);
                mConfigListDesignModel = ConfigListDesignModel.Instance;
            }
            catch (Exception ex)
            {
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                menuItem.ProfilePictureRGB = "5c5c5c";
                await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                {
                    Title = "ERROR",
                    Message = ex.Message,
                    OkText = "OK"
                });
            }
        }
        #endregion

        #region methods

        public async Task SaveAsync(object parameter)
        {
            await RunCommandAsync(() => SaveIsRunning, async () =>
            {
                try
                {
                    if (!(Bucket.Equals(Bucket.ToLower())))
                        throw new Exception("Enter BucketName in lowercase!");
                    if (Bucket.Contains(" "))
                        throw new Exception("Do not enter space in bucketname!");


                    // create bucket
                    ObjectStorage o = new ObjectStorage(AccessKey, SecretKey, EndPoint);
                    if (o.CreateBucket(Bucket))
                    {
                        dataManager.SetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint, EndPoint);
                        dataManager.SetValue(DataManager.Category.ObjectStorage, DataManager.Key.Bucket, Bucket);
                        dataManager.SaveUserData();
                    }
                    // wait for work!
                    await Task.Delay(2000);
                    
                    // upload script
                    TemplateChange();
                    await ObjectStorageBucketCheck();
                    UserDataFinalSave();
                    PsContentsTemplateChangedLocalSave();
                    UpLoadFile2ObjectStorage(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config/Upload"), true);

                    // wait for work!
                    await Task.Delay(2000);
                    // verify script
                    var userData = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.userDataFinal);
                    if (userData.Length < 1)
                        throw new Exception("userData does not saved! (vb script)");

                    if (!userData.Contains(PsFileName))
                        throw new Exception("userData does not saved! (powershell script)");

                    var powerShellContents = await DownloadPowerShellScript();

                    if (!powerShellContents.Contains(AgentFolder))
                        throw new Exception("userData does not saved! (powershell agentFolder veriry failed)");


                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "SUCCESS",
                        Message = "objectstorage bucket creation and initscript uploaded.",
                        OkText = "OK"
                    });
                    var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                    menuItem.ProfilePictureRGB = "3483eb";
                }
                catch (Exception ex)
                {
                    var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                    menuItem.ProfilePictureRGB = "5c5c5c";
                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "ERROR",
                        Message = $"try again! ({ex.Message})",
                        OkText = "OK"
                    });
                }
            });
        }


        private async Task<string> DownloadPowerShellScript()
        {
            string text = string.Empty;
            try
            {
                WebClient client = new WebClient();
                Task<byte[]> task = client.DownloadDataTaskAsync(string.Format("{0}/{1}/{2}", EndPoint, Bucket, "AD.TXT"));
                var contents = await task;
                text = Encoding.Default.GetString(contents);
                if (text.Length < 1)
                    new Exception("remote powershell script error");
            }
            catch (Exception)
            {
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                menuItem.ProfilePictureRGB = "5c5c5c";
                throw;
            }
            return text;
        }
        private async Task ObjectStorageBucketCheck()
        {
            try
            {
                ObjectStorage o = new ObjectStorage(
                    logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey),
                    logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey),
                    dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint)
                    );
                if (!await o.IsExistsBucket(Bucket))
                    throw new Exception("object storage bucket does not exists");
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void UserDataFinalSave()
        {
            try
            {
                dataManager.SetValue(DataManager.Category.InitScript, DataManager.Key.AgentFolder, AgentFolder);
                dataManager.SetValue(DataManager.Category.InitScript, DataManager.Key.userDataFinal, userDataTemplateChanged);
                dataManager.SaveUserData();
            }
            catch (Exception)
            {
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                menuItem.ProfilePictureRGB = "5c5c5c";
                throw;
            }
        }
        private void PsContentsTemplateChangedLocalSave()
        {
            try
            {
                string powerShellFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config/Upload", PsFileName);
                string path = System.IO.Path.GetDirectoryName(powerShellFileName);
                if (File.Exists(powerShellFileName))
                    File.Delete(powerShellFileName);
                File.WriteAllText(powerShellFileName, psContentsTemplateChanged);
            }
            catch (Exception)
            {
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                menuItem.ProfilePictureRGB = "5c5c5c";
                throw;
            }
        }
        private async void UpLoadFile2ObjectStorage(string folder, bool publicReadTrueOrFalse = false)
        {
            uploadCancellationTokenSources = new List<CancellationTokenSource>();
            List<Task> tasks = new List<Task>();
            string[] files = Directory.GetFiles(folder);
            foreach (string f in files)
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                uploadCancellationTokenSources.Add(cancelTokenSource);
                ObjectStorage o = new ObjectStorage(
                    logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey),
                    logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey),
                    dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint)
                    );
                tasks.Add(o.UploadObjectAsync(Bucket, Path.GetFullPath(f), Path.GetFileName(f), cancelTokenSource.Token, 0, publicReadTrueOrFalse));
            }
            await Task.WhenAll(tasks);
        }
        private void TemplateChange()
        {
            string vbTemplate = string.Empty;
            vbTemplate = userDataTemplate.Replace("DP_BUCKET_DP", Bucket);
            vbTemplate = vbTemplate.Replace("DP_PSFILENAME_DP", PsFileName);
            vbTemplate = vbTemplate.Replace("DP_OJBJECT_ENDPOINT_DP", EndPoint);
            userDataTemplateChanged = vbTemplate;

            string psTemplate = string.Empty;
            psTemplate = psContentsTemplate.Replace("DP_AGENT_FOLDER_DP", AgentFolder);
            psTemplate = psTemplate.Replace("DP_BUCKET_DP", Bucket);
            psTemplate = psTemplate.Replace("DP_OJBJECT_ENDPOINT_DP", EndPoint);
            psContentsTemplateChanged = psTemplate;
        }

        #endregion

        #region etc
        LogClient.Config logClientConfig;
        DataManager dataManager = DataManager.Instance;
        List<CancellationTokenSource> uploadCancellationTokenSources;
        ConfigListDesignModel mConfigListDesignModel;

        private string PsFileName { get; set; }

        private string userDataTemplate = string.Empty;

        private string psContentsTemplate = string.Empty;
        private string userDataTemplateChanged { get; set; } = string.Empty;
        private string psContentsTemplateChanged { get; set; } = string.Empty;
        #endregion
    }
}