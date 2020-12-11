namespace Mall.CommonModel
{
    public interface IManager
    {
        long Id { get; set; }
        long ShopId { get; set; }
        long RoleId { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string PasswordSalt { get; set; }
        System.DateTime CreateDate { get; set; }
        string Description { set; get; }
        string RoleName { get; set; }
    }
}
