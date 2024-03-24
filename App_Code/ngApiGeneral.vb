Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports System.Globalization
Imports System.Security.Cryptography

Public Class ngApiGeneral

    Public Shared Function GetJson(ByVal dt As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim row As Dictionary(Of String, Object) = Nothing
        For Each dr As DataRow In dt.Rows
            row = New Dictionary(Of String, Object)()
            For Each dc As DataColumn In dt.Columns
                'If dc.ColumnName.Trim() = "TAGNAME" Then
                If dr(dc).GetType() Is GetType(Types.MySqlDateTime) Then
                    row.Add(dc.ColumnName.Trim(), dr(dc))
                Else
                    row.Add(dc.ColumnName.Trim(), dr(dc))
                End If


                'End If
            Next
            rows.Add(row)
        Next
        Return serializer.Serialize(rows)
    End Function

End Class
