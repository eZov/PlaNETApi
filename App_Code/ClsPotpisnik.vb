Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsPotpisnik

    Public Sub New()

        DtList = New DataTable

    End Sub

    Public Property DtList As DataTable

    Public Function getList(Optional ByVal pDocVrsta As String = "UGORADU") As DataTable

        Dim myda As MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT es.`PotpisnikID` AS Id, es.`Name` AS Naziv FROM `evd_doc_potpisnici` es 
WHERE es.`Kat_Ovlastenja`=@DocVrsta ORDER BY es.`Name`;
]]>.Value
            'mycmd.CommandText = strSQL.Replace("@Sifra", pSfr)
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@DocVrsta", pDocVrsta)
            mycmd.Prepare()

            DtList.Rows.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using


        Return DtList

    End Function

End Class
