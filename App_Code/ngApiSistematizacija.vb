Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json


Public Class ngApiSistematizacija
    Public Sub New()

    End Sub

    Public Property List As List(Of NgSistematizacija)

    ' API Si-L1
    ' 
    Public Sub listItems()

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  o.id AS Id,
  o.Sifra,
  o.Naziv,
  o.Opis,
  o.StrSprema_1,
  o.id AS hidId,
  0 AS ParentId,
  0 AS HasChild,
  NULL AS Expanded
FROM
  sfr_sistematizacija o
ORDER BY o.Naziv;
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

            Me.List = JsonConvert.DeserializeObject(Of List(Of NgSistematizacija))(ngApiGeneral.GetJson(DtList))
        End Using

    End Sub

    ' API Si-L2
    ' 
    Public Function listItem(ByVal pId As Integer) As NgSistematizacija

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  o.id AS Id,
  o.Sifra,
  o.Naziv,
  o.Opis,
  o.StrSprema_1,
  o.id AS hidId,
  0 AS ParentId,
  0 AS HasChild,
  NULL AS Expanded
FROM
  sfr_sistematizacija o
  WHERE o.id = @id
ORDER BY o.Naziv;
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

            Me.List = JsonConvert.DeserializeObject(Of List(Of NgSistematizacija))(ngApiGeneral.GetJson(DtList))

        End Using

        If List.Count = 0 Then

            Dim _emptySistematizacija = New NgSistematizacija()
            _emptySistematizacija.Id = -1
            _emptySistematizacija.Sifra = ""
            List.Insert(0, _emptySistematizacija)
        End If


        Return List.Item(0)

    End Function

    Public Function listItem(ByVal pSifra As String) As NgSistematizacija

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  o.id AS Id,
  o.Sifra,
  o.Naziv,
  o.Opis,
  o.StrSprema_1,
  o.id AS hidId,
  0 AS ParentId,
  0 AS HasChild,
  NULL AS Expanded
FROM
  sfr_sistematizacija o
  WHERE o.Sifra=@Sifra
ORDER BY o.Naziv;
]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Sifra", pSifra)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.List = JsonConvert.DeserializeObject(Of List(Of NgSistematizacija))(ngApiGeneral.GetJson(DtList))

        End Using

        If List.Count = 0 Then

            Dim _emptySistematizacija = New NgSistematizacija()
            _emptySistematizacija.Id = -1
            _emptySistematizacija.Sifra = ""
            List.Insert(0, _emptySistematizacija)
        End If


        Return List.Item(0)

    End Function

    ' API Si-C1
    ' HACK: bez implementacije
    Public Function insItem(ByRef pInsItem As ngAktivnost) As Integer

        Return False

    End Function

    ' API Si-S1
    ' HACK: bez implementacije
    Public Function updItem(ByRef pInsItem As ngAktivnost) As Boolean

        Return False

    End Function

    ' API Si-D1
    ' HACK: bez implementacije
    Public Function delItem(ByRef pDelItem As ngAktivnost) As Boolean

        Return False

    End Function
End Class
