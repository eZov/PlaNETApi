Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Microsoft.VisualBasic

Public Class ngPNProtokolPrava

    Public Property Id As Integer
    Public Property ProtokolId As Integer
    Public Property EmployeeId As Integer
    Public Property CreatedBy As Integer
    Public Property RoleBy As String

    Public Sub Insert()
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
INSERT INTO `putninalog_protokol` (
  `protokol_id`,
  `employee_id`,
  `created_by`,
  `role_by`
)
VALUES
  (
    @protokol_id,
    @employee_id,
    @created_by,
    @role_by
  );

    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            BindParams(mycmd)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Id = mycmd.LastInsertedId
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using


    End Sub

    Public Sub Update()
        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
UPDATE
  `putninalog_protokol`
SET
  `protokol_id` = @protokol_id,
  `employee_id` = @employee_id,
  `created_by` = @created_by,
  `role_by` = @role_by
WHERE `id` = @id;
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
  `putninalog_protokol`
WHERE `id` = @id;
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

        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@id", .DbType = Data.DbType.UInt32, .Value = Id})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@protokol_id", .DbType = Data.DbType.Int32, .Value = ProtokolId})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@employee_id", .DbType = Data.DbType.Int32, .Value = EmployeeId})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@created_by", .DbType = Data.DbType.Int32, .Value = CreatedBy})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@role_by", .DbType = Data.DbType.String, .Value = RoleBy})


    End Sub



End Class
