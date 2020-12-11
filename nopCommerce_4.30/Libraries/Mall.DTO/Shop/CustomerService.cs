namespace Mall.DTO
{
    public class CustomerService
	{
		public long Id { get; set; }
		public long ShopId { get; set; }
		public Entities.CustomerServiceInfo.ServiceTool Tool { get; set; }
		public Entities.CustomerServiceInfo.ServiceType Type { get; set; }
		public string Name { get; set; }
		public string AccountCode { get; set; }
		public Entities.CustomerServiceInfo.ServiceTerminalType TerminalType { get; set; }
		public Entities.CustomerServiceInfo.ServiceStatusType ServerStatus { get; set; }
	}
}
