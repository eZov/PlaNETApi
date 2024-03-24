Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Microsoft.VisualBasic

Public Class ngProtokolQuery


    Public Function FindOne(ByVal pId As Integer) As ngProtokol
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `id`,
  `red_br`,
  `protokol`,
  `godina`,
  `org_jed`,
  `opis`
FROM
  `sfr_protokol`
WHERE `id` = @id;
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
                        Dim _response = New ngProtokol With {
                        .Id = dbRead.GetInt32("id"),
                        .RedBr = dbRead.GetInt32("red_br"),
                        .Protokol = dbRead.GetString("protokol"),
                        .Godina = dbRead.GetInt32("godina"),
                        .OrgJed = dbRead.GetString("org_jed"),
                        .Opis = dbRead.GetString("opis")
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


    Public Function GetAll() As List(Of ngProtokol)
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `id`,
  `red_br`,
  `protokol`,
  `godina`,
  `org_jed`,
  `opis`
FROM
  `sfr_protokol`;
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

    Public Function GetByEmp(ByVal pEmpId As Integer) As List(Of ngProtokol)
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  p.`id`,
  p.`red_br`,
  p.`protokol`,
  p.`godina`,
  p.`org_jed`,
  p.`opis`
FROM
  `sfr_protokol` p
INNER JOIN `putninalog_protokol`   pp ON p.`id`=pp.`protokol_id`
WHERE pp.`employee_id` = @employee_id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@employee_id", .DbType = Data.DbType.String, .Value = pEmpId})
            mycmd.Prepare()

            Dim dbRead As MySqlDataReader
            dbRead = mycmd.ExecuteReader()

            Return ReadAll(dbRead)

        End Using

        Return Nothing

    End Function

    Private Function ReadAll(ByVal dbRead As MySqlDataReader) As List(Of ngProtokol)

        Dim _lstProtokol = New List(Of ngProtokol)

        Try

            If dbRead.HasRows Then
                While dbRead.Read()
                    Dim _response = New ngProtokol With {
                        .Id = dbRead.GetInt32("id"),
                        .RedBr = dbRead.GetInt32("red_br"),
                        .Protokol = dbRead.GetString("protokol"),
                        .Godina = dbRead.GetInt32("godina"),
                        .OrgJed = dbRead.GetString("org_jed"),
                        .Opis = dbRead.GetString("opis")
                        }

                    _lstProtokol.Add(_response)
                End While
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return _lstProtokol

    End Function



End Class
