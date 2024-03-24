Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports System.Globalization
Imports System.Security.Cryptography

Public Class ngApiAktivnost

    Public Sub New()

    End Sub

    Public Property List As List(Of ngAktivnost)

    ' API A-L1
    ' 
    Public Sub listItems()

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  id
, sifra
, aktivnost
, short_code
, dummytimestamp
FROM
  sfr_aktivnost
 ORDER BY aktivnost;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            mycmd.Connection = myconnection

            'strSQL = strSQL.Replace("%__Days__%", pDays)
            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

            Me.List = JsonConvert.DeserializeObject(Of List(Of ngAktivnost))(ngApiGeneral.GetJson(DtList))
        End Using

    End Sub

    ' API A-L2
    ' 
    Public Function listItem(ByVal pId As Integer) As ngAktivnost

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  id
, sifra
, aktivnost
, short_code
, dummytimestamp
FROM
  sfr_aktivnost
WHERE id=@id;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@id", pId)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.List = JsonConvert.DeserializeObject(Of List(Of ngAktivnost))(ngApiGeneral.GetJson(DtList))

        End Using

        If List.Count = 0 Then

            Dim _emptyAktivnost = New ngAktivnost()
            _emptyAktivnost.id = -1
            _emptyAktivnost.sifra = ""
            _emptyAktivnost.aktivnost = ""
            _emptyAktivnost.short_code = ""

            List.Insert(0, _emptyAktivnost)
        End If


        Return List.Item(0)

    End Function

    ' API A-C1
    ' 
    Public Function insItem(ByRef pInsItem As ngAktivnost) As Integer

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1
        Dim _LastInsertedId As Integer = -1

        strSQL = <![CDATA[
INSERT INTO sfr_aktivnost (sifra, aktivnost, short_code)
VALUES
  (
    @sifra, @aktivnost, @short_code
  );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@sifra", pInsItem.sifra)
            mycmd.Parameters.AddWithValue("@aktivnost", pInsItem.aktivnost)
            mycmd.Parameters.AddWithValue("@short_code", pInsItem.short_code)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    _LastInsertedId = mycmd.LastInsertedId
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, _LastInsertedId, -1)

    End Function

    ' API A-S1
    ' 
    Public Function updItem(ByRef pInsItem As ngAktivnost) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
UPDATE
  sfr_aktivnost
SET
  sifra = @sifra
  , aktivnost = @aktivnost
  , short_code = @short_code
WHERE id = @id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@id", pInsItem.id)
            mycmd.Parameters.AddWithValue("@sifra", pInsItem.sifra)
            mycmd.Parameters.AddWithValue("@aktivnost", pInsItem.aktivnost)
            mycmd.Parameters.AddWithValue("@short_code", pInsItem.short_code)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function

    ' API A-D1
    ' 
    Public Function delItem(ByRef pDelItem As ngAktivnost) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
DELETE
FROM
  sfr_aktivnost
WHERE id = @id;
    ]]>.Value




        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@id", pDelItem.id)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function

End Class
