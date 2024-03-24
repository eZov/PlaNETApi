Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsProtokol

    Public Sub New()

        DtList = New DataTable

    End Sub

    Public Property DtList As DataTable

    Public Function getList(Optional ByVal pDocVrsta As String = "") As DataTable

        Dim myda As MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT es.`ProtokolId` AS Id, es.`Protokol_1` AS Naziv, es.`Protokol_2`  FROM `evd_doc_protokol` es 
 ORDER BY es.`Protokol_1`
]]>.Value
            'mycmd.CommandText = strSQL.Replace("@Sifra", pSfr)
            mycmd.CommandText = strSQL

            'mycmd.Parameters.AddWithValue("@DocVrsta", pDocVrsta)

            DtList.Rows.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using


        Return DtList

    End Function

    Public Function getProtokol2(ByVal pId As Integer) As String

        'Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            'idx += 1
            If er("Id") = pId Then
                retVal = er("Protokol_2")
                Exit For
            End If
        Next

        Return retVal
    End Function
End Class
