Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Public Class NgApiSys

    Public Sub New()

    End Sub

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

    Public Shared Function getSysJSON(ByVal pName As String) As String

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String


        strSQL = <![CDATA[ 
SELECT sj.Opis 
FROM `sys_json` sj
WHERE sj.`Naziv` = @Naziv;  
    ]]>.Value

        Dim rawJson As String = ""

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@Naziv", pName)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                rawJson = dbRead.GetString("Opis")
            Loop
            dbRead.Close()


        End Using


        Return rawJson

    End Function

End Class
