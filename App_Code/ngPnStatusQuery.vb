Imports Microsoft.VisualBasic
Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ngPnStatusQuery

    Public Function FindOne(ByVal pID As Integer) As ngPnStatus
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `id`,
  `status_text`
FROM
  `putninalog_status`
WHERE `id` = @id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@id", .DbType = Data.DbType.UInt32, .Value = pID})
            mycmd.Prepare()

            Try
                Dim dbRead As MySqlDataReader
                dbRead = mycmd.ExecuteReader()
                If dbRead.HasRows Then
                    While dbRead.Read()
                        Dim _response = New ngPnStatus With {
                        .id = dbRead.GetInt32("id"),
                        .text = dbRead.GetString("status_text")
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


    Public Function GetAll() As List(Of ngPnStatus)
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `id`,
  `status_text`
FROM
  `putninalog_status`;
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

    Private Function ReadAll(ByVal dbRead As MySqlDataReader) As List(Of ngPnStatus)

        Dim _lstPnStatus = New List(Of ngPnStatus)

        Try

            If dbRead.HasRows Then
                While dbRead.Read()
                    Dim _response = New ngPnStatus With {
                        .id = dbRead.GetInt32("id"),
                        .text = dbRead.GetString("status_text")
                        }

                    _lstPnStatus.Add(_response)
                End While
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return _lstPnStatus

    End Function

End Class
