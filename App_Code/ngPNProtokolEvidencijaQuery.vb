Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Microsoft.VisualBasic

Public Class ngPNProtokolEvidencijaQuery

    Public Function FindOne(ByVal pID As Integer) As ngPNProtokolEvidencija
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `pn_id`,
  `pn_protokol`,
  `pn_redbr`,
  `pn_mjesec`,
  `pn_godina`,
  `employee_id`,
  `dummytimestamp`
FROM
  `putninalog_evidencija`
WHERE `pn_id` = @pn_id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@pn_id", .DbType = Data.DbType.UInt32, .Value = pID})
            mycmd.Prepare()

            Try
                Dim dbRead As MySqlDataReader
                dbRead = mycmd.ExecuteReader()
                If dbRead.HasRows Then
                    While dbRead.Read()
                        Dim _response = New ngPNProtokolEvidencija With {
                        .PnId = dbRead.GetInt32("pn_id"),
                        .PnProtokol = dbRead.GetString("pn_protokol"),
                        .PnRedBr = dbRead.GetInt32("pn_redbr"),
                        .PnMjesec = dbRead.GetInt32("pn_mjesec"),
                        .PnGodina = dbRead.GetInt32("pn_godina"),
                        .EmployeeId = dbRead.GetInt32("employee_id")
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


    Public Function GetAll() As List(Of ngPNProtokolEvidencija)
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `pn_id`,
  `pn_protokol`,
  `pn_redbr`,
  `pn_mjesec`,
  `pn_godina`,
  `employee_id`,
  `dummytimestamp`
FROM
  `putninalog_evidencija`;
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

    Public Function GetByProtokol(ByVal pProtokol As String) As List(Of ngPNProtokolEvidencija)
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `pn_id`,
  `pn_protokol`,
  `pn_redbr`,
  `pn_mjesec`,
  `pn_godina`,
  `employee_id`,
  `dummytimestamp`
FROM
  `putninalog_evidencija`
WHERE `pn_protokol` = @pn_protokol
ORDER BY `pn_godina`,`pn_mjesec`,`pn_redbr`;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@pn_protokol", .DbType = Data.DbType.String, .Value = pProtokol})
            mycmd.Prepare()

            Dim dbRead As MySqlDataReader
            dbRead = mycmd.ExecuteReader()

            Return ReadAll(dbRead)

        End Using

        Return Nothing

    End Function

    Private Function ReadAll(ByVal dbRead As MySqlDataReader) As List(Of ngPNProtokolEvidencija)

        Dim _lstProtokolEvidencija = New List(Of ngPNProtokolEvidencija)

        Try

            If dbRead.HasRows Then
                While dbRead.Read()
                    Dim _response = New ngPNProtokolEvidencija With {
                        .PnId = dbRead.GetInt32("pn_id"),
                        .PnProtokol = dbRead.GetString("pn_protokol"),
                        .PnRedBr = dbRead.GetInt32("pn_redbr"),
                        .PnMjesec = dbRead.GetInt32("pn_mjesec"),
                        .PnGodina = dbRead.GetInt32("pn_godina"),
                        .EmployeeId = dbRead.GetInt32("employee_id")
                        }

                    _lstProtokolEvidencija.Add(_response)
                End While
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return _lstProtokolEvidencija

    End Function


End Class
