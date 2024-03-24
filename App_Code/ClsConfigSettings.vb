Imports Newtonsoft.Json
Imports System.Linq
Imports System.Data
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsConfigSettings

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="pKey"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetSetting(ByVal pKey As String) As String
        Dim cSetting As String = String.Empty


        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim strSQL As String = <![CDATA[ 
SELECT adm_value FROM sys_configuration WHERE adm_parameter=@adm_parameter;
]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@adm_parameter", pKey)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    cSetting = mydr("adm_value")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()
        End Using

        Return cSetting

    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="pKey"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetSettingDescription(ByVal pKey As String) As String
        Dim cSetting As String = String.Empty


        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim strSQL As String = <![CDATA[ 
SELECT adm_description FROM sys_configuration WHERE adm_parameter=@adm_parameter;
]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@adm_parameter", pKey)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    cSetting = mydr("adm_value")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()
        End Using

        Return cSetting
    End Function


    Public Shared Function GetSetting_JsOn(ByVal pAdmParameter As String) As DataTable

        Dim jsonOpis As String = Nothing
        Dim admVal As String = Nothing


        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim strSQL As String = <![CDATA[ 
SELECT sj.`Opis`, sc.`adm_value` FROM `sys_configuration` sc INNER JOIN `sys_json` sj ON sc.`adm_parameter`=sj.`Naziv` 
                                 WHERE sc.`adm_parameter`=@adm_parameter;
]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@adm_parameter", pAdmParameter)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    jsonOpis = mydr("Opis")
                    admVal = mydr("adm_value")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()
        End Using

        If jsonOpis IsNot Nothing Then
            Dim dtName As String = pAdmParameter.Substring(pAdmParameter.IndexOf(".") + 1)
            Dim dsOpis As DataSet = JsonConvert.DeserializeObject(Of DataSet)(jsonOpis)
            Dim dtOpis As DataTable = dsOpis.Tables(dtName)

            Return dtOpis
        End If

        Return New DataTable

    End Function

    Public Shared Function GetUserSetting_JsOn(ByVal pUserId As String) As DataSet

        Dim jsonOpis As String = Nothing
        Dim dsOpis As DataSet = Nothing


        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim strSQL As String = <![CDATA[ 
SELECT sj.`Opis` FROM `sys_json` sj WHERE sj.`Naziv`='user.configuration.?user_id';
]]>.Value
        strSQL = strSQL.Replace("?user_id", pUserId)

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            'mycmd.Parameters.AddWithValue("@adm_parameter", pAdmParameter)

            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    jsonOpis = mydr("Opis")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()



            If jsonOpis IsNot Nothing AndAlso jsonOpis.Length > 0 Then
                dsOpis = JsonConvert.DeserializeObject(Of DataSet)(jsonOpis)
            End If

        End Using

        Return dsOpis

    End Function
End Class
