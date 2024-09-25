Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports Newtonsoft.Json
Imports System.Globalization
Imports System.IO
Imports MySql.Data.MySqlClient
Imports System.Collections
Imports System.Collections.Generic
Imports System.Xml

Public Class NgApiOrganizacija

    Public Property lstOrgIds As New List(Of String)
    Public Property lstOrgSifras As New List(Of String)

    Public Property lstOrgTree As New List(Of NgOrganizacija)

    Public Property lstEmpIds As New List(Of String)

    Public Sub New()


    End Sub

    Public Function sqlPrepRegExpOrg(ByVal pStrOrgId As String) As String

        Dim _orgSfr As String() = pStrOrgId.Split(New Char() {","c})

        Dim _retVal As String = ""
        Dim _oneOrgSfr As String
        For Each _oneOrgSfr In _orgSfr
            Console.WriteLine(_oneOrgSfr)
            _retVal += String.Format("o.`Sifra` REGEXP '^{0}' OR ", _oneOrgSfr)
        Next

        _retVal = _retVal.TrimEnd("O", "R", " ")

        Return _retVal

    End Function

    Public Sub GetOrgSifra(ByVal pStrOrgId As String, ByVal pIntDepthLimit As Integer, ByVal pIntCurrentDepth As Integer)

        lstOrgIds.Clear()
        lstOrgSifras.Clear()

        getFullOrgTree()

        Dim _pStrOrgIds As List(Of String) = pStrOrgId.Split(",").ToList()
        For Each _pStrOrgId As String In _pStrOrgIds
            GetOrgSifras(_pStrOrgId, pIntDepthLimit, pIntCurrentDepth)
        Next


        Dim _lstOrgSifras As List(Of String) = lstOrgSifras.Distinct().ToList()
        Dim _lstOrgIds As List(Of String) = lstOrgIds.Distinct().ToList()

        Me.lstOrgSifras = _lstOrgSifras
        Me.lstOrgIds = _lstOrgIds

    End Sub

    Private Sub GetOrgSifras(ByVal pStrOrgId As String, ByVal pIntDepthLimit As Integer, ByVal pIntCurrentDepth As Integer)

        Dim _lstOrgSifras As New List(Of String)


        If pIntCurrentDepth < pIntDepthLimit Then

            _lstOrgSifras = GetSubOrgSifras1(pStrOrgId)
            For Each _orgId In _lstOrgSifras
                ' Recursively call ourselves incrementing the depth using the given folder path.
                GetOrgSifras(_orgId, pIntDepthLimit, pIntCurrentDepth + 1)
            Next
        End If


    End Sub

    Private Function GetSubOrgSifras(ByVal pStrOrgSifra As String) As List(Of String)

        Dim _lstOrgSifras As New List(Of String)


        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim dbRead As MySqlDataReader

        strSQL = <![CDATA[ 
SELECT so.`id`, so.`Sifra` 
FROM `sfr_organizacija` so
WHERE so.`Sifra Nadnivo`=@Sifra_Nadnivo
OR so.`Sifra`=@Sifra_Nadnivo;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@Sifra_Nadnivo", pStrOrgSifra)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read
                    _lstOrgSifras.Add(dbRead.GetString("Sifra"))

                    lstOrgSifras.Add(dbRead.GetString("Sifra"))
                    lstOrgIds.Add(dbRead.GetInt16("id").ToString)
                End While
            End If
            dbRead.Close()


        End Using

        Return _lstOrgSifras
    End Function

    Private Function GetSubOrgSifras1(ByVal pStrOrgSifra As String) As List(Of String)

        Dim _lstOrgSifras As New List(Of String)


        'Dim output = From m In lstOrgTree Where m.Sfr = pStrOrgSifra
        Dim output = lstOrgTree.Where(Function(x) x.Sfr = pStrOrgSifra Or x.SfrNadNivo = pStrOrgSifra)

        For Each item As NgOrganizacija In output
            _lstOrgSifras.Add(item.Sfr)

            lstOrgSifras.Add(item.Sfr)
            lstOrgIds.Add(item.Id)
        Next


        Return _lstOrgSifras
    End Function

    Public Sub GetPGEmpIds(ByVal pStrPGSifra As String)

        If pStrPGSifra.Length = 0 Then
            Exit Sub
        End If

        Dim _lstDbPGEmpIds As New List(Of String)
        Dim _lstPGEmpIds As New List(Of String)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim dbRead As MySqlDataReader

        strSQL = <![CDATA[ 
SELECT IFNULL(GROUP_CONCAT(IF(LENGTH(spg.`Opis`)>0,spg.`Opis`,'-1')),'') AS Opis
FROM `sfr_poslovnegrupe` spg
WHERE spg.`Sifra` IN(__%PGSifra%__);
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("__%PGSifra%__", pStrPGSifra)
            mycmd.CommandText = strSQL

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read
                    _lstDbPGEmpIds.Add(dbRead.GetString("Opis"))
                End While
            End If
            dbRead.Close()
        End Using

        For Each _PGEmpId As String In _lstDbPGEmpIds
            _PGEmpId = _PGEmpId.Replace(" ", "")
            Dim _lstPGEmpId As List(Of String) = _PGEmpId.Split(",").ToList()
            _lstPGEmpIds.AddRange(_lstPGEmpId)
        Next

        lstEmpIds = _lstPGEmpIds.Distinct().ToList

    End Sub

    Public Function GetPGExterni(ByVal pStrPGSifra As String) As String

        If pStrPGSifra.Length = 0 Then
            Return ""
        End If

        Dim _lstDbPGs As New List(Of String)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim dbRead As MySqlDataReader

        strSQL = <![CDATA[ 
SELECT IFNULL( IF(LENGTH(spg.`Atribut_1`) > 0, spg.`Atribut_1`,'' ), '') AS Opis
FROM `sfr_poslovnegrupe` spg
WHERE spg.`Sifra` IN(__%PGSifra%__) AND `Sifra Nadnivo` = '999000';
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("__%PGSifra%__", pStrPGSifra)
            mycmd.CommandText = strSQL


            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read
                    _lstDbPGs.Add(dbRead.GetString("Opis"))
                End While
            End If
            dbRead.Close()
        End Using


        Dim _retVal As String = ""
        For Each el In _lstDbPGs

            Select Case el.Length
                Case 0
                    ' sql:LENGTH(s.`kategorija_uposlenika`) = 0 OR
                    _retVal += " (LENGTH(s.`kategorija_uposlenika`) = 0) OR "

                Case Else
                    ' sql:s.`kategorija_uposlenika` REGEXP '^UPRAVNI' OR
                    _retVal += " (s.`kategorija_uposlenika` REGEXP '^" + el + "') OR "

            End Select

        Next

        '_retVal = _retVal.TrimEnd("O", "R", " ")

        Return _retVal

    End Function

    Public Function getFullOrgTree() As List(Of NgOrganizacija)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim dbRead As MySqlDataReader

        Dim _lstOrg As String = ""
        Dim _lstPG As String = ""


        '
        ' Lista iz tabele sys_usr_role_json lstOrg i lstPG (Name i NamePG)
        '
        strSQL = <![CDATA[ 
SELECT so.`id`, so.`Sifra`, so.`Sifra Nadnivo` 
FROM `sfr_organizacija` so
ORDER BY so.`Sifra`, so.`Sifra Nadnivo`;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)

            myconnection.Open()
            Dim mycmd As New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read
                    lstOrgTree.Add(New NgOrganizacija() With {.Id = dbRead.GetInt16("id"), .Sfr = dbRead.GetString("Sifra"), .SfrNadNivo = dbRead.GetString("Sifra Nadnivo")})
                End While
            End If
            dbRead.Close()

        End Using


        Return lstOrgTree

    End Function

    'TODO: Preseliti ovu funkciju u ApiSession
    Public Function getOrgRole(ByVal pEmail As String, Optional ByVal pIsPG As Boolean = False) As String

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim dbRead As MySqlDataReader

        Dim _lstOrg As String = ""
        Dim _lstPG As String = ""


        '
        ' Lista iz tabele sys_usr_role_json lstOrg i lstPG (Name i NamePG)
        '
        strSQL = <![CDATA[ 
SELECT u2e.employees_id, urj.Name AS org, urj.NamePG AS pg
FROM my_aspnet_membership m
INNER JOIN my_aspnet_users_to_employees u2e ON m.userId=u2e.users_id
INNER JOIN sys_usr_role_json urj ON CONVERT(m.Email USING utf8)=CONVERT(urj.Type USING utf8)
WHERE urj.Form='APIputnal'
AND m.Email=@Email
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)

            myconnection.Open()
            Dim mycmd As New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Email", pEmail)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read
                    _lstOrg = dbRead.GetString("org")
                    _lstPG = dbRead.GetString("pg")
                End While
            End If
            dbRead.Close()

        End Using


        Return If(pIsPG, _lstPG, _lstOrg)

    End Function

    Public Sub getOrgRole(ByVal pEmail As String, ByRef rLstOrg As String, ByRef rLstPG As String)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim dbRead As MySqlDataReader

        'Dim _lstOrg As String = ""
        'Dim _lstPG As String = ""


        '
        ' Lista iz tabele sys_usr_role_json lstOrg i lstPG (Name i NamePG)
        '
        strSQL = <![CDATA[ 
SELECT u2e.employees_id, urj.Name AS org, urj.NamePG AS pg
FROM my_aspnet_membership m
INNER JOIN my_aspnet_users_to_employees u2e ON m.userId=u2e.users_id
INNER JOIN sys_usr_role_json urj ON CONVERT(m.Email USING utf8)=CONVERT(urj.Type USING utf8)
WHERE urj.Form='APIputnal'
AND m.Email=@Email
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)

            myconnection.Open()
            Dim mycmd As New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Email", pEmail)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read
                    rLstOrg = dbRead.GetString("org")
                    rLstPG = dbRead.GetString("pg")
                End While
            End If
            dbRead.Close()

        End Using

    End Sub

    Public Function getList(Optional ByVal strSQLWhere As String = "", Optional numId As Boolean = False) As List(Of NgOrganizacija)

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim strSQL As String = ""

        Dim DtList As New DataTable

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection


            Select Case numId
                Case True
                    strSQL = <![CDATA[ 
Select o.id As Id, 
If (o.Sifra = o.`Sifra Nadnivo`,'0',o.`Sifra Nadnivo`) AS ParentId, 
CONCAT(o.Naziv,' (',o.`Sifra`,')') AS Naziv, 
1 As HasChild, 
0 AS Expanded, 
Opcina_1,
o.Sifra As Sfr,
o.`Sifra Nadnivo` AS SfrNadNivo, 
o.Obracun, o.Uposlenici, o.Atribut_1, o.Naziv As Naziv1, o.Opis,
Mjesto, Adresa, PTT_Broj,
BrojDjelovodni, BrojStatisticki, PDVBroj,
Banka_1, Banka_2, BankaRacun_1, BankaRacun_2,
o.id As hidId, o.Naziv As oNaziv, 0 AS Selected
From sfr_organizacija o
%_WHERE_% 
ORDER BY o.Sifra;
]]>.Value

                Case False
                    strSQL = <![CDATA[ 
Select o.Sifra As Id, 
If (o.Sifra = o.`Sifra Nadnivo`,'0',o.`Sifra Nadnivo`) AS ParentId, 
CONCAT(o.Naziv,' (',o.`Sifra`,')') AS Naziv, 
1 As HasChild, 
0 AS Expanded, 
Opcina_1,
o.Sifra As Sfr,
o.`Sifra Nadnivo` AS SfrNadNivo, 
o.Obracun, o.Uposlenici, o.Atribut_1, o.Naziv As Naziv1, o.Opis,
Mjesto, Adresa, PTT_Broj,
BrojDjelovodni, BrojStatisticki, PDVBroj,
Banka_1, Banka_2, BankaRacun_1, BankaRacun_2,
o.id As hidId, o.Naziv As oNaziv, 0 AS Selected
From sfr_organizacija o
%_WHERE_% 
ORDER BY o.Sifra;
]]>.Value

            End Select


            If strSQLWhere.Length = 0 Then
                strSQL = strSQL.Replace("%_WHERE_%", " WHERE o.Atribut_1='D'")
            Else
                strSQL = strSQL.Replace("%_WHERE_%", strSQLWhere)
            End If


            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)
        End Using


        Return JsonConvert.DeserializeObject(Of List(Of NgOrganizacija))(GetJson(DtList))

    End Function

    Public Function getPGList(Optional ByVal strSQLWhere As String = "") As List(Of NgOrganizacija)

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim strSQL As String = ""

        Dim DtList As New DataTable

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection


            strSQL = <![CDATA[ 
SELECT o.Sifra AS Id, 
IF (o.Sifra = o.`Sifra Nadnivo`,'0',o.`Sifra Nadnivo`) AS ParentId, 
CONCAT(o.Naziv,' (',o.`Sifra`,')') AS Naziv, 
1 AS HasChild, 
0 AS Expanded, 
o.Sifra AS Sfr,
o.`Sifra Nadnivo` AS SfrNadNivo,
o.id AS hidId, o.Naziv AS oNaziv, 0 AS Selected
FROM `sfr_poslovnegrupe` o
%_WHERE_% 
ORDER BY o.Sifra;
]]>.Value



            If strSQLWhere.Length = 0 Then
                strSQL = strSQL.Replace("%_WHERE_%", "")
            Else
                strSQL = strSQL.Replace("%_WHERE_%", strSQLWhere)
            End If


            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)
        End Using


        Return JsonConvert.DeserializeObject(Of List(Of NgOrganizacija))(GetJson(DtList))

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
