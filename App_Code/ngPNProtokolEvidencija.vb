Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Microsoft.VisualBasic

Public Class ngPNProtokolEvidencija

    Public Property PnId As Integer
    Public Property PnProtokol As String
    Public Property PnRedBr As Integer
    Public Property PnMjesec As Integer
    Public Property PnGodina As Integer
    Public Property EmployeeId As Integer

    Public Sub Insert()
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
INSERT INTO `putninalog_evidencija` (
  `pn_id`,
  `pn_protokol`,
  `pn_redbr`,
  `pn_mjesec`,
  `pn_godina`,
  `employee_id`
)
VALUES
  (
    @pn_id,
    @pn_protokol,
    @pn_redbr,
    @pn_mjesec,
    @pn_godina,
    @employee_id
  );

    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            BindParams(mycmd)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                'mycmd.LastInsertedId
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using


    End Sub

    Public Sub Update()
        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
UPDATE
  `putninalog_evidencija`
SET
  `pn_id` = @pn_id,
  `pn_protokol` = @pn_protokol,
  `pn_redbr` = @pn_redbr,
  `pn_mjesec` = @pn_mjesec,
  `pn_godina` = @pn_godina,
  `employee_id` = @employee_id
WHERE `pn_id` = @pn_id;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            BindParams(mycmd)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

    End Sub

    Public Sub Delete()
        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
DELETE
FROM
  `putninalog_evidencija`
WHERE `pn_id` = @pn_id;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            BindParams(mycmd)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

    End Sub

    Public Sub BindParams(ByRef mycmd As MySqlCommand)

        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@pn_id", .DbType = Data.DbType.UInt32, .Value = PnId})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@pn_protokol", .DbType = Data.DbType.String, .Value = PnProtokol})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@pn_redbr", .DbType = Data.DbType.Int32, .Value = PnRedBr})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@pn_mjesec", .DbType = Data.DbType.Int32, .Value = PnMjesec})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@pn_godina", .DbType = Data.DbType.Int32, .Value = PnGodina})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@employee_id", .DbType = Data.DbType.Int32, .Value = EmployeeId})

    End Sub


End Class
