Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports System.Globalization
Imports System.Security.Cryptography

Public Class NgApiMenusByRole

    Public Property MenusByRoleList As List(Of NgMenusByRole)

    ' API L1
    ' byemp
    Public Sub getMenu(ByVal pRole As String, ByVal pEmail As String)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable
        Dim jsonOpis As String = Nothing

        Dim pSubRole As String = getSubRole(pRole, pEmail)

        strSQL = <![CDATA[ 
SELECT mr.*
FROM `my_aspnet_menusbyrole` mr
WHERE
mr.`role`=@role
AND mr.`subrole`=@subrole;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@role", pRole)
            mycmd.Parameters.AddWithValue("@subrole", pSubRole)
            mycmd.Prepare()

            DtList.Clear()
            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    jsonOpis = mydr("json")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()

        End Using

        If jsonOpis IsNot Nothing Then
            Dim dtName As String = "menusbyrole"
            Dim dsOpis As DataSet = JsonConvert.DeserializeObject(Of DataSet)(jsonOpis)
            'Dim dtOpis As DataTable = dsOpis.Tables(dtName)
            DtList = dsOpis.Tables(dtName)

            Me.MenusByRoleList = JsonConvert.DeserializeObject(Of List(Of NgMenusByRole))(ngApiGeneral.GetJson(DtList))
        End If

    End Sub

    Public Function getSubRole(ByVal pRole As String, ByVal pEmail As String) As String

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable
        Dim subRole As String = Nothing


        strSQL = <![CDATA[ 
SELECT 
r.`name`, 
IFNULL(sr.`name`,'basic') AS `subrole`
FROM `my_aspnet_usersinroles` u2r
INNER JOIN `my_aspnet_roles` r ON u2r.`roleId`=r.`id`
LEFT JOIN `my_aspnet_usersinsubroles` u2sr ON u2r.`roleId`=u2sr.`roleId`
LEFT JOIN `my_aspnet_subroles` sr ON u2sr.`subroleId`=sr.`id`
LEFT JOIN `my_aspnet_membership` m ON u2r.`userId`=m.userId+1
WHERE
m.Email=@email
AND r.name=@role;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@role", pRole)
            mycmd.Parameters.AddWithValue("@email", pEmail)
            mycmd.Prepare()

            DtList.Clear()
            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    subRole = mydr("subrole")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()

        End Using

        Return subRole

    End Function

End Class
