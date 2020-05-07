namespace AdTool.Core
{

    public class ServerInstanceItem
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }
        public string PublicIp { get; set; }
        public string PrivateIp { get; set; }
        public string ZoneNo { get; set; }
        public string InstanceNo { get; set; }
        public string Status { get; set; }
        public string Operation { get; set; }
    }


    public class PublicIpInstanceItem
    {
        public bool IsChecked { get; set; }
        public string InstanceNo { get; set; }
        public string PublicIp { get; set; }
        public string ServerInstanceNo { get; set; }
        public string ServerName { get; set; }
        public string Status { get; set; }
        public string Operation { get; set; }
    }


    public class ServerOperationComboBox
    {
        public string Display { get; set; }
    }

    public class PublicIpOperationComboBox
    {
        public string Display { get; set; }
    }

    public class AdGroupItem
    {
        public bool IsChecked { get; set; }
        public string GroupName { get; set; }
        public string MasterServerName { get; set; }
        public string MasterServerPublicIp { get; set; }
        public string MasterServerInstanceNo { get; set; }
        public string SlaveServerName { get; set; }
        public string SlaveServerPublicIp { get; set; }
        public string SlaveServerInstanceNo { get; set; }
    }

    public class DomianMode
    {
        public string Display { get; set; }
    }
}


