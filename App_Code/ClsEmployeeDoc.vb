Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEmployeeDoc

    Public Sub New(ByVal pEmployeeId As Integer)

        DtList = New DataTable
        EmployeeId = pEmployeeId

    End Sub

    Public Property EmployeeId As Integer

    Public Property DtList As DataTable

    Public Function getList() As DataTable

        Dim myda As MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
   SELECT d.evd_id AS Id, d.`filename`, d.`md5`, DATE_FORMAT(ago.`datum`, "%Y.%m.%d") AS datum, 
CASE
    WHEN ago.`rjesenje_status` = -20 THEN "Plan GO"
    WHEN ago.`rjesenje_status` = -1  THEN "Rješenje GO"
    ELSE "  "
END   AS vrstaDoc
   FROM `a_evd_godisnjiodmor` ago, `evd_doc_documents` d
   WHERE ago.`doc_md5`=d.`md5` AND ago.`employeeID`= @employeeID;
]]>.Value
            'mycmd.CommandText = strSQL.Replace("@Sifra", pSfr)
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", EmployeeId)
            mycmd.Prepare()

            DtList.Rows.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using


        Return DtList

    End Function

End Class
