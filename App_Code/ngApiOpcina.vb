Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json


Public Class ngApiOpcina
    Public Sub New()

    End Sub

    Public Property List As List(Of NgOpcina)

    ' API Op-L1
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
  o.`Visi Nivo` AS SfrNadNivo,
  CONCAT(o.Naziv, ' (', o3.Naziv, ')') AS Naziv,
  o.SifraOpcine,
  o.OpcinaDN,
  o.Sifra_DP AS SifraDP,
  o.PTT_Broj AS PTTBroj,
  o.Mjesto,
  IF(o.MjestoYN='Y',-1,0) AS MjestoYN
FROM
  sfr_opcine o
  LEFT JOIN sfr_opcine o2
    ON o.`Visi Nivo` = o2.Sifra
  LEFT JOIN sfr_opcine o3
    ON o2.`Visi Nivo` = o3.Sifra
WHERE o.OpcinaDN = 1
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

            Me.List = JsonConvert.DeserializeObject(Of List(Of NgOpcina))(ngApiGeneral.GetJson(DtList))
        End Using

    End Sub

    ' API Op-L2
    ' 
    Public Function listItem(ByVal pId As Integer) As NgOpcina

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  o.id AS Id,
  o.Sifra,
  o.`Visi Nivo` AS SfrNadNivo,
  CONCAT(o.Naziv, ' (', o3.Naziv, ')') AS Naziv,
  o.SifraOpcine,
  o.OpcinaDN,
  o.Sifra_DP AS SifraDP,
  o.PTT_Broj AS PTTBroj,
  o.Mjesto,
  IF(o.MjestoYN='Y',-1,0) AS MjestoYN
FROM
  sfr_opcine o
  LEFT JOIN sfr_opcine o2
    ON o.`Visi Nivo` = o2.Sifra
  LEFT JOIN sfr_opcine o3
    ON o2.`Visi Nivo` = o3.Sifra
WHERE o.OpcinaDN = 1
AND o.id=@id
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

            Me.List = JsonConvert.DeserializeObject(Of List(Of NgOpcina))(ngApiGeneral.GetJson(DtList))

        End Using

        If List.Count = 0 Then

            Dim _emptySistematizacija = New NgOpcina()
            _emptySistematizacija.Id = -1
            _emptySistematizacija.Sifra = ""
            List.Insert(0, _emptySistematizacija)
        End If


        Return List.Item(0)

    End Function

    Public Function listItem(ByVal pSifra As String) As NgOpcina

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  o.id AS Id,
  o.Sifra,
  o.`Visi Nivo` AS SfrNadNivo,
  CONCAT(o.Naziv, ' (', o3.Naziv, ')') AS Naziv,
  o.SifraOpcine,
  o.OpcinaDN,
  o.Sifra_DP AS SifraDP,
  o.PTT_Broj AS PTTBroj,
  o.Mjesto,
  IF(o.MjestoYN='Y',-1,0) AS MjestoYN
FROM
  sfr_opcine o
  LEFT JOIN sfr_opcine o2
    ON o.`Visi Nivo` = o2.Sifra
  LEFT JOIN sfr_opcine o3
    ON o2.`Visi Nivo` = o3.Sifra
WHERE o.OpcinaDN = 1
AND o.Sifra=@Sifra
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

            Me.List = JsonConvert.DeserializeObject(Of List(Of NgOpcina))(ngApiGeneral.GetJson(DtList))

        End Using

        If List.Count = 0 Then

            Dim _emptySistematizacija = New NgOpcina()
            _emptySistematizacija.Id = -1
            _emptySistematizacija.Sifra = ""
            List.Insert(0, _emptySistematizacija)
        End If


        Return List.Item(0)

    End Function

    ' API Op-C1
    ' HACK: bez implementacije
    Public Function insItem(ByRef pInsItem As ngAktivnost) As Integer

        Return False

    End Function

    ' API Op-S1
    ' HACK: bez implementacije
    Public Function updItem(ByRef pInsItem As ngAktivnost) As Boolean

        Return False

    End Function

    ' API Op-D1
    ' HACK: bez implementacije
    Public Function delItem(ByRef pDelItem As ngAktivnost) As Boolean

        Return False

    End Function
End Class
