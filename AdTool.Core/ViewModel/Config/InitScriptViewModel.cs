using CsLib;
using LogClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using AdTool.Core;

namespace AdTool.Core
{
    public class InitScriptViewModel : BaseViewModel
    {
        /// <summary>
        ///  removed !! 
        /// </summary>


        #region command
        public ICommand UploadCommand { get; set; }
        public ICommand VerifyCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }
        #endregion

        #region property
        public bool UploadIsRunning { get; set; }
        public bool VerifyIsRunning { get; set; }
        public string AgentFolder { get; set; }
        #endregion

        #region constructor
        public InitScriptViewModel()
        {
            UploadCommand = new RelayParameterizedCommand(async (parameter) => await UploadAsync(parameter));
            VerifyCommand = new RelayParameterizedCommand(async (parameter) => await VerifyAsync(parameter));
            PageLoadedCommand = new RelayCommand(async () => await PageLoadedAsync());
        }
        #endregion

        #region Event
        private async Task PageLoadedAsync()
        {
            try
            {
                PsFileName = "AD.TXT";
                logClientConfig = LogClient.Config.Instance;
                EndPoint = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint);
                AccessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
                SecretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);
                Bucket = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Bucket);
                AgentFolder = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.AgentFolder);
                userDataTemplate = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.userData);
                psContentsTemplate = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.PsContents);
            }
            catch (Exception ex)
            {
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

        private async Task UploadAsync(object parameter)
        {
            await RunCommandAsync(() => UploadIsRunning, async () =>
            {
                try
                {
                    TemplateChange();
                    await ObjectStorageBucketCheck();
                    UserDataFinalSave();
                    PsContentsTemplateChangedLocalSave();
                    UpLoadFile2ObjectStorage(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config/Upload"), true);
                    await Task.Delay(1000);

                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "Success",
                        Message = "Upload Completed!",
                        OkText = "OK"
                    });
                }
                catch (Exception ex)
                {
                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "ERROR",
                        Message = ex.Message,
                        OkText = "OK"
                    });
                }
                finally
                {
                    uploadCancellationTokenSources.Clear();
                }
            });
        }
        private async Task VerifyAsync(object parameter)
        {
            await RunCommandAsync(() => VerifyIsRunning, async () =>
            {
                try
                {
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
                        Title = "Success",
                        Message = "Verify Completed!",
                        OkText = "OK"
                    });
                }
                catch (Exception ex)
                {
                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "ERROR",
                        Message = ex.Message,
                        OkText = "OK"
                    });
                }
            });
        }

        #endregion

        #region etc

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
                throw;
            }
            return text;
        }
        private async Task ObjectStorageBucketCheck()
        {
            try
            {
                ObjectStorage o = new ObjectStorage(
                    logClientConfig.GetValue(Category.Api, Key.AccessKey),
                    logClientConfig.GetValue(Category.Api, Key.SecretKey),
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
                    logClientConfig.GetValue(Category.Api, Key.AccessKey),
                    logClientConfig.GetValue(Category.Api, Key.SecretKey),
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

        LogClient.Config logClientConfig;
        DataManager dataManager = DataManager.Instance;
        List<CancellationTokenSource> uploadCancellationTokenSources;

        private string EndPoint { get; set; }
        private string AccessKey { get; set; }
        private string SecretKey { get; set; }
        private string Bucket { get; set; }
        private string PsFileName { get; set; }

        private string userDataTemplate = string.Empty;

        private string psContentsTemplate = string.Empty;
        private string userDataTemplateChanged { get; set; } = string.Empty;
        private string psContentsTemplateChanged { get; set; } = string.Empty;
        #endregion
    }
}