Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsSfrStrSprema

    Public Property DtList As New DataTable
    Public Property DtRow As DataRow

    Public Function getList(Optional ByVal pId As Integer = -1) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQLWhere As String = ""
            If pId <> -1 Then
                strSQLWhere = String.Format("WHERE o.id={0}", pId.ToString)
            End If


            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT o.id, o.Sifra, o.Naziv FROM sfr_strucnasprema o 
%_WHERE_% ;
]]>.Value
            strSQL = strSQL.Replace("%_WHERE_%", strSQLWhere)


            DtList.Rows.Clear()
            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

        End Using


        Return DtList

    End Function

    Public Function getIdx(ByVal pId As Integer) As Integer

        Dim idx As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If er("id") = pId Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getIdx(Optional ByVal pSifra As String = "000000") As Integer

        If pSifra.Length < 1 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If er("Sifra") = pSifra Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getId(ByVal pIdx As Integer) As String

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CStr(er("Sifra"))
                Exit For
            End If
        Next

        Return retVal
    End Function

    Public Function rowDetails(ByVal pId As Integer, ByVal pColName As String) As Object

        If pId < 0 Or pColName.Length = 0 Then Return Nothing

        For Each er In DtList.Rows
            If er("id") = pId Then
                DtRow = er
                Exit For
            End If
        Next

        If DtRow IsNot Nothing Then
            Return DtRow.Item(pColName)
        Else
            Return Nothing
        End If

    End Function

End Class
