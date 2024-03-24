Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsDokumenti

    Public Sub New()

        DtList = New DataTable

    End Sub

    Public Property DtList As DataTable

    Public Function getList(Optional ByVal pDocVrsta As String = "godisnji_odmor") As DataTable

        Dim myda As MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT es.`go_id` AS Id, es.`naziv` AS Naziv, es.`filename_code` FROM `evd_doc_status` es 
WHERE es.`document`=@DocVrsta AND es.`rjesenje_status`<>0;
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
