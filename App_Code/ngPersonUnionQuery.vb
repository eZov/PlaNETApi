Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Microsoft.VisualBasic

Public Class ngPersonUnionQuery


    Public Function AllPersons() As List(Of ngPersonUnion)
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
SELECT e.`EmployeeID` AS sifra,
e.FirstName AS ime,
el.`ImeRoditelja` AS `ime_roditelja`,
e.`LastName` AS prezime,
'UPOSLENICI' AS `kategorija_uposlenika`,
e.`JMB` AS `JMBG`,
IF(e.`Pol`='F','Ženski','Muški') AS `spol`,
'Aktivan' AS `status`,
CONCAT(e.FirstName,' ',e.`LastName`) AS ime_prezime,
IFNULL(s.Naziv,' ') AS radno_mjesto,
IFNULL(o.Sifra,' ') AS org_jed,
' ' AS broj_protokola
FROM `evd_employees` e
LEFT JOIN sfr_sistematizacija s ON e.RadnoMjesto=s.id
LEFT JOIN sfr_organizacija o ON e.DepartmentUP=o.id
LEFT JOIN `evd_employees_lpd` el ON e.`EmployeeID`=el.`EmployeeID`
WHERE e.`Aktivan` <> 0
UNION
SELECT 
s.`sifra` AS sifra,
s.ime,
s.`ime_roditelja`,
s.`prezime`,
s.`kategorija_uposlenika`,
s.`JMBG`,
s.`spol`,
s.`status`,
CONCAT(s.ime,' ',s.prezime) AS ime_prezime,
s.`kategorija_uposlenika` AS radno_mjesto,
'1' AS org_jed,
' ' AS broj_protokola
FROM `evd_saradnici` s
LEFT JOIN `evd_employees` e ON s.`sifra`=e.`EmployeeID`
WHERE (e.`EmployeeID` IS NULL OR e.`Aktivan` = 0)

ORDER BY prezime;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection

            Dim dbRead As MySqlDataReader
            dbRead = mycmd.ExecuteReader()

            Return ReadAll(dbRead)

        End Using

        Return Nothing

    End Function

    Public Function AllPersons(ByVal pLstOrg As String, ByVal pLstPG As String) As List(Of ngPersonUnion)
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String
        Dim mycmd As New MySqlCommand

        strSQL = <![CDATA[
SELECT e.`EmployeeID` AS sifra,
e.FirstName AS ime,
el.`ImeRoditelja` AS `ime_roditelja`,
e.`LastName` AS prezime,
'UPOSLENICI' AS `kategorija_uposlenika`,
e.`JMB` AS `JMBG`,
IF(e.`Pol`='F','Ženski','Muški') AS `spol`,
'Aktivan' AS `status`,
CONCAT(e.FirstName,' ',e.`LastName`) AS ime_prezime,
IFNULL(s.Naziv,' ') AS radno_mjesto,
IFNULL(o.Sifra,' ') AS org_jed,
' ' AS broj_protokola
FROM `evd_employees` e
LEFT JOIN sfr_sistematizacija s ON e.RadnoMjesto=s.id
LEFT JOIN sfr_organizacija o ON e.DepartmentUP=o.id
LEFT JOIN `evd_employees_lpd` el ON e.`EmployeeID`=el.`EmployeeID`
WHERE e.`Aktivan` <> 0
AND (o.`Sifra` IN(%__ListOrgJed__%) OR e.`EmployeeID` IN(%__ListPGJed__%))
UNION
SELECT 
s.`sifra` AS sifra,
s.ime,
s.`ime_roditelja`,
s.`prezime`,
s.`kategorija_uposlenika`,
s.`JMBG`,
s.`spol`,
s.`status`,
CONCAT(s.ime,' ',s.prezime) AS ime_prezime,
s.`kategorija_uposlenika` AS radno_mjesto,
'1' AS org_jed,
' ' AS broj_protokola
FROM `evd_saradnici` s
LEFT JOIN `evd_employees` e ON s.`sifra`=e.`EmployeeID`
WHERE (e.`EmployeeID` IS NULL OR e.`Aktivan` = 0)
AND (s.`sifra` IN(%__ListPGJed__%) OR IF(%__TopOrgJed__%=1,TRUE, FALSE))
ORDER BY prezime;
    ]]>.Value

        ' Nema org rolu
        '
        If pLstOrg.Length = 0 Then
            pLstOrg = "''"
            strSQL = strSQL.Replace("%__TopOrgJed__%", "-1")
        Else
            Dim pLstOrgToken As String() = pLstOrg.Split(","c)
            If pLstOrgToken.Contains("1") Then
                strSQL = strSQL.Replace("%__TopOrgJed__%", "1")
            Else
                strSQL = strSQL.Replace("%__TopOrgJed__%", "-1")
            End If
        End If

        ' Nema PG rolu
        '
        If pLstPG.Length = 0 Then
            pLstPG = "''"
        End If



        strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
        strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)

        mycmd.CommandText = strSQL

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection

            Dim dbRead As MySqlDataReader
            dbRead = mycmd.ExecuteReader()

            Return ReadAll(dbRead)

        End Using

        Return New List(Of ngPersonUnion)

    End Function

    Public Shared Function AllPersons2(ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pLstPGeXT As String) As List(Of ngPersonUnion)
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String
        Dim mycmd As New MySqlCommand

        strSQL = <![CDATA[
SELECT e.`EmployeeID` AS sifra,
e.FirstName AS ime,
el.`ImeRoditelja` AS `ime_roditelja`,
e.`LastName` AS prezime,
'UPOSLENICI' AS `kategorija_uposlenika`,
e.`JMB` AS `JMBG`,
IF(e.`Pol`='F','Ženski','Muški') AS `spol`,
'Aktivan' AS `status`,
CONCAT(e.FirstName,' ',e.`LastName`) AS ime_prezime,
IFNULL(s.Naziv,' ') AS radno_mjesto,
IFNULL(o.Sifra,' ') AS org_jed,
' ' AS broj_protokola,
1 AS uposlenik
FROM `evd_employees` e
LEFT JOIN sfr_sistematizacija s ON e.RadnoMjesto=s.id
LEFT JOIN sfr_organizacija o ON e.DepartmentUP=o.id
LEFT JOIN `evd_employees_lpd` el ON e.`EmployeeID`=el.`EmployeeID`
WHERE e.`Aktivan` <> 0
AND (%__RegExpOrgJed__% %__ListPGJed1__%)
UNION
SELECT 
s.`sifra` AS sifra,
s.ime,
s.`ime_roditelja`,
s.`prezime`,
s.`kategorija_uposlenika`,
s.`JMBG`,
s.`spol`,
s.`status`,
CONCAT(s.ime,' ',s.prezime) AS ime_prezime,
s.`kategorija_uposlenika` AS radno_mjesto,
'1' AS org_jed,
' ' AS broj_protokola,
0 AS uposlenik
FROM `evd_saradnici` s
LEFT JOIN `evd_employees` e ON s.`sifra`=e.`EmployeeID`
WHERE (e.`EmployeeID` IS NULL OR e.`Aktivan` = 0)
AND (%__ListPGJed3__% IF(%__TopOrgJed__%=1,TRUE, FALSE))
AND s.status <> 'Arhiviran'
ORDER BY prezime;
    ]]>.Value

        ' Nema org rolu
        '
        If pLstOrg.Length = 0 Then
            pLstOrg = "''"
            strSQL = strSQL.Replace("%__TopOrgJed__%", "-1")
        Else
            Dim pLstOrgToken As String() = pLstOrg.Split(","c)
            If pLstOrgToken.Contains("1") Then
                strSQL = strSQL.Replace("%__TopOrgJed__%", "1")
            Else
                strSQL = strSQL.Replace("%__TopOrgJed__%", "-1")
            End If
        End If

        ' PG podaci
        '
        Dim __ListPGJed1__ As String = ""
        Dim __ListPGJed2__ As String = ""

        If pLstPG.Length > 0 Then
            __ListPGJed1__ = String.Format("OR ( e.`EmployeeID` IN({0}) )", pLstPG)
            __ListPGJed2__ = String.Format("s.`sifra` IN({0}) OR", pLstPG)
        End If



        strSQL = strSQL.Replace("%__RegExpOrgJed__%", pLstOrg)
        strSQL = strSQL.Replace("%__ListPGJed1__%", __ListPGJed1__)
        strSQL = strSQL.Replace("%__ListPGJed2__%", __ListPGJed2__)
        strSQL = strSQL.Replace("%__ListPGJed3__%", pLstPGeXT)

        mycmd.CommandText = strSQL

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection

            Dim dbRead As MySqlDataReader
            dbRead = mycmd.ExecuteReader()

            Return ReadAll(dbRead)

        End Using

        Return New List(Of ngPersonUnion)

    End Function

    Private Shared Function ReadAll(ByVal dbRead As MySqlDataReader) As List(Of ngPersonUnion)

        Dim _lstPersons = New List(Of ngPersonUnion)

        Try

            If dbRead.HasRows Then
                While dbRead.Read()
                    Dim _response = New ngPersonUnion With {
                    .Sifra = dbRead.GetInt32("sifra"),
                    .Ime = dbRead.GetString("ime"),
                    .ImeRoditelja = dbRead.GetString("ime_roditelja"),
                    .Prezime = dbRead.GetString("prezime"),
                    .KategorijaUposlenika = dbRead.GetString("kategorija_uposlenika"),
                    .JMBG = dbRead.GetString("JMBG"),
                    .Spol = dbRead.GetString("spol"),
                    .Status = dbRead.GetString("status"),
                    .ImePrezime = dbRead.GetString("ime_prezime"),
                    .RadnoMjesto = dbRead.GetString("radno_mjesto"),
                    .OrgJed = dbRead.GetString("org_jed"),
                    .BrojProtokola = dbRead.GetString("broj_protokola"),
                    .Uposlenik = dbRead.GetInt16("uposlenik")
                    }

                    _lstPersons.Add(_response)
                End While
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return _lstPersons

    End Function

End Class
