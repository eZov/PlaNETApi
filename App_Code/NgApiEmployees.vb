Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports Newtonsoft.Json
Imports System.Globalization

''' <summary>
''' Objekat koji predstavlja evidencije prisustva. Evidencije predstavljaju jedan kalendarski mjesec.
''' </summary>
Public Class NgApiEmployees


    Public Sub New()


    End Sub

    Public Property Filter As String
    Public Property Top As Integer


    Public Function getEmployees(Optional ByVal pLstOrg As String = "", Optional ByVal pLstPG As String = "", Optional pAktivan As String = "*") As List(Of NgEmployees)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL, strSQLStart, strSQLOrgPG, strSQLOrg, strSQLPG, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT CONCAT(e.FirstName, ' ',e.LastName) AS FirstName, 
e.EmployeeID,
s.Naziv AS RadnoMjesto,
o.Sifra AS OrgJed,
' ' AS BrojProtokola
FROM evd_employees e
INNER JOIN sfr_sistematizacija s ON e.RadnoMjesto=s.id
INNER JOIN sfr_organizacija o ON e.DepartmentUP=o.id
WHERE e.FirstName LIKE CONCAT(UPPER(@filter),'%')
LIMIT @top;
    ]]>.Value

        strSQLStart = <![CDATA[ 
SELECT CONCAT(e.FirstName, ' ',e.LastName) AS FirstName, 
e.EmployeeID,
s.Naziv AS RadnoMjesto,
o.Sifra AS OrgJed,
'4110/18' AS BrojProtokola
FROM evd_employees e
INNER JOIN sfr_sistematizacija s ON e.RadnoMjesto=s.id
INNER JOIN sfr_organizacija o ON e.DepartmentUP=o.id
WHERE
    ]]>.Value

        strSQLOrgPG = <![CDATA[ 
 (e.`DepartmentUP` IN(SELECT so.`id` 
FROM `sfr_organizacija` so
WHERE so.`Sifra` IN(%__ListOrgJed__%))
OR e.`EmployeeID`  IN(%__ListPGJed__%))
AND 
]]>.Value

        strSQLOrg = <![CDATA[ 
 (e.`DepartmentUP` IN(SELECT so.`id` 
FROM `sfr_organizacija` so
WHERE so.`Sifra` IN(%__ListOrgJed__%)))
AND 
]]>.Value

        strSQLPG = <![CDATA[ 
 (e.`EmployeeID`  IN(%__ListPGJed__%))
AND
]]>.Value

        strSQLEnd = <![CDATA[ 
 %__ListAktivni__%
 e.FirstName LIKE CONCAT(UPPER(@filter),'%')
LIMIT @top
]]>.Value

        If pLstOrg.Length > 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLOrgPG + strSQLEnd
        ElseIf pLstOrg.Length > 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLEnd
        End If


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)

            strSQL = IIf(pAktivan <> "*", strSQL.Replace("%__ListAktivni__%", "  e.Aktivan <> 0 AND "), strSQL.Replace("%__ListAktivni__%", ""))

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@filter", Me.Filter)
            mycmd.Parameters.AddWithValue("@top", Me.Top)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


            Return JsonConvert.DeserializeObject(Of List(Of NgEmployees))(GetJson(DtList))

        End Using

    End Function



    Public Function getEmployeesExt(Optional ByVal pLstOrg As String = "", Optional ByVal pLstPG As String = "") As List(Of NgEmployees)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL, strSQLStart, strSQLOrgPG, strSQLOrg, strSQLPG, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
    ]]>.Value

        strSQLStart = <![CDATA[ 
SELECT CONCAT(e.FirstName, ' ',e.LastName) AS FirstName, 
e.EmployeeID,
s.Naziv AS RadnoMjesto,
o.Sifra AS OrgJed,
'4110/18' AS BrojProtokola
FROM evd_employees e
LEFT JOIN sfr_sistematizacija s ON e.RadnoMjesto=s.id
LEFT JOIN sfr_organizacija o ON e.DepartmentUP=o.id
WHERE
    ]]>.Value

        strSQLOrgPG = <![CDATA[ 
 (e.`DepartmentUP` IN(SELECT so.`id` 
FROM `sfr_organizacija` so
WHERE so.`Sifra` IN(%__ListOrgJed__%))
OR e.`EmployeeID`  IN(%__ListPGJed__%))
AND 
]]>.Value

        strSQLOrg = <![CDATA[ 
 (e.`DepartmentUP` IN(SELECT so.`id` 
FROM `sfr_organizacija` so
WHERE so.`Sifra` IN(%__ListOrgJed__%)))
AND 
]]>.Value

        strSQLPG = <![CDATA[ 
 (e.`EmployeeID`  IN(%__ListPGJed__%))
AND
]]>.Value

        strSQLEnd = <![CDATA[ 
 e.Title IN('S', 'P', 'K', 'A') AND e.Aktivan = 0 AND
 e.FirstName LIKE CONCAT(UPPER(@filter),'%')
LIMIT @top
]]>.Value

        If pLstOrg.Length > 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLOrgPG + strSQLEnd
        ElseIf pLstOrg.Length > 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLEnd
        End If


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@filter", Me.Filter)
            mycmd.Parameters.AddWithValue("@top", Me.Top)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


            Return JsonConvert.DeserializeObject(Of List(Of NgEmployees))(GetJson(DtList))

        End Using

    End Function


    Private Function GetJson(ByVal dt As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim row As Dictionary(Of String, Object) = Nothing
        For Each dr As DataRow In dt.Rows
            row = New Dictionary(Of String, Object)()
            For Each dc As DataColumn In dt.Columns
                'If dc.ColumnName.Trim() = "TAGNAME" Then
                row.Add(dc.ColumnName.Trim(), dr(dc))
                'End If
            Next
            rows.Add(row)
        Next
        Return serializer.Serialize(rows)
    End Function

    Public Shared Function getDate(ByVal pDateString As String) As Date

        Dim format As String
        Dim result As Date
        Dim provider As CultureInfo = CultureInfo.InvariantCulture

        format = "yyyy-MM-dd"
        Try
            result = Date.ParseExact(pDateString, format, provider)
            Return result
        Catch e As FormatException

        End Try

        Return Nothing

    End Function

End Class
