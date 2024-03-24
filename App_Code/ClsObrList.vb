Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

'
' Kopija ClsObracuni
'
Public Class ClsObrList

    Public Sub New()

        DtList = New DataTable
        DtList.TableName = "dtObrList"

    End Sub

    Public Property DtList As DataTable


    Public Function getList(Optional ByRef pVrsta As String = "plc_net") As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = ""

            DtList.Clear()


            Select Case pVrsta
                Case "plc_net"
                    strSQL = <![CDATA[ 
SELECT DISTINCT pol.`porezGodina` AS Id, pol.`porezGodina` AS Naziv
FROM `prz_obrlist` pol
ORDER BY pol.`porezGodina`;
]]>.Value


                Case Else

            End Select


            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

        End Using

        Return DtList

    End Function

    Public Function getIdx(Optional ByVal pSfrReport As String = "000000") As Integer

        If pSfrReport.Length = 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows

            idx += 1
            If er("Id") = pSfrReport Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getIdx(ByVal pIdReport As Integer) As Integer

        If pIdReport <= 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows

            idx += 1
            If er("id") = pIdReport Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getSifra(ByVal pIdx As Integer) As String

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = er("Id")
                Exit For
            End If
        Next

        Return retVal
    End Function

    Public Function getId(ByVal pIdx As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CInt(er("Id"))
                Exit For
            End If
        Next

        Return retVal
    End Function



End Class
