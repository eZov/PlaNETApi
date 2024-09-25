Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Microsoft.VisualBasic

Public Class ngPNProtokolPravaQuery


    Public Function FindOne(ByVal pId As Integer) As ngPNProtokolPrava
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `id`,
  `protokol_id`,
  `employee_id`,
  `created_by`,
  `role_by`
FROM
  `putninalog_protokol`
WHERE  `id` = @id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@id", .DbType = Data.DbType.UInt32, .Value = pId})
            mycmd.Prepare()

            Try
                Dim dbRead As MySqlDataReader
                dbRead = mycmd.ExecuteReader()
                If dbRead.HasRows Then
                    While dbRead.Read()
                        Dim _response = New ngPNProtokolPrava With {
                        .Id = dbRead.GetInt32("id"),
                        .ProtokolId = dbRead.GetInt32("protokol_id"),
                        .EmployeeId = dbRead.GetInt32("employee_id"),
                        .CreatedBy = dbRead.GetInt32("created_by"),
                        .RoleBy = dbRead.GetString("role_by")
                        }

                        Return _response
                    End While
                End If


            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return Nothing

    End Function

    Public Function FindByProtokol(ByVal pProtokolId As Integer) As List(Of ngPNProtokolPrava)
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `id`,
  `protokol_id`,
  `employee_id`,
  `created_by`,
  `role_by`
FROM
  `putninalog_protokol`
WHERE  `protokol_id` = @protokol_id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@protokol_id", .DbType = Data.DbType.UInt32, .Value = pProtokolId})
            mycmd.Prepare()

            Try
                Dim dbRead As MySqlDataReader
                dbRead = mycmd.ExecuteReader()
                Return ReadAll(dbRead)

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return Nothing

    End Function


    Public Function GetAll() As List(Of ngPNProtokolPrava)
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `id`,
  `protokol_id`,
  `employee_id`,
  `created_by`,
  `role_by`
FROM
  `putninalog_protokol`;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection

            Dim dbRead As MySqlDataReader
            dbRead = mycmd.ExecuteReader()

            Return ReadAll(dbRead)

        End Using

        Return Nothing

    End Function

    Private Function ReadAll(ByVal dbRead As MySqlDataReader) As List(Of ngPNProtokolPrava)

        Dim _lstProtokolPrava = New List(Of ngPNProtokolPrava)

        Try

            If dbRead.HasRows Then
                While dbRead.Read()
                    Dim _response = New ngPNProtokolPrava With {
                        .Id = dbRead.GetInt32("id"),
                        .ProtokolId = dbRead.GetInt32("protokol_id"),
                        .EmployeeId = dbRead.GetInt32("employee_id"),
                        .CreatedBy = dbRead.GetInt32("created_by"),
                        .RoleBy = dbRead.GetString("role_by")
                        }

                    _lstProtokolPrava.Add(_response)
                End While
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return _lstProtokolPrava

    End Function



End Class
