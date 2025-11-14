using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient; 
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data; 
using System.Linq;

public class MySqlConnectionSingleton
{
    private readonly string _connectionString;

    public MySqlConnectionSingleton(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("UsuariosConnection");
    }

    public MySqlConnection CreateConnection()
        => new MySqlConnection(_connectionString);
}

