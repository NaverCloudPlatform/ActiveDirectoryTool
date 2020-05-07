using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AdTool.Core
{
    public class SetAdGroupViewModel : BaseViewModel
    {
        #region command
        public ICommand ServerReloadCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }
        public ICommand CreateGroupCommand { get; set; }
        public ICommand AdGroupDeleteCommand { get; set; }
        public ICommand AdGroupReloadCommand { get; set; }
        #endregion

        #region property
        public string AdGroupName { get; set; } = "";
        public bool CreateGroupIsRunning { get; set; }
        public bool AdGroupDelelteIsRunning { get; set; }
        public bool AdGroupReloadIsRunning { get; set; }
        public bool ServerReloadIsRunning { get; set; }
        public ObservableCollection<ServerInstanceItem> AdMasterServerComboBoxItems { get; set; }
        public ServerInstanceItem SelectedAdMasterServerComboBoxItem { get; set; }
        public ObservableCollection<ServerInstanceItem> AdSlaveServerComboBoxItems { get; set; }
        public ServerInstanceItem SelectedAdSlaveServerComboBoxItem { get; set; }
        public ObservableCollection<AdGroupItem> AdGroupItems { get; set; }
        #endregion

        #region constructor
        public SetAdGroupViewModel()
        {
            AdMasterServerComboBoxItems = new ObservableCollection<ServerInstanceItem>();
            AdSlaveServerComboBoxItems = new ObservableCollection<ServerInstanceItem>();
            AdGroupItems = new ObservableCollection<AdGroupItem>();
            CreateGroupCommand = new RelayCommand(async () => await CreateGroupAsync());
            ServerReloadCommand = new RelayCommand(async () => await ServerReloadAsync());
            AdGroupDeleteCommand = new RelayCommand(async () => await AdGroupDeleteAsync());
            AdGroupReloadCommand = new RelayCommand(async () => await AdGroupReloadAsync());
            PageLoadedCommand = new RelayCommand(async () => await PageLoadedAsync());
        }
        #endregion
        
        #region event
        public async Task PageLoadedAsync()
        {
            try
            {
                await ServerReloadAsync();
                await AdGroupReloadAsync();
                logClientConfig = LogClient.Config.Instance;
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
        public async Task AdGroupDeleteAsync()
        {
            await RunCommandAsync(() => AdGroupDelelteIsRunning, async () =>
            {
                try
                {
                    List<string> instanceNoList = new List<string>();
                    foreach (var item in AdGroupItems)
                    {
                        if (item.IsChecked)
                            instanceNoList.Add(item.GroupName);
                    }
                    if (instanceNoList.Count == 0)
                        throw new Exception("Select AD Group to delete");

                    foreach (var AdGroupItem in AdGroupItems.ToList())
                    {

                        if (AdGroupItem.IsChecked)
                        {
                            try
                            {
                                var p = new List<KeyValuePair<string, string>>();
                                p.Add(new KeyValuePair<string, string>("clusterName", AdGroupItem.GroupName));
                                p.Add(new KeyValuePair<string, string>("serverName", AdGroupItem.MasterServerName));
                                await fileDb.DeleteTable(FileDb.TableName.TBL_CLUSTER_SERVER, p);
                            }
                            catch { }
                            try
                            {
                                var p = new List<KeyValuePair<string, string>>();
                                p.Add(new KeyValuePair<string, string>("clusterName", AdGroupItem.GroupName));
                                p.Add(new KeyValuePair<string, string>("serverName", AdGroupItem.SlaveServerName));
                                await fileDb.DeleteTable(FileDb.TableName.TBL_CLUSTER_SERVER, p);
                            }
                            catch { }
                            try
                            {
                                var p = new List<KeyValuePair<string, string>>();
                                p.Add(new KeyValuePair<string, string>("clusterName", AdGroupItem.GroupName));
                                await fileDb.DeleteTable(FileDb.TableName.TBL_CLUSTER, p);
                            }
                            catch { }
                            AdGroupItems.Remove(AdGroupItem);
                        }
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
            });
        }

        public async Task ServerReloadAsync()
        {

            await RunCommandAsync(() => ServerReloadIsRunning, async () =>
            {
                try
                {
                    await fileDb.ReadTable(FileDb.TableName.TBL_SERVER);
                    List<string> instanceNoList = new List<string>();
                    foreach (var a in fileDb.TBL_SERVER.Data)
                    {
                        if (a.Value.serverInstanceNo != "NULL")
                            instanceNoList.Add(a.Value.serverInstanceNo);
                    }

                    List<serverInstance> serverInstances = new List<serverInstance>();
                    try
                    {
                        serverInstances = await ServerOperation.GetServerInstanceList(instanceNoList);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("server not found"))
                        { }
                        else
                            throw new Exception(ex.Message);
                    }

                    try
                    {
                        List<string> deleteServerNameList = new List<string>();

                        AdMasterServerComboBoxItems.Clear();
                        AdSlaveServerComboBoxItems.Clear();

                        foreach (var a in fileDb.TBL_SERVER.Data)
                        {
                            var serverInstance = serverInstances.Find(x => x.serverName == a.Key.serverName);
                            if (serverInstance != null)
                            {
                                var item = new ServerInstanceItem
                                {
                                    IsChecked = false,
                                    Name = a.Key.serverName,
                                    PublicIp = serverInstance.publicIp,
                                    PrivateIp = a.Value.serverPrivateIp,
                                    ZoneNo = a.Value.zoneNo + "(" + serverInstance.zone.zoneCode + ")",
                                    InstanceNo = a.Value.serverInstanceNo,
                                    Status = serverInstance.serverInstanceStatus.code,
                                    Operation = serverInstance.serverInstanceOperation.code
                                };
                                AdMasterServerComboBoxItems.Add(item);
                                AdSlaveServerComboBoxItems.Add(item);
                            }
                            else
                                deleteServerNameList.Add(a.Key.serverName);
                        }
                        foreach (var a in deleteServerNameList)
                        {
                            var p = new List<KeyValuePair<string, string>>();
                            p.Add(new KeyValuePair<string, string>("serverName", a));
                            await fileDb.DeleteTable(FileDb.TableName.TBL_SERVER, p);
                        }
                        var tempitem = new ServerInstanceItem
                        {
                            IsChecked = false,
                            Name = "",
                            PublicIp = "",
                            PrivateIp = "",
                            ZoneNo = "",
                            InstanceNo = "",
                            Status = "",
                            Operation = ""
                        };
                        AdSlaveServerComboBoxItems.Add(tempitem);
                        SelectedAdMasterServerComboBoxItem = AdMasterServerComboBoxItems.FirstOrDefault();
                        SelectedAdSlaveServerComboBoxItem = AdSlaveServerComboBoxItems.FirstOrDefault();
                    }
                    catch { }
                }
                catch
                { }
            });
        }

        private async Task CreateGroupAsync()
        {
            await RunCommandAsync(() => CreateGroupIsRunning, async () =>
            {
                try
                {
                    if (AdGroupName.Length < 1)
                        throw new Exception("AD Grouup Name does not exist.");
                    if (SelectedAdMasterServerComboBoxItem.Name.Equals(SelectedAdSlaveServerComboBoxItem.Name))
                        throw new Exception("Master and Slave server names cannot be the same.");

                    // read 
                    await fileDb.ReadTable(FileDb.TableName.TBL_CLUSTER);
                    // check
                    foreach (var cluster in fileDb.TBL_CLUSTER.Data)
                    {
                        if (AdGroupName.Equals(cluster.Key.clusterName, StringComparison.OrdinalIgnoreCase))
                            throw new Exception("There is already an AD Group with the same name.");
                    }
                    // insert tbl_cluster
                    var clusterInfo = new List<KeyValuePair<string, string>>();
                    clusterInfo.Add(new KeyValuePair<string, string>("clusterName", AdGroupName));
                    await fileDb.UpSertTable(FileDb.TableName.TBL_CLUSTER, clusterInfo);

                    // read tbl_clster_server
                    await fileDb.ReadTable(FileDb.TableName.TBL_CLUSTER_SERVER);
                    // insert tbl_cluster_server
                    var serverInfo = new List<KeyValuePair<string, string>>();
                    serverInfo.Add(new KeyValuePair<string, string>("clusterName", AdGroupName));
                    serverInfo.Add(new KeyValuePair<string, string>("serverName", SelectedAdMasterServerComboBoxItem.Name));
                    serverInfo.Add(new KeyValuePair<string, string>("serverRole", "MASTER"));
                    await fileDb.UpSertTable(FileDb.TableName.TBL_CLUSTER_SERVER, serverInfo);

                    serverInfo = new List<KeyValuePair<string, string>>();
                    serverInfo.Add(new KeyValuePair<string, string>("clusterName", AdGroupName));
                    serverInfo.Add(new KeyValuePair<string, string>("serverName", SelectedAdSlaveServerComboBoxItem.Name));
                    serverInfo.Add(new KeyValuePair<string, string>("serverRole", "SLAVE"));
                    await fileDb.UpSertTable(FileDb.TableName.TBL_CLUSTER_SERVER, serverInfo);
                    await ReloadAdGroupItemInfo();
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

        private async Task AdGroupReloadAsync()
        {
            await RunCommandAsync(() => AdGroupReloadIsRunning, async () =>
            {
                try
                {
                    await ReloadAdGroupItemInfo();
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

        private async Task ReloadAdGroupItemInfo()
        {
            try
            {
                await fileDb.ReadTable(FileDb.TableName.TBL_CLUSTER);
                await fileDb.ReadTable(FileDb.TableName.TBL_CLUSTER_SERVER);
                await fileDb.ReadTable(FileDb.TableName.TBL_SERVER);

                List<string> instanceNoList = new List<string>();
                foreach (var a in fileDb.TBL_SERVER.Data)
                {
                    if (a.Value.serverInstanceNo != "NULL")
                        instanceNoList.Add(a.Value.serverInstanceNo);
                }

                List<serverInstance> serverInstances = new List<serverInstance>();
                try
                {
                    serverInstances = await ServerOperation.GetServerInstanceList(instanceNoList);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("server not found"))
                    { }
                    else
                        throw new Exception(ex.Message);
                }

                AdGroupItems.Clear();
                foreach (var cluster in fileDb.TBL_CLUSTER.Data)
                {
                    var adGroupItem = new AdGroupItem();
                    adGroupItem.IsChecked = false;
                    adGroupItem.GroupName = cluster.Key.clusterName;
                    foreach (var cluster_server in fileDb.TBL_CLUSTER_SERVER.Data)
                    {
                        if (cluster_server.Key.clusterName.Equals(adGroupItem.GroupName, StringComparison.OrdinalIgnoreCase) && cluster_server.Value.serverRole.Equals("MASTER"))
                        {
                            try
                            {
                                adGroupItem.MasterServerName = cluster_server.Key.serverName;
                                var serverInstance = serverInstances.Find(x => x.serverName == cluster_server.Key.serverName);
                                adGroupItem.MasterServerPublicIp = serverInstance.publicIp;
                                adGroupItem.MasterServerInstanceNo = fileDb.TBL_SERVER.Data[new TBL_SERVER_KEY { serverName = cluster_server.Key.serverName }].serverInstanceNo;
                            }
                            catch { }
                        }
                        if (cluster_server.Key.clusterName.Equals(adGroupItem.GroupName, StringComparison.OrdinalIgnoreCase) && cluster_server.Value.serverRole.Equals("SLAVE"))
                        {
                            try
                            {
                                adGroupItem.SlaveServerName = cluster_server.Key.serverName;
                                var serverInstance = serverInstances.Find(x => x.serverName == cluster_server.Key.serverName);
                                adGroupItem.SlaveServerPublicIp = serverInstance.publicIp;
                                adGroupItem.SlaveServerInstanceNo = fileDb.TBL_SERVER.Data[new TBL_SERVER_KEY { serverName = cluster_server.Key.serverName }].serverInstanceNo;
                            }
                            catch { }
                        }
                    }
                    AdGroupItems.Add(adGroupItem);
                }
            }
            catch { }
        }
        #endregion

        #region private
        FileDb fileDb = FileDb.Instance;
        LogClient.Config logClientConfig;
        DataManager dataManager = DataManager.Instance;
        #endregion
    }
}