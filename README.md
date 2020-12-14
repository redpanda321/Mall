# Mall
 B2B2C Ecommercial Shopping System Base on NopCommerce 


A.Installation

1. .net core 3.1  &&  MySql 8.0.17


2. Initial  NopCommerce 4.30 first

   Mall.Web/App_Data/dataSettings.json
   
   {
  "DataProvider": "sqlserver",
  "DataConnectionString": "Data Source=localhost\\SQLEXPRESS;Database=shop43;Integrated Security=True;Persist Security Info=True;Trusted_Connection=False;MultipleActiveResultSets=True;",
  "RawDataSettings": {}
}




3. Create Mall 4.30  MySql Database

    Sql file at  Mall.Web/SqlScripts/Mall33.sql
    
4.  Mall 4.30  Configuation

    Mall.Web/appsettings.json

   Mall:ConnectionString": "server=localhost;user id=root;password=xxxxxxxxx;persistsecurityinfo=True;database=mall33;Allow User Variables=True;SslMode=none"
   
      

B.Logical Structure


Administrator

    Commodity
    Transaction
    Member
    Shop
    Statistics
    Website
    Distribution
    System
    Marketing
    Micro Mall
    Application
    Applets

 SellerAdmin
 
    Merchant settled
    Commodity
    Product release
    Commodity management
    Transaction
    Order
    Return
    Freight template
    Payment method
    Shop
    Store management
    Customer Service Management
    Financial Management
    Ticket management
    Margin management
    Store management
    Statistics
    Marketing   
    Distribution
    WeChat
    WeChat Store
      

Personal center

 
    Order center
      
      My Order
      Consulting management
      Evaluation Management

    Asset Center
  
    My discount coupon
    My scores
    My account

    My focus
     
     Commodity attention
     Shop attention

    Promote
     
     Platform promotion
     After-sales service
     Return
     Complaint

    Account management
      
      Personal information
      Account security management
      SMS 
         

    WeChat public account
     
     WeChat Store
     Mobile phone shop wap
     Mobile Store App

    Customer Center

    Help
    After sale
    Complaint
