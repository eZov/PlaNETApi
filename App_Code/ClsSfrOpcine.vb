Imports System.Data

Imports Microsoft.VisualBasic
Imports MySql.Data

Public Class ClsSfrOpcine

    Public Sub New()

    End Sub

    Public Property DtList As DataTable

    Public Function getList() As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim dsCalendar As New Data.DataSet

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = <![CDATA[ 
SELECT
  o.`id`, o.`Sifra`, CONCAT(o.`Naziv`,' (',o3.`Naziv`,')') AS Naziv
FROM `sfr_opcine` o LEFT JOIN `sfr_opcine` o2 ON o.`Visi Nivo`=o2.`Sifra` 
LEFT JOIN `sfr_opcine` o3 ON o2.`Visi Nivo`=o3.`Sifra` 
WHERE o.`OpcinaDN`=1  ORDER BY o.Naziv;
]]>.Value


            dsCalendar.Clear()

            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(dsCalendar, "dtOpcine")

        End Using

        DtList = dsCalendar.Tables("dtOpcine")

        Return DtList

    End Function

    Public Function getIdx(Optional ByVal pSfrOpcine As String = "000000") As Integer

        If pSfrOpcine.Length < 6 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If er("Sifra") = pSfrOpcine Then
                Return idx
                'Exit For
            End If
        Next

        Return -1
    End Function

    Public Function getSifra(ByVal pIdx As Integer) As String

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = er("Sifra")
                Exit For
            End If
        Next

        Return retVal
    End Function

End Class
