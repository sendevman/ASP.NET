
### Download Starter Template

Download the starter template with **ASP.NET Core** and **Entity Framework Core** to integrate MySQL. 
[Multi-page template with **ASP.NET Core 2.x** + **.NET Core Framework** + **Authentication**](https://aspnetboilerplate.com/Templates) 
will be explained in this document.

### Getting Started

There are two Entity Framework Core providers for MySQL that are mentioned in the Micrososft Docs. One of them is the
[Official MySQL EF Core Database Provider](https://docs.microsoft.com/en-us/ef/core/providers/mysql/) and the
other is [Pomelo EF Core Database Provider for MySQL](https://docs.microsoft.com/en-us/ef/core/providers/pomelo/).

> **NOTE:** The official provider [`MySql.Data.EntityFrameworkCore`](https://www.nuget.org/packages/MySql.Data.EntityFrameworkCore) [supports EF Core 2.0 after version 6.10.5](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html), but you have to make a lot of changes when using it. So, the Pomelo EF Core Database Provider will be used in this example instead.
> 
> Related issue: https://github.com/sendevman/ASP.NET/issues/4007
>
> Related guide:  https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html

### Install 

Install the [`Pomelo.EntityFrameworkCore.MySql`](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql/) NuGet package to the ***.EntityFrameworkCore** project. 

### Configuration

#### Configure DbContext 

Replace `YourProjectNameDbContextConfigurer.cs` with the following lines

```c#
public static class MySqlDemoDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<MySqlDemoDbContext> builder, string connectionString)
    {
        var serverVersion = ServerVersion.AutoDetect(connectionString);
        builder.UseMySql(connectionString, serverVersion);
    }

    public static void Configure(DbContextOptionsBuilder<MySqlDemoDbContext> builder, DbConnection connection)
    {
        var serverVersion = ServerVersion.AutoDetect(connection.ConnectionString);
        builder.UseMySql(connection, serverVersion);
    }
 }
 ```

Some configuration and workarounds are needed to use MySQL with ASP.NET Core and Entity Framework Core. 

#### Configure connection string 

Change the connection string to your MySQL connection in ***.Web.Mvc/appsettings.json**. Example:

```js
{
  "ConnectionStrings": {
    "Default": "server=127.0.0.1;uid=root;pwd=1234;database=mysqldemodb"
  },
  ...
}

```

#### A workaround

To prevent EF Core from calling `Program.BuildWebHost()` rename `BuildWebHost`. For example, change it to `InitWebHost`. 
To understand why it needs to be renamed, check the following issues:

> **Reason** : [EF Core 2.0: design-time DbContext discovery changes](https://github.com/aspnet/EntityFrameworkCore/issues/9033)
> 
> **Workaround** : [Design: Allow IDesignTimeDbContextFactory to short-circuit service provider creation](https://github.com/aspnet/EntityFrameworkCore/issues/9076#issuecomment-313278753)
>
> **NOTE :** If you don't rename BuildWebHost, you'll get an error running BuildWebHost method.

### Create Database

Remove all migration classes (including DbContextModelSnapshot) under **\*.EntityFrameworkCore/Migrations** folder; generally, you can remove all files in that folder.
Because `Pomelo.EntityFrameworkCore.MySql` will add some of its own configurations to work with Entity Framework Core.

Now it's ready to build the database.

- Select **\*.Web.Mvc** as the startup project.
- Open **Package Manager Console** and select the **\*.EntityFrameworkCore** project.
- Run the `add-migration Initial_Migration` command
- Run the `update-database` command

The MySQL integration is now complete. You can now run your project with MySQL. 

