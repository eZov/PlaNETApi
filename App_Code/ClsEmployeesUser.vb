Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEmployeesUser

    Public Sub New()


        DtList = New DataTable

    End Sub


    Public Property DtList As DataTable
    Public Property DrDetails As DataRow

    Public Function getList(ByVal pSfr As String) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
CALL EMP_create_list(945);
]]>.Value
            mycmd.CommandText = strSQL.Replace("@Sifra", pSfr)



            'mycmd.Parameters.AddWithValue("@row_number", 0)

            DtList.Rows.Clear()
            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using



        Return DtList

    End Function

End Class
