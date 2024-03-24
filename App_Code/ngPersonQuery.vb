Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Microsoft.VisualBasic

Public Class ngPersonQuery


    Public Function FindOne(ByVal pID As Integer) As ngPerson
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `sifra`,
  `ime`,
  `ime_roditelja`,
  `prezime`,
  `kategorija_uposlenika`,
  `JMBG`,
  `spol`,
  `status`,
   dummytimestamp
FROM
  `evd_saradnici`
WHERE `sifra` = @sifra;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@sifra", .DbType = Data.DbType.UInt32, .Value = pID})
            mycmd.Prepare()

            Try
                Dim dbRead As MySqlDataReader
                dbRead = mycmd.ExecuteReader()
                If dbRead.HasRows Then
                    While dbRead.Read()
                        Dim _response = New ngPerson With {
                        .Sifra = dbRead.GetInt32("sifra"),
                        .Ime = dbRead.GetString("ime"),
                        .ImeRoditelja = dbRead.GetString("ime_roditelja"),
                        .Prezime = dbRead.GetString("prezime"),
                        .KategorijaUposlenika = dbRead.GetString("kategorija_uposlenika"),
                        .JMBG = dbRead.GetString("JMBG"),
                        .Spol = dbRead.GetString("spol"),
                        .Status = dbRead.GetString("status")
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


    Public Function AllPersons() As List(Of ngPerson)
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT
  `sifra`,
  `ime`,
  `ime_roditelja`,
  `prezime`,
  `kategorija_uposlenika`,
  `JMBG`,
  `spol`,
  `status`
FROM
  `evd_saradnici`;
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

    Private Function ReadAll(ByVal dbRead As MySqlDataReader) As List(Of ngPerson)

        Dim _lstPersons = New List(Of ngPerson)

        Try

            If dbRead.HasRows Then
                While dbRead.Read()
                    Dim _response = New ngPerson With {
                    .Sifra = dbRead.GetInt32("sifra"),
                    .Ime = dbRead.GetString("ime"),
                    .ImeRoditelja = dbRead.GetString("ime_roditelja"),
                    .Prezime = dbRead.GetString("prezime"),
                    .KategorijaUposlenika = dbRead.GetString("kategorija_uposlenika"),
                    .JMBG = dbRead.GetString("JMBG"),
                    .Spol = dbRead.GetString("spol"),
                    .Status = dbRead.GetString("status")
                    }

                    _lstPersons.Add(_response)
                End While
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return _lstPersons

    End Function

End Class
