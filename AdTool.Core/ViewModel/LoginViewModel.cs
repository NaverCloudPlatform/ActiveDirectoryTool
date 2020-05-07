using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdTool.Core;
using CsLib;
using Newtonsoft.Json;

namespace AdTool.Core
{
    public class LoginViewModel : BaseViewModel
    {
        #region command
        public ICommand LoginCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }
        public ICommand LoginTypeChangeCommand { get; set; }
        #endregion

        #region property
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public bool IsSave { get; set; }
        public bool LoginIsRunning { get; set; }
        public bool LoginTypeDefault { get; set; }
        public bool LoginTypeGov { get; set; }
        #endregion

        #region constructor
        public LoginViewModel()
        {
            PageLoadedCommand = new RelayCommand(async () => await LoginPageLoaded());
            LoginCommand = new RelayParameterizedCommand(async (parameter) => await LoginAsync(parameter));
            CancelCommand = new RelayParameterizedCommand((parameter) => Cancel(parameter));
            LoginTypeChangeCommand = new RelayParameterizedCommand((parameter) => LoginTypeChange(parameter));
        } 
        #endregion

        #region event
        private async Task LoginPageLoaded()
        {
            try
            {
                logClientConfig = LogClient.Config.Instance;
                dataManager = DataManager.Instance;
                string isSaveKeyYn = dataManager.GetValue(DataManager.Category.Login, DataManager.Key.IsSaveKeyYn);
                EndPoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                

                if (isSaveKeyYn.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    IsSave = true;
                    AccessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
                    SecretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);
                }
                else
                    IsSave = false;

                if (dataManager.LoginType == LoginType.GOV)
                {
                    LoginTypeDefault = false;
                    LoginTypeGov = true;
                }
                else
                {
                    LoginTypeDefault = true;
                    LoginTypeGov = false;
                }


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

        private void LoginTypeChange(object parameter)
        {
            var loginTypeName = parameter.ToString();
            if (loginTypeName.Equals("LoginTypeGov"))
                dataManager.LoginType = LoginType.GOV;
            else
                dataManager.LoginType = LoginType.Default;
            EndPoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
            dataManager.SaveUserData();

        }
        private async Task LoginAsync(object parameter)
        {
            await RunCommandAsync(() => LoginIsRunning, async () =>
            {
                try
                {
                    var SecretKey = (parameter as IHavePassword).SecurePassword.Unsecure();
                    List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                    parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                    SoaCall soaCall = new SoaCall();
                    var task = soaCall.WebApiCall(EndPoint, RequestType.POST, @"/server/v2/getRegionList", parameters, AccessKey, SecretKey);
                    string response = await task;

                    JsonSerializerSettings options = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    if (response.Contains("responseError"))
                    {
                        hasError hasError = JsonConvert.DeserializeObject<hasError>(response, options);
                        throw new Exception(hasError.responseError.returnMessage);
                    }

                    if (response.Contains("error"))
                    {
                        authError authError = JsonConvert.DeserializeObject<authError>(response, options);
                        throw new Exception(authError.error.message);
                    }

                    getRegionList getRegionList = JsonConvert.DeserializeObject<getRegionList>(response, options);
                    if (!getRegionList.getRegionListResponse.returnCode.Equals("0"))
                        throw new Exception(response);

                    if (IsSave)
                    {
                        dataManager.SetValue(DataManager.Category.Login, DataManager.Key.IsSaveKeyYn, "Y");
                        logClientConfig.SetValue(LogClient.Category.Api, LogClient.Key.AccessKey, AccessKey.Trim());
                        logClientConfig.SetValue(LogClient.Category.Api, LogClient.Key.SecretKey, SecretKey.Trim());
                        dataManager.SaveUserData();
                        logClientConfig.SaveLogClientData();
                    }
                    else
                    {
                        dataManager.SetValue(DataManager.Category.Login, DataManager.Key.IsSaveKeyYn, "N");
                        logClientConfig.SetValue(LogClient.Category.Api, LogClient.Key.AccessKey, "");
                        logClientConfig.SetValue(LogClient.Category.Api, LogClient.Key.SecretKey, "");
                        logClientConfig.SaveLogClientData();
                        logClientConfig.SetValue(LogClient.Category.Api, LogClient.Key.AccessKey, AccessKey.Trim());
                        logClientConfig.SetValue(LogClient.Category.Api, LogClient.Key.SecretKey, SecretKey.Trim());
                        dataManager.SaveUserData();
                    }
                    IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.ObjectStorage);
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

        private void Cancel(object parameter)
        {
            var windowFacade = parameter as IWindowFacade;
            windowFacade?.Close();
        }
        #endregion

        #region etc
        DataManager dataManager;
        LogClient.Config logClientConfig;
        private string EndPoint { get; set; } 
        #endregion
    }
}