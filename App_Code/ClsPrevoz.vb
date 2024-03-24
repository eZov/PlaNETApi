Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsPrevoz

    Public Property DtList As DataTable

    Public Function getList() As DataTable

        DtList = ClsConfigSettings.GetSetting_JsOn("obracun.prevoz")

        Return DtList

    End Function

    Public Function getIdx(Optional ByVal pValue As String = "000000") As Integer

        Dim idx As Integer = -1

        For Each el As DataRow In DtList.Rows
            idx += 1
            If el.Item("value") = pValue Then
                Return idx
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
                retVal = er("value")
                Return retVal
            End If
        Next

        Return ""
    End Function
End Class
